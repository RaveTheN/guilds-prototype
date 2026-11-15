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
        public int Damage, Venom, AbilityUsedTurn; public bool Tapped, HasAttacked, MovedThisTurn, Dead;
        public List<Modifier> Mods = new List<Modifier>(); public List<string> Items = new List<string>();
        public string Role; public int SummonTurn; // alleati: pezzo degli scacchi scelto all'ingresso, turno d'ingresso
    }
    public class Permanent { public string CardId; public bool Tapped; public int Damage; } // Damage: solo con gli edifici
    public class PlayerState
    {
        public string DeckId;
        public List<string> Deck = new List<string>(), Hand = new List<string>(), Discard = new List<string>();
        public List<Permanent> Resources = new List<Permanent>(); // carte risorsa in gioco: si tappano per pagare
        public Dictionary<Guild, int> Pool = new Dictionary<Guild, int>(); // risorse sciolte (solo effetti e debug): perse a fine turno
        public List<string> SkipMonsters = new List<string>(); // caselle dei mostri che questo giocatore ha scelto di non attaccare
        public int ResourcesPlayedThisTurn, MonsterKills, Granted; // Granted: risorse fuori dal mazzo già ricevute (F2-10)
        public List<string> Drawn = new List<string>(), Played = new List<string>(); // per il resoconto
    }
    public class StackItem
    {
        public string Kind; // move | card
        public int Controller, UnitId = -1, TargetId = -1; public string CardId;
        public Cell From, TargetPos, Dest; public bool HasDest;
        public bool Assault; public int Prefer = -1, BuildingOwner = -1, BuildingIndex; public string Role; public List<int> Drop; // Prefer: bersaglio scelto del combattimento (o dell'Assalto)
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
            if (preset.C("monsters") == "attivi")
            {   // mazzo dei mostri: solo coi mostri attivi, così i preset senza mostri usano gli stessi numeri casuali di prima
                foreach (var k in MonsterKinds) for (int i = 0; i < preset.N("monsterN_" + k); i++) g.MonsterDeck.Add(k);
                g.Rng.Shuffle(g.MonsterDeck);
                foreach (var c in g.Board.All().Where(c => g.Board.Code(c) == "M")) g.SpawnMonster(c);
            }
            if (preset.C("resourceModel") != "carte")
                for (int p = 0; p < 2; p++)
                {   // F2-10: le risorse stanno fuori dal mazzo, quindi le carte risorsa escono dai mazzi
                    int n = g.Players[p].Deck.RemoveAll(id => g.Cards[id].Type == CardType.Resource);
                    if (preset.C("resourceModel") == "fisse") for (int i = 0; i < preset.N("resourceX"); i++) g.Grant(p);
                    g.Say(p, "setup", $"Risorse fuori dal mazzo ({preset.C("resourceModel")}): {n} carte risorsa tolte dal mazzo di G{p + 1}, che resta di {g.Players[p].Deck.Count} carte.", true);
                }
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
        public bool Sees(Unit from, Cell target) => Preset.C("los") == "LOS-Z" ? Board.LosZone(from.Pos, target) : Board.Los(from.Pos, target, c => EnemyAt(from.Owner, c), Preset.C("losLine"));
        public bool InAttackRange(Unit a, Unit d) => InRange(a, d.Pos);
        // N-05 doppio profilo (H10): adiacente vale il profilo in mischia, altrimenti quello a distanza
        bool Dual(Unit u) => Preset.C("attackProfile") == "doppio" && Def(u).AltAttack > 0;
        bool Adj(Unit a, Cell c) => Board.Adjacent(a.Pos, c, Adjacency);
        bool InRange(Unit a, Cell c) => Def(a).Range == "ranged" ? Sees(a, c) || (Dual(a) && Adj(a, c)) : Adj(a, c) || (Dual(a) && Sees(a, c));
        public int AttackAt(Unit a, Cell c) => !Dual(a) || Adj(a, c) == (Def(a).Range == "melee") ? Attack(a) : Math.Max(0, Def(a).AltAttack + Mod(a, "attack"));
        bool StrikesBack(Unit t, Unit a) => Adj(t, a.Pos) && (Def(t).Range == "melee" || Dual(t)); // F1-33 + H6
        public string RoleOf(Unit u) => Preset.C("moveStyle") == "re" ? "king" : u.Id < 2 ? Preset.C("gmRole") : u.Role ?? Def(u).Role ?? "pawn";
        Cell Was(StackItem it, Unit u) => it.Positions.TryGetValue(u.Id, out var c) ? c : u.Pos;

        // ---------- risorse fuori dal mazzo (F2-10) ----------
        void Grant(int player)
        {
            var guild = Def(Gm(player)).Guild; var card = Cards.Values.FirstOrDefault(c => c.Type == CardType.Resource && c.Guild == guild);
            if (card == null) return; // mazzo senza carte risorsa della gilda: non c'è una carta da mettere in gioco
            Players[player].Resources.Add(new Permanent { CardId = card.Id }); Players[player].Granted++;
        }

        // ---------- alleati (F1-10, F1-29) ----------
        public List<Cell> SummonCells(int player) => Board.All().Where(c => Board.Code(c) == "." && UnitAt(c) == null &&
            (Preset.C("summonZone") == "territorio" ? Board.Zone(c) == "G" + (player + 1)
                : Preset.C("summonZone") == "torre" ? Board.Tower(player) != null && Board.Adjacent(c, Board.Tower(player).Value, 8) : Board.Adjacent(c, Gm(player).Pos, 8))).ToList();
        public List<Unit> Namesakes(int player, CardDef c) => Units.Where(u => u.Owner == player && u.Id >= 2 && u.Placed && u.CardId == c.Id).ToList();
        bool Replaces(int player, CardDef c) => Preset.C("sameName") == "sostituisce" && Namesakes(player, c).Count > 0;
        /// <summary>Dove si gioca una carta alleato: una casella di evocazione, oppure (variante «sostituisce») la pedina con lo stesso nome.</summary>
        public List<Cell> SummonDests(int player, CardDef c) => Replaces(player, c) ? Namesakes(player, c).Select(u => u.Pos).ToList() : SummonCells(player);
        static readonly string[] RolePool = { "knight", "bishop", "king", "queen", "rook", "pawn", "pawn" };
        /// <summary>Pezzi degli scacchi ancora liberi per un alleato che entra (variante «scelta»). Il pezzo di una pedina che sta per essere rimossa o sostituita è libero.</summary>
        public List<string> AvailableRoles(int player, CardDef c)
        {
            var free = RolePool.ToList(); bool keep = Preset.C("sameName") == "nuova";
            if (Preset.C("gmRole") == "king") free.Remove("king"); // il Re è del Guildmaster; se il Guildmaster muove in altro modo, il Re è disponibile
            foreach (var u in Units.Where(u => u.Owner == player && u.Id >= 2 && u.Placed && (keep || u.CardId != c.Id))) free.Remove(RoleOf(u));
            return free.Distinct().ToList();
        }
        bool Weak(Unit u) => Preset.C("summonSick") == "sni" && u.SummonTurn == Turn && !Def(u).Keywords.Contains("swift");
        string SummonLimit(int player, CardDef c)
        {
            if (Replaces(player, c)) return null; // nessuna pedina nuova
            var mine = Units.Where(u => u.Owner == player && u.Placed && (Preset.C("sameName") != "rimpiazza" || u.Id < 2 || u.CardId != c.Id)).ToList();
            if (mine.Count >= Preset.N("maxUnits")) return $"hai già {mine.Count} miniature sulla board (massimo {Preset.N("maxUnits")}, F2-01)";
            if (Preset.C("moveStyle") == "ruoli" && mine.Any(u => u.Id >= 2 && RoleOf(u) == (c.Role ?? "pawn"))) return $"hai già una pedina col ruolo {Names.Role(c.Role)} (una per tipo, F4C-02)";
            if (Preset.C("moveStyle") == "scelta" && AvailableRoles(player, c).Count == 0) return "non hai più pezzi degli scacchi disponibili";
            return null;
        }
        void Summon(StackItem it, CardDef c)
        {
            var p = Players[it.Controller]; bool pick = Preset.C("moveStyle") == "scelta";
            if (Replaces(it.Controller, c))
            {   // la pedina con lo stesso nome resta dov'è e viene rinnovata; la carta giocata va negli scarti
                var old = Namesakes(it.Controller, c).FirstOrDefault(x => x.Pos.Equals(it.Dest));
                p.Discard.Add(c.Id);
                if (old == null) { Say(it.Controller, "resolve", $"{c.Name} non ha effetto: la pedina da sostituire non è più in {it.Dest}. La carta va negli scarti.", true); return; }
                old.Damage = 0; old.Venom = 0; int removed = 0;
                foreach (var i in (it.Drop ?? new List<int>()).Distinct().OrderByDescending(k => k)) if (i >= 0 && i < old.Mods.Count) { old.Mods.RemoveAt(i); removed++; }
                if (pick && AvailableRoles(it.Controller, c).Contains(it.Role)) old.Role = it.Role;
                Say(it.Controller, "resolve", $"{c.Name} sostituisce la pedina in {it.Dest}: vita piena, veleno rimosso, {removed} effetti tolti" + (pick ? $", pezzo {Names.Role(old.Role)}." : "."));
                return;
            }
            string fail = !SummonCells(it.Controller).Contains(it.Dest) ? $"{it.Dest} non è più disponibile" : SummonLimit(it.Controller, c)
                ?? (pick && !AvailableRoles(it.Controller, c).Contains(it.Role) ? $"il pezzo {Names.Role(it.Role)} non è più disponibile" : null);
            if (fail != null) { p.Discard.Add(c.Id); Say(it.Controller, "resolve", $"{c.Name} non entra in gioco: {fail}. La carta va negli scarti.", true); return; }
            if (Preset.C("sameName") == "rimpiazza")
                foreach (var old in Namesakes(it.Controller, c))
                {
                    old.Placed = false; old.Dead = true; p.Discard.Add(old.CardId);
                    Say(it.Controller, "resolve", $"La vecchia pedina di {c.Name} in {old.Pos} viene rimossa: entra quella nuova.", true);
                }
            string sick = c.Keywords.Contains("swift") ? "no" : Preset.C("summonSick");
            var u = new Unit { Id = Units.Count, Owner = it.Controller, CardId = c.Id, Pos = it.Dest, Placed = true, Tapped = sick == "si", SummonTurn = Turn, Role = pick ? it.Role : null };
            Units.Add(u);
            Say(it.Controller, "resolve", $"{c.Name} entra in gioco in {it.Dest}" + (pick ? $" come {Names.Role(u.Role)}" : "")
                + (sick == "si" ? ", tappato: agirà dal prossimo turno (F1-30)." : sick == "sni" ? ", pronto ma con debolezza da evocazione: in questo turno non può essere tappato." : ", pronto."));
            TowerEffect(u, "Enter");
        }

        // ---------- mostri (F3-06) ----------
        public Dictionary<string, int> MonsterDamage = new Dictionary<string, int>();
        public static readonly string[] MonsterKinds = { "cura", "attacco", "pesca" };
        public Dictionary<string, string> MonsterKind = new Dictionary<string, string>(); // casella → tipo del mostro che la occupa
        public List<string> MonsterDeck = new List<string>();                             // mazzo dei mostri, fuori dal gioco
        public int MonsterMax(Cell c) => MonsterKind.TryGetValue(c.Name, out var k) ? Preset.N("monsterHp_" + k) : 0;
        public int MonsterHp(Cell c) => MonsterMax(c) - (MonsterDamage.TryGetValue(c.Name, out var d) ? d : 0);
        void SpawnMonster(Cell c)
        {
            if (MonsterDeck.Count == 0) return;
            string k = MonsterDeck[0]; MonsterDeck.RemoveAt(0); MonsterKind[c.Name] = k; MonsterDamage.Remove(c.Name);
            Say(-1, "monster", $"Dal mazzo dei mostri esce un mostro «{k}» in {c}: punti vita {MonsterMax(c)}, danno {Preset.N("monsterAtk_" + k)} (nel mazzo restano {MonsterDeck.Count} carte).", true);
        }
        IEnumerable<Cell> LiveMonsters(int player) => Preset.C("monsters") != "attivi" ? Enumerable.Empty<Cell>()
            : Board.All().Where(c => Board.Code(c) == "M" && MonsterHp(c) > 0 && !Players[player].SkipMonsters.Contains(c.Name));
        Cell? MonsterInRange(Unit a) => LiveMonsters(a.Owner).Where(c => InRange(a, c)).Select(c => (Cell?)c).FirstOrDefault();
        /// <summary>Codice di un bersaglio che non è un'unità (Torre o mostro), da passare come «prefer».</summary>
        public static int CellTarget(Cell c) => -1000 - (c.Y * 100 + c.X);
        /// <summary>Tutto ciò che il giocatore può scegliere come bersaglio del combattimento automatico (vale solo se è a portata).</summary>
        public List<(int Code, string Label)> TargetChoices(int player)
        {
            var l = Units.Where(u => u.Owner != player && u.Placed).Select(u => (u.Id, $"{Def(u).Name} in {u.Pos}")).ToList();
            int e = 1 - player;
            if (Preset.C("towerHp") == "si" && TowerActive(e)) l.Add((CellTarget(Board.Tower(e).Value), $"Torre di G{e + 1}"));
            foreach (var c in LiveMonsters(player)) l.Add((CellTarget(c), $"Mostro «{MonsterKind[c.Name]}» in {c}"));
            return l;
        }
        void HitMonster(Unit a, Cell mc)
        {
            int hit = AttackAt(a, mc); a.HasAttacked = true;
            string kind = MonsterKind[mc.Name];
            MonsterDamage[mc.Name] = MonsterMax(mc) - MonsterHp(mc) + hit;
            Say(a.Owner, "combat", $"Combattimento automatico: {Def(a).Name} infligge {hit} al mostro «{kind}» in {mc} (punti vita {Math.Max(0, MonsterHp(mc))}/{MonsterMax(mc)}).", true);
            if (MonsterHp(mc) > 0)
            {
                int back = Preset.N("monsterAtk_" + kind);
                if (back > 0 && Adj(a, mc)) { a.Damage += back; Say(a.Owner, "combat", $"Il mostro restituisce {back} a {Def(a).Name} (Salute {Health(a)}/{MaxHealth(a)}).", true); }
                return;
            }
            var p = Players[a.Owner]; var gm = Gm(a.Owner); p.MonsterKills++;
            Say(a.Owner, "monster", $"Il mostro «{kind}» in {mc} è sconfitto da G{a.Owner + 1} (mostri sconfitti: {p.MonsterKills}).", true);
            int gain = Preset.N("monsterGain_" + kind);
            switch (kind)
            {
                case "cura": gm.Damage = Math.Max(0, gm.Damage - gain); Say(a.Owner, "monster", $"Beneficio: {Def(gm).Name} cura {gain} (Salute {Health(gm)}/{MaxHealth(gm)}).", true); break;
                case "pesca": Say(a.Owner, "monster", $"Beneficio: pesca {gain} {(gain == 1 ? "carta" : "carte")}.", true); for (int i = 0; i < gain; i++) Draw(a.Owner, false); break;
                case "attacco": gm.Mods.Add(new Modifier { Stat = "attack", Amount = gain, ExpiresTurn = -1, Source = "mostro" }); Say(a.Owner, "monster", $"Beneficio: {Def(gm).Name} ottiene +{gain} Attacco permanente.", true); break;
            }
            if (Preset.C("monsterNext") == "nuovo") SpawnMonster(mc);
            int need = Preset.N("monsterWin");
            if (need > 0 && p.MonsterKills >= need && Phase != Phase.Over) Win(a.Owner, $"G{a.Owner + 1} ha sconfitto {p.MonsterKills} mostri (F3-08)");
        }

        // ---------- edifici (F3-12/13, H30): le carte risorsa in gioco, bersagliabili dai danni ----------
        public static int BuildingTarget(int owner, int index) => -2 - (owner * 100 + index);
        bool BuildingOk(int player, CardDef c, int targetId)
        {
            int k = -2 - targetId, owner = k / 100, idx = k % 100;
            return Preset.C("buildings") == "si" && owner < 2 && idx < Players[owner].Resources.Count && (c.Target == "any" || (c.Target == "enemy" && owner != player))
                && c.Effects.Any(e => e.Kind == "damage") && c.Effects.All(e => e.Kind == "damage" || e.Kind == "healOwn");
        }
        public bool AnyBuildingTarget(int player, CardDef c) => Enumerable.Range(0, 2).Any(o => BuildingOk(player, c, BuildingTarget(o, 0)));
        void HitBuilding(StackItem it, CardDef c, Unit me)
        {
            var res = Players[it.BuildingOwner].Resources;
            // shortcut: l'edificio è ricordato per posizione; se nel frattempo ne è stato distrutto un altro prima di lui, l'effetto cade su quello che ne ha preso il posto
            if (it.BuildingIndex >= res.Count) { Say(it.Controller, "resolve", $"{c.Name} non ha effetto: l'edificio non è più in gioco.", true); return; }
            var b = res[it.BuildingIndex]; int dmg = c.Effects.Where(e => e.Kind == "damage").Sum(e => e.Amount);
            b.Damage += dmg;
            Say(it.Controller, "resolve", $"{c.Name}: {dmg} danni all'edificio {Def(b.CardId).Name} di G{it.BuildingOwner + 1} (Struttura {Math.Max(0, Preset.N("buildingHp") - b.Damage)}/{Preset.N("buildingHp")}).");
            if (b.Damage >= Preset.N("buildingHp"))
            {
                res.RemoveAt(it.BuildingIndex); Players[it.BuildingOwner].Discard.Add(b.CardId);
                Say(it.Controller, "resolve", $"L'edificio {Def(b.CardId).Name} di G{it.BuildingOwner + 1} è distrutto e va negli scarti (F3-12).", true);
            }
            foreach (var e in c.Effects.Where(e => e.Kind == "healOwn")) { me.Damage = Math.Max(0, me.Damage - e.Amount); Say(it.Controller, "resolve", $"{c.Name}: cura {e.Amount} a {Def(me).Name} (Salute {Health(me)}/{MaxHealth(me)})."); }
        }

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
            return InRange(a, t);
        }

        // ---------- turno ----------
        void BeginTurn()
        {
            Turn++; Phase = Phase.Main;
            var p = Players[Active]; p.ResourcesPlayedThisTurn = 0;
            foreach (var u in Units.Where(u => u.Owner == Active)) { u.Tapped = false; u.HasAttacked = false; u.MovedThisTurn = false; }
            foreach (var r in p.Resources) r.Tapped = false;
            if (Preset.C("resourceModel") == "crescente" && p.Granted < Preset.N("resourceX")) Grant(Active);
            if (Preset.C("gmResource") == "si") p.Pool[Def(Gm(Active)).Guild]++;
            Say(Active, "turn", $"— Turno {Turn}: G{Active + 1}. Le sue unità e le sue {p.Resources.Count} carte risorsa si stappano.", true);
            if (Preset.C("monsterLife") == "ripristinata")
                foreach (var c in Board.All().Where(c => MonsterHp(c) > 0 && MonsterDamage.ContainsKey(c.Name))) { MonsterDamage.Remove(c.Name); Say(-1, "rule", $"Il mostro in {c} torna a vita piena.", true); }
            if (Preset.C("allyLife") == "ripristinata")
                foreach (var u in Units.Where(u => u.Id >= 2 && u.Placed && u.Damage > 0)) { u.Damage = 0; Say(u.Owner, "rule", $"{Def(u).Name} torna a vita piena (N-01).", true); }
            foreach (var u in Units.Where(u => u.Owner == Active && u.Placed && u.Venom > 0))
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
            if (Preset.C("mandatoryMove") != "si") return null;
            var mine = Units.Where(u => u.Owner == player && u.Placed).ToList();
            var can = mine.Where(u => !u.MovedThisTurn && WhyNotMove(player, u.Id) == null).ToList();
            if (can.Count == 0) return null;
            if (Preset.C("obligation") == "OBB-B" && mine.Count > 1) return "Movimento obbligatorio (F3-03, OBB-B): deve ancora muovere " + string.Join(", ", can.Select(u => Def(u).Name)) + ".";
            if (mine.Any(u => u.MovedThisTurn)) return null;
            return mine.Count == 1 ? "Movimento obbligatorio (F3-03): devi muovere il tuo Guildmaster prima di finire il turno."
                : "Movimento obbligatorio (F3-03, OBB-A): devi muovere almeno una tua unità prima di finire il turno.";
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
            if (Preset.C("mandatoryMove") == "si" && !Units.Any(u => u.Owner == player && u.Placed && u.MovedThisTurn)) Say(player, "rule", $"{Def(gm).Name} non poteva muoversi: l'obbligo di movimento decade (decisione del 14 nov).", true);
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
            bool steps = maxSteps != null || Preset.C("moveStyle") == "passi" || RoleOf(u) == "steps"; // le carte che spostano di N caselle restano a passi
            var m = steps ? Board.Reach(u.Pos, maxSteps ?? MoveMax(u), c => !EnemyAt(u.Owner, c), c => UnitAt(c) == null)
                : Board.ChessReach(u.Pos, RoleOf(u), c => UnitAt(c) == null);
            if (maxSteps == null && Phase == Phase.Main && Preset.C("assault") != "no" && !u.HasAttacked)
                foreach (var e in Units.Where(x => x.Owner != u.Owner && x.Placed && (x.Id >= 2 || Preset.C("assault") == "tutti" || (Preset.C("assault") == "nogm" && u.Id >= 2))))
                {   // N-03: la casella di un nemico è una destinazione se ci si arriva accanto con un passo ancora da fare (o si è già accanto)
                    var near = m.Where(k => Board.Adjacent(k.Key, e.Pos, 8) && (!steps || k.Value.Count < MoveMax(u))).OrderBy(k => k.Value.Count).Select(k => k.Value).FirstOrDefault();
                    if (Board.Adjacent(u.Pos, e.Pos, 8)) near = new List<Cell>();
                    if (near != null) m[e.Pos] = new List<Cell>(near) { e.Pos };
                }
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
            if (Weak(u)) return $"{Def(u).Name} è appena entrato: ha debolezza da evocazione e in questo turno non può essere tappato.";
            if (u.HasAttacked) return $"{Def(u).Name} ha già attaccato in questo turno (F1-27).";
            if (!Units.Any(d => d.Owner != player && d.Placed && InAttackRange(u, d)) && !CanHitTower(u) && MonsterInRange(u) == null) return $"Nessun nemico {(Def(u).Range == "ranged" ? "in linea di tiro" : "a contatto")}.";
            return null;
        }
        public string DeclareAttack(int player, int unitId, int prefer = -1)
        {
            var why = WhyNotAttack(player, unitId); if (why != null) return why;
            var u = Units[unitId];
            // decisione del 15 nov: il tap È il movimento, anche se la pedina non cambia casella. Ordine: tap > movimento > attacco.
            u.Tapped = true; u.MovedThisTurn = true;
            Say(player, "attack", $"{Def(u).Name} si tappa e attacca restando in {u.Pos}.");
            var item = new StackItem { Kind = "attack", Controller = player, UnitId = unitId, From = u.Pos, Prefer = prefer };
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
            if (Weak(u)) return $"{Def(u).Name} è appena entrato: ha debolezza da evocazione e in questo turno non può essere tappato.";
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
        public string Move(int player, int unitId, Cell dest, int discardIndex = -1, int prefer = -1)
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
            var foe = UnitAt(dest); bool assault = foe != null && foe.Owner != player; // N-03: la casella di un nemico come destinazione
            if (assault)
            {   // l'unità si ferma nell'ultima casella libera del percorso: entra in quella nemica solo se vince
                prefer = foe.Id; route = from + string.Concat(path.Take(path.Count - 1).Select(c => " → " + c));
                dest = path.Count >= 2 ? path[path.Count - 2] : from;
            }
            u.Pos = dest; u.Tapped = true; u.MovedThisTurn = true;
            Say(player, "move", assault ? $"{Def(u).Name} si tappa e dichiara un Assalto contro {Def(foe).Name} in {foe.Pos} (N-03): {route}."
                : dest.Equals(from) ? $"{Def(u).Name} si tappa e resta in {from}: vale come movimento." : $"{Def(u).Name} si tappa e muove: {route}.");
            if (!dest.Equals(from)) TowerEffect(u, "Enter");
            if (Phase == Phase.Over || !u.Placed) return null;
            var item = new StackItem { Kind = "move", Controller = player, UnitId = unitId, From = from, Prefer = prefer, Assault = assault };
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
            if (c.Type == CardType.Ally) return SummonDests(player, c);
            return e == null || targetId < 0 || Rooted(Units[targetId]) ? new List<Cell>() : MovesFor(targetId, e.Amount).Keys.ToList();
        }
        public bool NeedsTarget(CardDef c) => c.Target != "none" && c.Type != CardType.Item;
        public bool NeedsDest(CardDef c) => c.Type == CardType.Ally || c.Effects.Any(e => e.Kind == "reposition" || e.Kind == "damageCross");
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
            if (NeedsTarget(a) && TargetsFor(player, a).Count == 0 && !AnyBuildingTarget(player, a)) return "Nessun bersaglio valido in linea di vista (F1-57).";
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
            if (NeedsTarget(c) && targetId <= -2) return BuildingOk(player, c, targetId) ? null : "Edificio non valido: servono la variante «Edifici», una carta che infligge solo danni e, se dice «nemica», un edificio avversario.";
            if (NeedsTarget(c) && !TargetsFor(player, c).Contains(targetId)) return "Bersaglio non valido.";
            if (NeedsDest(c) && (dest == null || !DestsFor(c, targetId, player).Contains(dest.Value))) return "Casella non valida.";
            return null;
        }
        void Declare(int player, CardDef c, int targetId, Cell? dest, string what)
        {
            Pay(player, c);
            var item = new StackItem { Kind = "card", Controller = player, CardId = c.Id, TargetId = NeedsTarget(c) && targetId >= 0 ? targetId : -1 };
            if (NeedsTarget(c) && targetId <= -2) { item.BuildingOwner = (-2 - targetId) / 100; item.BuildingIndex = (-2 - targetId) % 100; what += $" sull'edificio {Def(Players[item.BuildingOwner].Resources[item.BuildingIndex].CardId).Name} di G{item.BuildingOwner + 1}"; }
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
            if (c.Type == CardType.Ally)
            {
                var lim = SummonLimit(player, c); if (lim != null) return "Non puoi evocare: " + lim + ".";
                if (SummonDests(player, c).Count == 0) return "Nessuna casella libera in cui evocare (F1-29).";
            }
            if (NeedsTarget(c) && TargetsFor(player, c).Count == 0 && !AnyBuildingTarget(player, c)) return "Nessun bersaglio valido in linea di vista (F1-57).";
            return null;
        }

        /// <summary>role: pezzo degli scacchi scelto per l'alleato (variante «scelta»). drop: indici degli effetti da togliere dalla pedina sostituita (variante «sostituisce»).</summary>
        public string Play(int player, int handIndex, int targetId = -1, Cell? dest = null, string role = null, List<int> drop = null)
        {
            var why = WhyNotPlay(player, handIndex); if (why != null) return why;
            var p = Players[player]; var c = Def(p.Hand[handIndex]);
            if (c.Type != CardType.Resource) { why = CheckChoice(player, c, targetId, dest); if (why != null) return why; }
            if (c.Type == CardType.Ally && Preset.C("moveStyle") == "scelta" && !AvailableRoles(player, c).Contains(role))
                return "Scegli il pezzo degli scacchi con cui entra, fra quelli disponibili: " + string.Join(", ", AvailableRoles(player, c).Select(Names.Role)) + ".";
            p.Hand.RemoveAt(handIndex); p.Played.Add(c.Id);
            if (c.Type == CardType.Resource)
            {   // H1/H2: la risorsa entra in gioco senza passare dalla pila e produce dal prossimo inizio turno
                bool tapped = Preset.C("resourceEnters") == "tappata";
                p.Resources.Add(new Permanent { CardId = c.Id, Tapped = tapped }); p.ResourcesPlayedThisTurn++;
                Say(player, "resource", $"G{player + 1} gioca {c.Name}" + (tapped ? " (entra tappata: utilizzabile dal suo prossimo turno)." : " (entra pronta).")); return null;
            }
            Declare(player, c, targetId, dest, $"G{player + 1} gioca {c.Name}");
            Stack[Stack.Count - 1].Role = role; Stack[Stack.Count - 1].Drop = drop;
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
            if (!a.Placed) { Say(a.Owner, "combat", $"{Def(a).Name} non è più in gioco: nessun combattimento.", true); return; }
            if (a.HasAttacked) { Say(a.Owner, "combat", $"{Def(a).Name} ha già attaccato in questo turno (F1-27): nessun combattimento.", true); return; }
            bool annul = Preset.C("recheck") == "RIC-ANNULLA";
            if (annul && !a.Pos.Equals(Was(it, a)))
            {
                Say(a.Owner, "combat", $"{Def(a).Name} è stato spostato dopo il suo movimento: colpisce il vuoto, nessun combattimento (RIC-ANNULLA).", true);
                return;
            }
            if (it.Assault) { Assault(it, a); return; }
            var targets = new List<Unit>();
            foreach (var d in Units.Where(d => d.Owner != a.Owner && d.Placed && InAttackRange(a, d)))
            {
                if (annul && !d.Pos.Equals(Was(it, d)))
                    Say(d.Owner, "combat", $"{Def(d).Name} ha cambiato casella in risposta: evita il combattimento (RIC-ANNULLA).", true);
                else targets.Add(d);
            }
            // bersagli validi: unità nemiche, Torre, mostri. Chi attacca può sceglierne uno; senza scelta: unità, poi Torre, poi mostro.
            Cell? tw = CanHitTower(a) ? Board.Tower(1 - a.Owner) : null;
            var mons = LiveMonsters(a.Owner).Where(c => InRange(a, c)).ToList();
            bool pickTower = tw != null && it.Prefer == CellTarget(tw.Value);
            Cell? pickMon = mons.Where(c => CellTarget(c) == it.Prefer).Select(c => (Cell?)c).FirstOrDefault();
            if ((pickTower || pickMon != null) && targets.Count + (tw != null ? 1 : 0) + mons.Count > 1)
                Say(a.Owner, "combat", $"Più bersagli validi: {Def(a).Name} attacca {(pickTower ? "la Torre" : "il mostro in " + pickMon)} (bersaglio scelto).", true);
            if (pickTower || (targets.Count == 0 && tw != null && pickMon == null))
            {
                int e = 1 - a.Owner, hit = AttackAt(a, Board.Tower(e).Value); a.HasAttacked = true; TowerDamage[e] += hit;
                Say(a.Owner, "combat", $"Combattimento automatico: {Def(a).Name} infligge {hit} alla Torre di G{e + 1} (punti vita {Math.Max(0, TowerHp(e))}/{Preset.N("towerHpMax")}).", true);
                if (TowerHp(e) <= 0) Say(a.Owner, "tower", $"La Torre di G{e + 1} è a 0: non è più attiva, le sue caselle non hanno più effetto.", true);
                if (TowerHp(e) <= 0 && Preset.C("towerWin") == "si") Win(a.Owner, $"la Torre di G{e + 1} è stata distrutta (F4b-15)");
                return;
            }
            if (pickMon != null || (targets.Count == 0 && mons.Count > 0)) { HitMonster(a, pickMon ?? mons[0]); return; }
            if (targets.Count == 0)
            {
                Say(a.Owner, "combat", $"Nessun combattimento: nessun nemico {(Def(a).Range == "ranged" ? "in linea di vista" : "a contatto")} di {Def(a).Name}.", true);
                return;
            }
            // con più nemici a portata: il bersaglio scelto da chi attacca; senza scelta, il primo in ordine (il Guildmaster per primo)
            var t = targets.FirstOrDefault(x => x.Id == it.Prefer) ?? targets[0]; a.HasAttacked = true;
            if (targets.Count > 1) Say(a.Owner, "combat", $"Più nemici a portata: {Def(a).Name} attacca {Def(t).Name} ({(t.Id == it.Prefer ? "bersaglio scelto" : "scelta automatica")}).", true);
            int dmg = AttackAt(a, t.Pos); t.Damage += dmg; Envenom(a, t, dmg);
            Say(a.Owner, "combat", $"Combattimento automatico: {Def(a).Name} ({Def(a).Range}) infligge {dmg} a {Def(t).Name} (Salute {Health(t)}/{MaxHealth(t)}).", true);
            if (StrikesBack(t, a))
            {
                int back = AttackAt(t, a.Pos); a.Damage += back; Envenom(t, a, back);
                Say(t.Owner, "combat", $"{Def(t).Name} restituisce {back} a {Def(a).Name} (Salute {Health(a)}/{MaxHealth(a)}).", true);
            }
            else Say(t.Owner, "combat", $"{Def(t).Name} non restituisce danni ({(Def(t).Range == "ranged" ? "difensore a distanza, F1-33" : "non adiacente, H6")}).", true);
        }

        /// <summary>N-03/N-04: gli scambi si ripetono finché una delle due unità muore; il difensore a distanza infligge 1 a scambio.</summary>
        void Assault(StackItem it, Unit a)
        {
            var t = Units[it.Prefer];
            if (!t.Placed || !t.Pos.Equals(Was(it, t)) || !Board.Adjacent(a.Pos, t.Pos, 8))
            { Say(a.Owner, "combat", $"Assalto annullato: {Def(t).Name} non è più nella casella dichiarata.", true); return; }
            a.HasAttacked = true; var cell = t.Pos;
            for (int round = 1; round <= 200 && Health(a) > 0 && Health(t) > 0; round++)
            {
                int da = AttackAt(a, t.Pos), dt = Def(t).Range == "ranged" && !Dual(t) ? 1 : AttackAt(t, a.Pos);
                if (da == 0 && dt == 0) { Say(a.Owner, "combat", "Assalto interrotto: nessuna delle due unità può fare danno (caso non definito: D19).", true); break; }
                t.Damage += da; a.Damage += dt; Envenom(a, t, da); Envenom(t, a, dt);
                Say(a.Owner, "combat", $"Assalto, scambio {round}: {Def(a).Name} infligge {da} a {Def(t).Name} (Salute {Health(t)}/{MaxHealth(t)}); {Def(t).Name} infligge {dt} a {Def(a).Name} (Salute {Health(a)}/{MaxHealth(a)}).", true);
            }
            bool won = Health(t) <= 0 && Health(a) > 0;
            CheckDeaths();
            if (Phase == Phase.Over || !won || !a.Placed) return;
            a.Pos = cell; Say(a.Owner, "combat", $"{Def(a).Name} vince l'Assalto e occupa {cell}.", true);
            TowerEffect(a, "Enter");
        }

        void ResolveCard(StackItem it)
        {
            var c = Def(it.CardId); var me = Gm(it.Controller); var p = Players[it.Controller];
            if (c.Type == CardType.Ally) { Summon(it, c); return; }
            if (c.Type == CardType.Item)
            {
                me.Items.Add(c.Id);
                foreach (var e in c.Effects) me.Mods.Add(new Modifier { Stat = e.Stat, Amount = e.Amount, ExpiresTurn = -1, Source = c.Id });
                Say(it.Controller, "resolve", $"{c.Name} è assegnato a {Def(me).Name}."); return;
            }
            if (!c.IsAbility) p.Discard.Add(c.Id);
            if (it.BuildingOwner >= 0) { HitBuilding(it, c, me); return; }
            var t = it.TargetId >= 0 ? Units[it.TargetId] : null;
            if (t != null)
            {   // ricontrollo del bersaglio alla risoluzione
                string fail = !t.Placed ? "il bersaglio non è più in gioco" : !t.Pos.Equals(it.TargetPos) ? "il bersaglio ha cambiato casella ed evita l'effetto (F3-11)"
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
            foreach (var u in Units.Where(u => u.Id >= 2 && u.Placed && Health(u) <= 0).ToList())
            {   // un alleato sconfitto lascia la board e la sua carta va negli scarti
                u.Placed = false; u.Dead = true; Players[u.Owner].Discard.Add(u.CardId);
                Say(u.Owner, "death", $"{Def(u).Name} è sconfitto e lascia la board.", true);
            }
            var dead = Units.Take(2).Where(u => u.Placed && Health(u) <= 0).Select(u => u.Owner).ToList();
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
                else if (g.NeedsDest(c)) { var ds = g.DestsFor(c, -1, me); if (ds.Count == 0) continue; dest = ds[r.Next(ds.Count)]; }
                if (g.Play(me, i, target, dest, c.Type == CardType.Ally ? g.AvailableRoles(me, c).FirstOrDefault() : null) == null) return;
            }
            if (g.Phase == Phase.Response) { g.Pass(me); return; }
            var foe = g.Gm(1 - me);
            foreach (var u in g.Units.Where(u => u.Owner == me && u.Placed).ToList())
            {
                if (g.WhyNotAttack(me, u.Id) == null) { g.DeclareAttack(me, u.Id); return; }
                if (!u.MovedThisTurn && g.WhyNotMove(me, u.Id) == null)
                {
                    var best = g.MovesFor(u.Id).Keys.OrderBy(c => Math.Max(Math.Abs(c.X - foe.Pos.X), Math.Abs(c.Y - foe.Pos.Y)) * 10 + r.Next(10)).First();
                    g.Move(me, u.Id, best); return;
                }
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
