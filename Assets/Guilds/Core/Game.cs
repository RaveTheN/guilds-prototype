// Guilds.Core — stato di una partita e risoluzione delle azioni (famiglia di preset P01).
// Flusso: dichiarazione → finestra di risposta (pila) → risoluzione con ricontrollo dei bersagli → combattimento → morte → vittoria.
// Ogni comando restituisce null se eseguito, altrimenti il motivo per cui è vietato. Le stesse funzioni Why*/…For
// governano sia la validazione sia le evidenziazioni dell'interfaccia.
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Guilds.Core
{
    public class Rng
    {
        public ulong State;
        public Rng() { }
        public Rng(int seed) { State = (ulong)seed * 2654435761UL + 0x9E3779B97F4A7C15UL; }
        public int Next(int n) { State ^= State << 13; State ^= State >> 7; State ^= State << 17; return (int)(State % (ulong)n); }
        public void Shuffle<T>(IList<T> l) { for (int i = l.Count - 1; i > 0; i--) { int j = Next(i + 1); var t = l[i]; l[i] = l[j]; l[j] = t; } }
    }

    public class Modifier { public string Stat; public int Amount; public int ExpiresTurn; public string Source; } // ExpiresTurn -1 = permanente
    public class Unit
    {
        public int Id, Owner; public string CardId; public Cell Pos; public bool Placed;
        public int Damage, Venom, AbilityUsedTurn; public bool Tapped, HasAttacked, MovedThisTurn;
        public List<Modifier> Mods = new List<Modifier>(); public List<string> Items = new List<string>();
    }
    public class Permanent { public string CardId; public bool Tapped; }
    public class PlayerState
    {
        public string DeckId;
        public List<string> Deck = new List<string>(), Hand = new List<string>(), Discard = new List<string>();
        public List<Permanent> Resources = new List<Permanent>(); // carte risorsa in gioco: si tappano per pagare
        public Dictionary<Guild, int> Pool = new Dictionary<Guild, int>(); // risorse sciolte (solo effetti e debug): perse a fine turno
        public int ResourcesPlayedThisTurn;
        public List<string> Drawn = new List<string>(), Played = new List<string>(); // per il resoconto
    }
    public class StackItem
    {
        public string Kind; // move | card
        public int Controller, UnitId = -1, TargetId = -1; public string CardId;
        public Cell From, TargetPos, Dest; public bool HasDest;
        public Dictionary<int, Cell> Positions = new Dictionary<int, Cell>(); // posizioni alla dichiarazione (RIC-ANNULLA)
    }
    public class LogEntry { public int Turn, Player; public string Kind, Text; public bool Auto, Debug; }

    public class Game
    {
        public Preset Preset; public MapDef Map; public int Seed;
        public Dictionary<string, CardDef> Cards = new Dictionary<string, CardDef>(); // copia delle versioni con cui la partita è iniziata
        public List<DeckDef> Decks = new List<DeckDef>();
        public PlayerState[] Players; public List<Unit> Units = new List<Unit>();
        public int Turn, Active, FirstPlayer, Priority, SetupPlayer, Winner = -1; // Winner -2 = entrambi sconfitti
        public Phase Phase; public string WinReason;
        public List<StackItem> Stack = new List<StackItem>();
        public List<LogEntry> Log = new List<LogEntry>();
        public Rng Rng; public bool DebugTouched;
        [JsonIgnore] Board board;
        [JsonIgnore] public Board Board => board ?? (board = new Board(Map));

        // ---------- avvio ----------
        public static Game Start(Preset preset, MapDef map, IEnumerable<CardDef> cards, DeckDef d0, DeckDef d1, int seed)
        {
            var g = new Game { Preset = preset, Map = map, Seed = seed, Rng = new Rng(seed), Decks = new List<DeckDef> { d0, d1 } };
            foreach (var c in cards) { g.Cards[c.Id] = c; if (c.Ability != null) g.Cards[c.Ability.Id] = c.Ability; }
            g.Players = new[] { d0, d1 }.Select(d => new PlayerState { DeckId = d.Id, Deck = d.Expand().ToList() }).ToArray();
            for (int p = 0; p < 2; p++)
            {
                g.Rng.Shuffle(g.Players[p].Deck);
                g.Units.Add(new Unit { Id = p, Owner = p, CardId = g.Decks[p].Guildmaster });
                foreach (Guild gl in Enum.GetValues(typeof(Guild))) g.Players[p].Pool[gl] = 0;
            }
            g.FirstPlayer = g.Rng.Next(2); g.Active = g.FirstPlayer;
            g.Say(-1, "setup", $"Partita iniziata. Preset {preset.Id}, mappa {map.Id}, seed {seed}. Primo giocatore sorteggiato: G{g.FirstPlayer + 1}.", true);
            for (int p = 0; p < 2; p++) for (int i = 0; i < preset.N("handStart"); i++) g.Draw(p, true);
            g.SetupPlayer = g.FirstPlayer;
            if (preset.C("mulligan") == "iniziale") g.Phase = Phase.Mulligan; else g.AfterMulligan();
            return g;
        }

        void AfterMulligan()
        {
            SetupPlayer = FirstPlayer;
            if (Preset.C("start") == "S-A")
            {
                for (int p = 0; p < 2; p++) Place(p, Board.StartCells(p, "S-A")[0]);
                BeginTurn();
            }
            else Phase = Phase.Setup;
        }

        /// <summary>Mulligan iniziale (N-06, da BL-48): le carte scelte vanno in fondo al mazzo, se ne pescano altrettante meno una. Lista vuota = tieni la mano.</summary>
        public string Mulligan(int player, List<int> handIndexes)
        {
            if (Phase != Phase.Mulligan || player != SetupPlayer) return "Non è il tuo momento di decidere il Mulligan.";
            var p = Players[player];
            var idx = handIndexes.Distinct().OrderByDescending(i => i).ToList();
            if (idx.Any(i => i < 0 || i >= p.Hand.Count)) return "Carta non valida.";
            foreach (var i in idx) { p.Deck.Add(p.Hand[i]); p.Drawn.Remove(p.Hand[i]); p.Hand.RemoveAt(i); }
            for (int i = 0; i < idx.Count - 1; i++) Draw(player, true);
            Say(player, "mulligan", idx.Count == 0 ? $"G{player + 1} tiene la mano." : $"G{player + 1} fa Mulligan: {idx.Count} carte in fondo al mazzo, {idx.Count - 1} pescate.");
            if (player == FirstPlayer) SetupPlayer = 1 - player; else AfterMulligan();
            return null;
        }

        // ---------- letture ----------
        public CardDef Def(string id) => Cards[id];
        public CardDef Def(Unit u) => Cards[u.CardId];
        public Unit Gm(int player) => Units[player];
        public Unit UnitAt(Cell c) => Units.FirstOrDefault(u => u.Placed && u.Pos.Equals(c));
        public int Actor => Phase == Phase.Setup || Phase == Phase.Mulligan ? SetupPlayer : Phase == Phase.Response ? Priority : Phase == Phase.Over ? -1 : Active;
        int Mod(Unit u, string stat) => u.Mods.Where(m => m.Stat == stat).Sum(m => m.Amount);
        public int Attack(Unit u) => Math.Max(0, Def(u).Attack + Mod(u, "attack"));
        public int MaxHealth(Unit u) => Def(u).Health + Mod(u, "health");
        public int Health(Unit u) => MaxHealth(u) - u.Damage;
        public int MoveMax(Unit u) => Math.Max(1, Preset.N("moveMax") + Mod(u, "move"));
        public bool Rooted(Unit u) => u.Mods.Any(m => m.Stat == "root");
        int Adjacency => int.Parse(Preset.C("adjacency"));
        bool EnemyAt(int player, Cell c) { var u = UnitAt(c); return u != null && u.Owner != player; }
        public bool Sees(Unit from, Cell target) => Board.Los(from.Pos, target, c => EnemyAt(from.Owner, c), Preset.C("losLine"));
        public bool InAttackRange(Unit a, Unit d) => Def(a).Range == "ranged" ? Sees(a, d.Pos) : Board.Adjacent(a.Pos, d.Pos, Adjacency);

        // ---------- setup ----------
        public List<Cell> StartOptions(int player) =>
            Phase != Phase.Setup || player != SetupPlayer ? new List<Cell>() : Board.StartCells(player, "S-B").Where(c => UnitAt(c) == null).ToList();

        public string ChooseStart(int player, Cell c)
        {
            if (Phase != Phase.Setup || player != SetupPlayer) return "Non è il tuo momento di scegliere la partenza.";
            if (!StartOptions(player).Contains(c)) return "La partenza deve essere una casella libera adiacente alla tua Torre (F4b-16/17).";
            Place(player, c);
            if (Units.All(u => u.Placed)) BeginTurn(); else SetupPlayer = 1 - player;
            return null;
        }
        void Place(int player, Cell c) { var u = Gm(player); u.Pos = c; u.Placed = true; Say(player, "setup", $"{Def(u).Name} parte in {c}."); }

        // ---------- Torri attive (varianti) ----------
        public int[] TowerDamage = new int[2];
        public int TowerHp(int player) => Preset.N("towerHpMax") - TowerDamage[player];
        public bool TowerActive(int player) => Board.Tower(player) != null && (Preset.C("towerHp") == "no" || TowerHp(player) > 0);
        public bool NearTower(Cell c, int player) => Board.Adjacent(c, Board.Tower(player).Value, 8);
        /// <summary>when: «Enter» (l'unità è appena arrivata nella casella) oppure «Start» (inizio del suo turno).</summary>
        void TowerEffect(Unit u, string when)
        {
            for (int i = 0; i < 2; i++)
            {
                if (!TowerActive(i) || !NearTower(u.Pos, i)) continue;
                bool own = i == u.Owner; string how = when == "Enter" ? "entra accanto" : "inizia il turno accanto";
                string key = (own ? "towerHeal" : "towerDmg") + when;
                if (Preset.C(key) != "si") continue;
                int x = Preset.N(key + "X");
                if (own)
                {
                    int n = Math.Min(u.Damage, x); if (n == 0) continue;
                    u.Damage -= n; Say(u.Owner, "tower", $"{Def(u).Name} {how} alla propria Torre: cura {n} (Salute {Health(u)}/{MaxHealth(u)}).", true);
                }
                else
                {
                    u.Damage += x;
                    Say(i, "tower", $"{Def(u).Name} {how} alla Torre nemica: subisce {x} danni (Salute {Health(u)}/{MaxHealth(u)}).", true);
                }
            }
            CheckDeaths();
        }
        /// <summary>H26: la Torre nemica è un bersaglio del combattimento automatico solo se ha punti vita e non c'è un'unità nemica da colpire.</summary>
        bool CanHitTower(Unit a)
        {
            int e = 1 - a.Owner;
            if (Preset.C("towerHp") != "si" || !TowerActive(e)) return false;
            var t = Board.Tower(e).Value;
            return Def(a).Range == "ranged" ? Sees(a, t) : Board.Adjacent(a.Pos, t, Adjacency);
        }

        // ---------- turno ----------
        void BeginTurn()
        {
            Turn++; Phase = Phase.Main;
            var p = Players[Active]; p.ResourcesPlayedThisTurn = 0;
            foreach (var u in Units.Where(u => u.Owner == Active)) { u.Tapped = false; u.HasAttacked = false; u.MovedThisTurn = false; }
            foreach (var r in p.Resources) r.Tapped = false;
            Say(Active, "turn", $"— Turno {Turn}: G{Active + 1}. Le sue unità e le sue {p.Resources.Count} carte risorsa si stappano.", true);
            foreach (var u in Units.Where(u => u.Owner == Active && u.Venom > 0))
            {   // F1-65: 1 danno a inizio turno, indipendentemente dal numero di segnalini
                u.Damage++; Say(Active, "venom", $"{Def(u).Name} è avvelenato: subisce 1 danno (Salute {Health(u)}/{MaxHealth(u)}).", true);
                if (Preset.C("venom") == "cala") { u.Venom--; Say(Active, "venom", $"{Def(u).Name} perde un segnalino veleno (ne restano {u.Venom}).", true); }
            }
            CheckDeaths(); if (Phase == Phase.Over) return;
            foreach (var u in Units.Where(u => u.Owner == Active && u.Placed).ToList()) TowerEffect(u, "Start");
            if (Phase == Phase.Over) return;
            if (Turn == 1 && Preset.N("skipFirstDraw") == 1) Say(Active, "draw", "Il primo giocatore salta la prima pesca (F1-52).", true);
            else for (int i = 0; i < Preset.N("drawPerTurn") && Phase != Phase.Over; i++) Draw(Active, false);
        }

        void Draw(int player, bool opening)
        {
            var p = Players[player];
            if (p.Deck.Count == 0)
            {
                if (Preset.C("deckOut") == "perde") Win(1 - player, $"G{player + 1} deve pescare da un mazzo vuoto");
                else Say(player, "draw", "Mazzo vuoto: pesca saltata (F1-55).", true);
                return;
            }
            var c = p.Deck[0]; p.Deck.RemoveAt(0); p.Hand.Add(c); p.Drawn.Add(c);
            if (!opening) Say(player, "draw", $"G{player + 1} pesca una carta.", true);
        }

        public string WhyNotEndTurn(int player)
        {
            if (Phase != Phase.Main || player != Active) return "Puoi finire il turno solo nella tua fase principale, con la pila vuota.";
            var gm = Gm(player);
            if (Preset.C("mandatoryMove") == "si" && !gm.MovedThisTurn && WhyNotMove(player, gm.Id) == null)
                return "Movimento obbligatorio (F3-03): devi muovere il tuo Guildmaster prima di finire il turno.";
            return null;
        }

        public string EndTurn(int player)
        {
            var why = WhyNotEndTurn(player); if (why != null) return why;
            // finestra di fine turno (decisione del 15 nov): prima che il turno finisca si può sempre rispondere con istantanee e abilità
            Say(player, "turn", $"G{player + 1} dichiara la fine del turno: finestra di risposta di fine turno.");
            Stack.Add(new StackItem { Kind = "end", Controller = player });
            Phase = Phase.Response; Priority = 1 - player;
            return null;
        }

        string FinishTurn(int player)
        {
            var gm = Gm(player);
            if (Preset.C("mandatoryMove") == "si" && !gm.MovedThisTurn) Say(player, "rule", $"{Def(gm).Name} non poteva muoversi: l'obbligo di movimento decade (decisione del 14 nov).", true);
            foreach (var g in Players[player].Pool.Keys.ToList()) Players[player].Pool[g] = 0;
            foreach (var u in Units) u.Mods.RemoveAll(m => m.ExpiresTurn == Turn);
            Say(player, "turn", $"G{player + 1} termina il turno. Risorse non spese perse (F1-45).");
            CheckDeaths(); if (Phase == Phase.Over) return null;
            if (Players[player].Hand.Count > Preset.N("handLimit")) { Phase = Phase.Discard; return null; }
            Active = 1 - Active; BeginTurn(); return null;
        }

        public string DiscardCard(int player, int handIndex)
        {
            if (Phase != Phase.Discard || player != Active) return "Non devi scartare ora.";
            var p = Players[player]; var c = p.Hand[handIndex];
            p.Hand.RemoveAt(handIndex); p.Discard.Add(c);
            Say(player, "discard", $"G{player + 1} scarta {Def(c).Name} (limite di mano {Preset.N("handLimit")}).");
            if (p.Hand.Count <= Preset.N("handLimit")) { Active = 1 - Active; BeginTurn(); }
            return null;
        }

        // ---------- movimento ----------
        /// <summary>Destinazioni legali per geometria (non controlla tap, turno o Root).</summary>
        public Dictionary<Cell, List<Cell>> MovesFor(int unitId, int? maxSteps = null)
        {
            var u = Units[unitId];
            var m = Board.Reach(u.Pos, maxSteps ?? MoveMax(u), c => !EnemyAt(u.Owner, c), c => UnitAt(c) == null);
            // variante: restare fermi è una mossa (solo nel proprio turno, non per le carte che spostano né per lo spostamento in risposta)
            if (maxSteps == null && Phase == Phase.Main && Preset.C("stayMove") == "si") m[u.Pos] = new List<Cell>();
            return m;
        }

        // ---------- attacco senza cambiare casella (variante) ----------
        public string WhyNotAttack(int player, int unitId)
        {
            var u = Units[unitId];
            if (Preset.C("attackNoMove") != "si") return "In questo preset si combatte solo dopo un movimento.";
            if (player != Actor || Phase != Phase.Main) return "Si attacca solo nella propria fase principale.";
            if (u.Owner != player || !u.Placed) return "Puoi attaccare solo con le tue unità.";
            if (u.Tapped) return $"{Def(u).Name} è tappato: anche l'attacco senza cambiare casella si fa tappando la carta.";
            if (u.HasAttacked) return $"{Def(u).Name} ha già attaccato in questo turno (F1-27).";
            if (!Units.Any(d => d.Owner != player && d.Placed && InAttackRange(u, d)) && !CanHitTower(u)) return $"Nessun nemico {(Def(u).Range == "ranged" ? "in linea di tiro" : "a contatto")}.";
            return null;
        }
        public string DeclareAttack(int player, int unitId)
        {
            var why = WhyNotAttack(player, unitId); if (why != null) return why;
            var u = Units[unitId];
            // decisione del 15 nov: il tap È il movimento, anche se la pedina non cambia casella. Ordine: tap > movimento > attacco.
            u.Tapped = true; u.MovedThisTurn = true;
            Say(player, "attack", $"{Def(u).Name} si tappa e attacca restando in {u.Pos}.");
            var item = new StackItem { Kind = "attack", Controller = player, UnitId = unitId, From = u.Pos };
            foreach (var x in Units) item.Positions[x.Id] = x.Pos;
            Stack.Add(item); Phase = Phase.Response; Priority = 1 - player;
            return null;
        }

        public string WhyNotMove(int player, int unitId)
        {
            var u = Units[unitId];
            if (player != Actor) return "Non è il tuo momento di agire.";
            if (u.Owner != player || !u.Placed) return "Puoi muovere solo le tue unità.";
            if (Phase != Phase.Main && Phase != Phase.Response) return "Ora non si può muovere.";
            bool byDiscard = Phase == Phase.Response && Preset.C("dodge") == "SCH-SCARTO";
            if (u.Tapped) // il movimento è sempre subordinato al tap, salvo che una carta dica altro
                return $"{Def(u).Name} è tappato: " + (Phase == Phase.Response ? "un'unità tappata non può spostarsi in risposta." : "si muove tappando la carta (F1-24).");
            if (Phase == Phase.Response && Preset.C("dodgeOnlyVsAttack") == "si" && !UnderAttack(u)) return "Ci si sposta in risposta solo a un attacco contro questa unità (N-08).";
            if (byDiscard && Players[player].Hand.Count == 0) return "Per spostarti in risposta devi scartare una carta, ma non ne hai in mano.";
            if (Rooted(u)) return $"{Def(u).Name} è bloccato da Root (CS-21).";
            if (MovesFor(unitId).Count == 0) return $"{Def(u).Name} non ha caselle raggiungibili.";
            return null;
        }

        /// <summary>H24: un'unità è «sotto attacco» se in cima alla pila c'è un movimento nemico che porterebbe al combattimento contro di lei,
        /// oppure una carta o un'abilità nemica che le infliggerebbe danni.</summary>
        public bool UnderAttack(Unit u)
        {
            if (Stack.Count == 0) return false;
            var top = Stack[Stack.Count - 1];
            if (top.Controller == u.Owner || top.Kind == "end") return false;
            if (top.Kind != "card") { var a = Units[top.UnitId]; return !a.HasAttacked && InAttackRange(a, u); }
            var c = Def(top.CardId);
            return (top.TargetId == u.Id && c.Effects.Any(e => e.Kind == "damage")) || (c.Effects.Any(e => e.Kind == "damageCross") && Cross(top.Dest).Contains(u.Pos));
        }

        /// <summary>discardIndex: carta della mano da scartare, richiesta solo per lo spostamento in risposta con SCH-SCARTO.</summary>
        public string Move(int player, int unitId, Cell dest, int discardIndex = -1)
        {
            var why = WhyNotMove(player, unitId); if (why != null) return why;
            var u = Units[unitId];
            if (!MovesFor(unitId).TryGetValue(dest, out var path))
                return $"{dest} non è raggiungibile: da 1 a {MoveMax(u)} caselle, non attraverso muri o nemici, non su caselle occupate.";
            var from = u.Pos; string route = from + " → " + string.Join(" → ", path);
            if (Phase == Phase.Response)
            {   // H8: lo spostamento in risposta non apre un secondo combattimento automatico
                if (Preset.C("dodge") == "SCH-SCARTO")
                {
                    var hand = Players[player].Hand;
                    if (discardIndex < 0 || discardIndex >= hand.Count) return "Per spostarti in risposta devi scegliere una carta da scartare.";
                    var card = hand[discardIndex]; hand.RemoveAt(discardIndex); Players[player].Discard.Add(card);
                    u.Pos = dest; u.Tapped = true; u.MovedThisTurn = true;
                    Say(player, "dodge", $"{Def(u).Name} si sposta in risposta tappandosi e scartando {Def(card).Name} (SCH-SCARTO): {route}.");
                }
                else
                {
                    u.Pos = dest; u.Tapped = true; u.MovedThisTurn = true;
                    Say(player, "dodge", $"{Def(u).Name} si sposta in risposta tappandosi (SCH-TAP): {route}.");
                }
                TowerEffect(u, "Enter");
                Priority = 1 - player; return null;
            }
            u.Pos = dest; u.Tapped = true; u.MovedThisTurn = true;
            Say(player, "move", dest.Equals(from) ? $"{Def(u).Name} si tappa e resta in {from}: vale come movimento." : $"{Def(u).Name} si tappa e muove: {route}.");
            if (!dest.Equals(from)) TowerEffect(u, "Enter");
            if (Phase == Phase.Over) return null;
            var item = new StackItem { Kind = "move", Controller = player, UnitId = unitId, From = from };
            foreach (var x in Units) item.Positions[x.Id] = x.Pos;
            Stack.Add(item); Phase = Phase.Response; Priority = 1 - player;
            return null;
        }

        // ---------- carte ----------
        /// <summary>Risorse spendibili ora: carte risorsa stappate di quel colore + eventuali risorse sciolte.</summary>
        public int Available(int player, Guild g) => Players[player].Pool[g] + Players[player].Resources.Count(r => !r.Tapped && Def(r.CardId).Guild == g);
        public int AvailableTotal(int player) => Enum.GetValues(typeof(Guild)).Cast<Guild>().Sum(g => Available(player, g));
        public bool CanPay(int player, CardDef c) => Available(player, c.Guild) >= c.CostColored && AvailableTotal(player) >= c.CostColored + c.CostGeneric;
        void Spend(int player, Guild g)
        {
            var p = Players[player];
            if (p.Pool[g] > 0) p.Pool[g]--; else p.Resources.First(r => !r.Tapped && Def(r.CardId).Guild == g).Tapped = true;
        }
        void Pay(int player, CardDef c)
        {
            for (int i = 0; i < c.CostColored; i++) Spend(player, c.Guild);
            for (int i = 0; i < c.CostGeneric; i++) // il generico consuma prima i colori diversi da quello della carta
                Spend(player, Enum.GetValues(typeof(Guild)).Cast<Guild>().Where(g => Available(player, g) > 0).OrderBy(g => g == c.Guild).First());
        }

        bool ValidTarget(int player, CardDef c, Unit t)
        {
            if (!t.Placed) return false;
            if (c.Target == "self") return t.Id == Gm(player).Id;
            if (c.Target == "own" && t.Owner != player) return false;
            if (c.Target == "enemy" && t.Owner == player) return false;
            if (!Sees(Gm(player), t.Pos)) return false; // F1-57: bersaglio in linea di vista; F1-58: ci si può bersagliare
            if (c.Effects.Any(e => e.Kind == "reposition") && DestsFor(c, t.Id, player).Count == 0) return false;
            return true;
        }
        public List<int> TargetsFor(int player, CardDef c) =>
            c.Target == "none" || c.Type == CardType.Item ? new List<int>() : Units.Where(t => ValidTarget(player, c, t)).Select(t => t.Id).ToList();
        /// <summary>Caselle sceglibili: destinazioni di uno spostamento, oppure il centro di una Croce (in linea di vista del Guildmaster).</summary>
        public List<Cell> DestsFor(CardDef c, int targetId, int player)
        {
            if (c.Effects.Any(x => x.Kind == "damageCross")) return Board.All().Where(k => !Board.Opaque(k) && Sees(Gm(player), k)).ToList();
            var e = c.Effects.FirstOrDefault(x => x.Kind == "reposition");
            return e == null || targetId < 0 || Rooted(Units[targetId]) ? new List<Cell>() : MovesFor(targetId, e.Amount).Keys.ToList();
        }
        public bool NeedsTarget(CardDef c) => c.Target != "none" && c.Type != CardType.Item;
        public bool NeedsDest(CardDef c) => c.Effects.Any(e => e.Kind == "reposition" || e.Kind == "damageCross");
        /// <summary>Croce (ipotesi H21): la casella scelta e le 4 adiacenti ortogonali.</summary>
        public static IEnumerable<Cell> Cross(Cell c) => new[] { c, new Cell(c.X + 1, c.Y), new Cell(c.X - 1, c.Y), new Cell(c.X, c.Y + 1), new Cell(c.X, c.Y - 1) };

        // ---------- abilità dei Guildmaster (N-07): una volta per turno, anche in risposta, senza tappare ----------
        public CardDef AbilityOf(int player) => Preset.C("gmAbilities") != "no" ? Def(Gm(player)).Ability : null;
        public string WhyNotAbility(int player)
        {
            if (player != Actor) return "Non è il tuo momento di agire.";
            if (Phase != Phase.Main && Phase != Phase.Response) return "Ora non si possono usare abilità.";
            var a = AbilityOf(player); var gm = Gm(player);
            if (a == null) return "Le abilità dei Guildmaster sono spente in questo preset, oppure questo Guildmaster non ne ha.";
            if (Preset.C("gmAbilities") == "turno" && (Phase != Phase.Main || player != Active)) return "In questo preset l'abilità si usa solo nel proprio turno, non in risposta.";
            if (gm.AbilityUsedTurn == Turn) return "Abilità già usata in questo turno (una volta per turno).";
            if (!CanPay(player, a)) return $"Risorse insufficienti: l'abilità costa {CardRules.Cost(a, Preset)}.";
            if (NeedsTarget(a) && TargetsFor(player, a).Count == 0) return "Nessun bersaglio valido in linea di vista (F1-57).";
            return null;
        }
        public string UseAbility(int player, int targetId = -1, Cell? dest = null)
        {
            var why = WhyNotAbility(player) ?? CheckChoice(player, AbilityOf(player), targetId, dest); if (why != null) return why;
            Gm(player).AbilityUsedTurn = Turn; Players[player].Played.Add(AbilityOf(player).Id);
            Declare(player, AbilityOf(player), targetId, dest, $"{Def(Gm(player)).Name} usa la sua abilità");
            return null;
        }

        string CheckChoice(int player, CardDef c, int targetId, Cell? dest)
        {
            if (NeedsTarget(c) && !TargetsFor(player, c).Contains(targetId)) return "Bersaglio non valido.";
            if (NeedsDest(c) && (dest == null || !DestsFor(c, targetId, player).Contains(dest.Value))) return "Casella non valida.";
            return null;
        }
        void Declare(int player, CardDef c, int targetId, Cell? dest, string what)
        {
            Pay(player, c);
            var item = new StackItem { Kind = "card", Controller = player, CardId = c.Id, TargetId = NeedsTarget(c) ? targetId : -1 };
            if (item.TargetId >= 0) item.TargetPos = Units[targetId].Pos;
            if (NeedsDest(c)) { item.Dest = dest.Value; item.HasDest = true; }
            Stack.Add(item);
            Say(player, "play", what + (item.TargetId >= 0 ? $" su {Def(Units[targetId]).Name}" : "") + (item.HasDest ? $" verso {item.Dest}" : "") + $" (costo {CardRules.Cost(c, Preset)}).");
            Phase = Phase.Response; Priority = 1 - player;
        }

        public string WhyNotPlay(int player, int handIndex)
        {
            if (player != Actor) return "Non è il tuo momento di agire.";
            var c = Def(Players[player].Hand[handIndex]);
            if (Phase == Phase.Response && c.Type != CardType.Instant) return "In risposta si possono giocare solo le azioni istantanee (F1-12).";
            if (Phase != Phase.Main && Phase != Phase.Response) return "Ora non si possono giocare carte.";
            if (c.Type == CardType.Resource)
                return Players[player].ResourcesPlayedThisTurn >= Preset.N("resourcesPerTurn") ? "Hai già giocato le carte risorsa consentite in questo turno (H1)." : null;
            if (!CanPay(player, c)) return $"Risorse insufficienti: costa {CardRules.Cost(c, Preset)}.";
            if (NeedsTarget(c) && TargetsFor(player, c).Count == 0) return "Nessun bersaglio valido in linea di vista (F1-57).";
            return null;
        }

        public string Play(int player, int handIndex, int targetId = -1, Cell? dest = null)
        {
            var why = WhyNotPlay(player, handIndex); if (why != null) return why;
            var p = Players[player]; var c = Def(p.Hand[handIndex]);
            if (c.Type != CardType.Resource) { why = CheckChoice(player, c, targetId, dest); if (why != null) return why; }
            p.Hand.RemoveAt(handIndex); p.Played.Add(c.Id);
            if (c.Type == CardType.Resource)
            {   // H1/H2: la risorsa entra in gioco senza passare dalla pila e produce dal prossimo inizio turno
                bool tapped = Preset.C("resourceEnters") == "tappata";
                p.Resources.Add(new Permanent { CardId = c.Id, Tapped = tapped }); p.ResourcesPlayedThisTurn++;
                Say(player, "resource", $"G{player + 1} gioca {c.Name}" + (tapped ? " (entra tappata: utilizzabile dal suo prossimo turno)." : " (entra pronta).")); return null;
            }
            Declare(player, c, targetId, dest, $"G{player + 1} gioca {c.Name}");
            return null;
        }

        // ---------- risposte e pila ----------
        /// <summary>H8: chi ha la priorità risponde o passa; quando passa si risolve la cima della pila.</summary>
        public string Pass(int player)
        {
            if (Phase != Phase.Response || player != Priority) return "Non hai la priorità.";
            Say(player, "pass", $"G{player + 1} passa.");
            var item = Stack[Stack.Count - 1]; Stack.RemoveAt(Stack.Count - 1);
            if (item.Kind == "end") return FinishTurn(item.Controller);
            if (item.Kind != "card") ResolveMove(item); else ResolveCard(item);
            CheckDeaths(); if (Phase == Phase.Over) return null;
            if (Stack.Count > 0) Priority = 1 - Stack[Stack.Count - 1].Controller; else Phase = Phase.Main;
            return null;
        }

        public bool HasAnyResponse(int player) =>
            Phase == Phase.Response && player == Priority &&
            (Enumerable.Range(0, Players[player].Hand.Count).Any(i => WhyNotPlay(player, i) == null) || WhyNotAbility(player) == null || Units.Any(u => u.Owner == player && WhyNotMove(player, u.Id) == null));

        void ResolveMove(StackItem it)
        {   // F2-03: combattimento automatico dopo il movimento. F1-27: un attacco per unità.
            var a = Units[it.UnitId];
            if (a.HasAttacked) { Say(a.Owner, "combat", $"{Def(a).Name} ha già attaccato in questo turno (F1-27): nessun combattimento.", true); return; }
            bool annul = Preset.C("recheck") == "RIC-ANNULLA";
            if (annul && !a.Pos.Equals(it.Positions[a.Id]))
            {
                Say(a.Owner, "combat", $"{Def(a).Name} è stato spostato dopo il suo movimento: colpisce il vuoto, nessun combattimento (RIC-ANNULLA).", true);
                return;
            }
            var targets = new List<Unit>();
            foreach (var d in Units.Where(d => d.Owner != a.Owner && d.Placed && InAttackRange(a, d)))
            {
                if (annul && !d.Pos.Equals(it.Positions[d.Id]))
                    Say(d.Owner, "combat", $"{Def(d).Name} ha cambiato casella in risposta: evita il combattimento (RIC-ANNULLA).", true);
                else targets.Add(d);
            }
            if (targets.Count == 0 && CanHitTower(a))
            {
                int e = 1 - a.Owner, hit = Attack(a); a.HasAttacked = true; TowerDamage[e] += hit;
                Say(a.Owner, "combat", $"Combattimento automatico: {Def(a).Name} infligge {hit} alla Torre di G{e + 1} (punti vita {Math.Max(0, TowerHp(e))}/{Preset.N("towerHpMax")}).", true);
                if (TowerHp(e) <= 0) Say(a.Owner, "tower", $"La Torre di G{e + 1} è a 0: non è più attiva, le sue caselle non hanno più effetto.", true);
                return;
            }
            if (targets.Count == 0)
            {
                Say(a.Owner, "combat", $"Nessun combattimento: nessun nemico {(Def(a).Range == "ranged" ? "in linea di vista" : "a contatto")} di {Def(a).Name}.", true);
                return;
            }
            // shortcut: con un solo eroe per parte il bersaglio è unico; con gli alleati (P02) servirà la scelta del bersaglio.
            var t = targets[0]; a.HasAttacked = true;
            int dmg = Attack(a); t.Damage += dmg; Envenom(a, t, dmg);
            Say(a.Owner, "combat", $"Combattimento automatico: {Def(a).Name} ({Def(a).Range}) infligge {dmg} a {Def(t).Name} (Salute {Health(t)}/{MaxHealth(t)}).", true);
            if (Def(t).Range == "melee" && Board.Adjacent(a.Pos, t.Pos, Adjacency))
            {   // F1-33 + H6
                int back = Attack(t); a.Damage += back; Envenom(t, a, back);
                Say(t.Owner, "combat", $"{Def(t).Name} restituisce {back} a {Def(a).Name} (Salute {Health(a)}/{MaxHealth(a)}).", true);
            }
            else Say(t.Owner, "combat", $"{Def(t).Name} non restituisce danni ({(Def(t).Range == "ranged" ? "difensore a distanza, F1-33" : "non adiacente, H6")}).", true);
        }

        void ResolveCard(StackItem it)
        {
            var c = Def(it.CardId); var me = Gm(it.Controller); var p = Players[it.Controller];
            if (c.Type == CardType.Item)
            {
                me.Items.Add(c.Id);
                foreach (var e in c.Effects) me.Mods.Add(new Modifier { Stat = e.Stat, Amount = e.Amount, ExpiresTurn = -1, Source = c.Id });
                Say(it.Controller, "resolve", $"{c.Name} è assegnato a {Def(me).Name}."); return;
            }
            if (!c.IsAbility) p.Discard.Add(c.Id);
            var t = it.TargetId >= 0 ? Units[it.TargetId] : null;
            if (t != null)
            {   // ricontrollo del bersaglio alla risoluzione
                string fail = !t.Pos.Equals(it.TargetPos) ? "il bersaglio ha cambiato casella ed evita l'effetto (F3-11)"
                    : c.Target != "self" && !Sees(me, t.Pos) ? "il bersaglio non è più in linea di vista (F1-57)" : null;
                if (fail != null) { Say(it.Controller, "resolve", $"{c.Name} non ha effetto: {fail}.", true); return; }
            }
            foreach (var e in c.Effects)
                switch (e.Kind)
                {
                    case "damage": t.Damage += e.Amount; Say(it.Controller, "resolve", $"{c.Name}: {e.Amount} danni a {Def(t).Name} (Salute {Health(t)}/{MaxHealth(t)})."); if (c.IsAbility) Envenom(me, t, e.Amount); break;
                    case "healOwn": me.Damage = Math.Max(0, me.Damage - e.Amount); Say(it.Controller, "resolve", $"{c.Name}: cura {e.Amount} a {Def(me).Name} (Salute {Health(me)}/{MaxHealth(me)})."); break;
                    case "damageCross":
                        if (!Sees(me, it.Dest)) { Say(it.Controller, "resolve", $"{c.Name} non ha effetto: {it.Dest} non è più in linea di vista.", true); break; }
                        var hit = Cross(it.Dest).Select(UnitAt).Where(u => u != null).ToList();
                        foreach (var u in hit) { u.Damage += e.Amount; Envenom(me, u, e.Amount); }
                        Say(it.Controller, "resolve", $"{c.Name}: Croce su {it.Dest}, {e.Amount} danni a " + (hit.Count == 0 ? "nessuna unità" : string.Join(", ", hit.Select(u => $"{Def(u).Name} (Salute {Health(u)}/{MaxHealth(u)})"))) + "."); break;
                    case "heal": t.Damage = Math.Max(0, t.Damage - e.Amount); Say(it.Controller, "resolve", $"{c.Name}: cura {e.Amount} a {Def(t).Name} (Salute {Health(t)}/{MaxHealth(t)})."); break;
                    case "root":
                    case "mod":
                        int exp = e.Kind == "mod" && e.Duration == "turn" ? Turn : (Active == t.Owner ? Turn : Turn + 1);
                        t.Mods.Add(new Modifier { Stat = e.Kind == "root" ? "root" : e.Stat, Amount = e.Amount, ExpiresTurn = exp, Source = c.Id });
                        Say(it.Controller, "resolve", e.Kind == "root" ? $"{c.Name}: {Def(t).Name} non può muoversi fino alla fine del turno {exp}."
                            : $"{c.Name}: {Def(t).Name} {e.Amount:+#;-#} {e.Stat} fino alla fine del turno {exp}."); break;
                    case "reposition":
                        if (!DestsFor(c, t.Id, it.Controller).Contains(it.Dest)) { Say(it.Controller, "resolve", $"{c.Name} non ha effetto: {it.Dest} non è più raggiungibile.", true); break; }
                        Say(it.Controller, "resolve", $"{c.Name}: {Def(t).Name} è spostato da {t.Pos} a {it.Dest}."); t.Pos = it.Dest; TowerEffect(t, "Enter"); break;
                    default: throw new InvalidOperationException("Effetto non supportato: " + e.Kind);
                }
        }

        // F1-65 Venomous: quando un'unità con Venomous infligge danni a un'unità, le mette un segnalino veleno.
        void Envenom(Unit source, Unit target, int damage)
        {
            if (damage <= 0 || source == target || !Def(source).Keywords.Contains("venomous")) return;
            target.Venom++; Say(source.Owner, "venom", $"{Def(target).Name} riceve un segnalino veleno da {Def(source).Name} (Venomous).", true);
        }

        void CheckDeaths()
        {
            if (Phase == Phase.Over) return;
            var dead = Units.Where(u => u.Placed && Health(u) <= 0).Select(u => u.Owner).ToList();
            if (dead.Count == 2) { Winner = -2; WinReason = "entrambi i Guildmaster sconfitti nello stesso momento (esito non definito: D21)"; Phase = Phase.Over; Say(-1, "end", "Fine partita: " + WinReason + ".", true); }
            else if (dead.Count == 1) Win(1 - dead[0], $"{Def(Gm(dead[0])).Name} è stato sconfitto (F1-54)");
        }
        void Win(int player, string reason) { Winner = player; WinReason = reason; Phase = Phase.Over; Say(player, "end", $"Fine partita: vince G{player + 1}. {reason}.", true); }

        public void Say(int player, string kind, string text, bool auto = false) => Log.Add(new LogEntry { Turn = Turn, Player = player, Kind = kind, Text = text, Auto = auto });

        // ---------- debug: ogni intervento è registrato e rende la partita non valida per il confronto ----------
        public void DebugEdit(string description, Action<Game> edit)
        {
            edit(this); DebugTouched = true;
            Log.Add(new LogEntry { Turn = Turn, Player = -1, Kind = "debug", Text = "DEBUG: " + description, Debug = true });
            CheckDeaths();
        }
    }

    /// <summary>Giocatore automatico minimo: simula partite complete nei test e fa da avversario AI nell'interfaccia. Gioca carte a caso tra quelle sensate e si avvicina al nemico.</summary>
    public static class Bot
    {
        public static void Step(Game g, Rng r)
        {
            int me = g.Actor; var p = g.Players[me];
            if (g.Phase == Phase.Mulligan) { g.Mulligan(me, new List<int>()); return; }
            if (g.Phase == Phase.Setup) { var o = g.StartOptions(me); g.ChooseStart(me, o[r.Next(o.Count)]); return; }
            if (g.Phase == Phase.Discard) { g.DiscardCard(me, r.Next(p.Hand.Count)); return; }
            if (g.Phase != Phase.Discard && g.WhyNotAbility(me) == null && r.Next(2) == 0)
            {
                var a = g.AbilityOf(me); var foe0 = g.Gm(1 - me); int tg = -1; Cell? dc = null; bool go = true;
                if (g.NeedsTarget(a)) { go = g.TargetsFor(me, a).Contains(foe0.Id); tg = foe0.Id; }
                if (go && g.NeedsDest(a))
                {
                    var ds = g.DestsFor(a, tg, me).Where(c => a.Target != "none" || (Game.Cross(c).Contains(foe0.Pos) && !Game.Cross(c).Contains(g.Gm(me).Pos))).ToList();
                    go = ds.Count > 0; if (go) dc = ds[r.Next(ds.Count)];
                }
                if (go && g.UseAbility(me, tg, dc) == null) return;
            }
            for (int i = 0; i < p.Hand.Count; i++)
            {
                if (g.WhyNotPlay(me, i) != null || (g.Phase == Phase.Response && r.Next(2) == 0)) continue;
                var c = g.Def(p.Hand[i]); int target = -1; Cell? dest = null;
                if (g.NeedsTarget(c))
                {
                    bool good = c.Effects.All(e => e.Kind == "heal" || (e.Kind == "mod" && e.Amount > 0) || e.Kind == "reposition");
                    var ts = g.TargetsFor(me, c).Where(t => (g.Units[t].Owner == me) == good).ToList();
                    if (ts.Count == 0) continue;
                    target = ts[r.Next(ts.Count)];
                    if (g.NeedsDest(c)) { var ds = g.DestsFor(c, target, me); dest = ds[r.Next(ds.Count)]; }
                }
                if (g.Play(me, i, target, dest) == null) return;
            }
            if (g.Phase == Phase.Response) { g.Pass(me); return; }
            var gm = g.Gm(me); var foe = g.Gm(1 - me);
            if (g.WhyNotAttack(me, gm.Id) == null) { g.DeclareAttack(me, gm.Id); return; }
            if (!gm.MovedThisTurn && g.WhyNotMove(me, gm.Id) == null)
            {
                var best = g.MovesFor(gm.Id).Keys.OrderBy(c => Math.Max(Math.Abs(c.X - foe.Pos.X), Math.Abs(c.Y - foe.Pos.Y)) * 10 + r.Next(10)).First();
                g.Move(me, gm.Id, best); return;
            }
            var why = g.EndTurn(me);
            if (why != null) throw new InvalidOperationException("Bot bloccato: " + why);
        }

        public static Game Play(Game g, int seed, int maxSteps = 20000)
        {
            var r = new Rng(seed);
            for (int i = 0; i < maxSteps && g.Phase != Phase.Over; i++) Step(g, r);
            return g;
        }
    }
}
