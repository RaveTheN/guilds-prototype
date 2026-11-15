// Schermata di partita: HUD, mano, selezione, finestre di risposta, passaggio del controllo, registro, debug.
// Tutte le azioni passano dai comandi di Guilds.Core; i motivi dei divieti arrivano dal motore.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Guilds.Core;
using UnityEngine;

namespace Guilds.Unity
{
    public partial class GuildsApp
    {
        int selCard = -1, selTarget = -1, selUnit = -1, shownTo = -1;
        bool showBoth, dirty, debugTeleport, selAbility; string overlay, exported, debugCard = "";
        Vector2 logScroll, overlayScroll; int lastLog;
        readonly HashSet<int> mullSel = new HashSet<int>();
        List<int> markedTargets = new List<int>();
        CardDef peek; // carta già giocata (oggetto, risorsa, carta in pila) mostrata in grande: un clic qualsiasi la chiude
        Cell? hover; // casella sotto il puntatore: anteprima del bersaglio o dell'area d'effetto
        string selRole; readonly HashSet<int> selDrop = new HashSet<int>(); // alleato da giocare: pezzo scelto, effetti da togliere dalla pedina sostituita
        int prefer = -1; // bersaglio scelto per il combattimento automatico quando più nemici sono a portata (-1 = automatico)
        int dodgeUnit = -1; Cell dodgeDest; // spostamento in risposta in attesa della carta da scartare

        const float LeftW = 400, RightW = 380, TopH = 56, HandH = 310, TableW = 200;
        Rect BoardRect => new Rect(LeftW + TableW, TopH, W - LeftW - RightW - 2 * TableW, H - TopH - HandH);

        // Ciò che è selezionato per essere giocato: una carta della mano oppure l'abilità del proprio Guildmaster.
        CardDef SelDef(int me) => me < 0 ? null : selAbility ? game.AbilityOf(me) : selCard >= 0 && selCard < game.Players[me].Hand.Count ? game.Def(game.Players[me].Hand[selCard]) : null;
        string SelWhy(int me) => selAbility ? game.WhyNotAbility(me) : game.WhyNotPlay(me, selCard);
        string SelPlay(int me, int target, Cell? dest) => selAbility ? game.UseAbility(me, target, dest) : game.Play(me, selCard, target, dest, selRole, selDrop.ToList());
        bool HandVisible(int player) => showBoth || shownTo == player;
        static readonly string[] PhaseNames = { "Scelta della partenza", "Fase principale", "Finestra di risposta", "Scarto a fine turno", "Partita finita", "Mulligan iniziale" };

        void Update()
        {
            if (mode != Mode.Play || game == null) return;
            // automazione visibile: se chi ha la priorità non ha risposte possibili, passa da solo (registrato)
            while (autoPass && game.Phase == Phase.Response && !game.HasAnyResponse(game.Priority))
            {
                game.Say(game.Priority, "pass", $"G{game.Priority + 1} non ha risposte possibili: passa automaticamente.", true);
                game.Pass(game.Priority); dirty = true;
            }
            if (game.Phase == Phase.Over || game.Actor != Ai) aiAt = Time.time + 0.9f; // pausa prima di ogni mossa dell'AI, per poterla seguire
            else if (Time.time >= aiAt)
            {
                try { Bot.Step(game, aiRng); } catch (System.Exception ex) { vsAi = false; Say("AI fermata, G2 torna in mano tua: " + ex.Message); }
                aiAt = Time.time + 0.9f; dirty = true;
            }
            if (game.Phase == Phase.Over && exported == null) exported = ExportReport();
            if (dirty) { dirty = false; view.Sync(game); RefreshMarks(); }
        }

        void Act(string why) { if (Say(why)) { selCard = selTarget = selUnit = -1; selAbility = false; selDrop.Clear(); } dodgeUnit = -1; dirty = true; }
        void Deselect() { selCard = selTarget = selUnit = dodgeUnit = -1; selAbility = false; selDrop.Clear(); dirty = true; }

        /// <summary>Caselle accanto alle Torri attive: verde tenue = cura, rosso tenue = danno; colore intenso se l'unità che c'è sopra subisce quell'effetto.</summary>
        void TowerMarks()
        {
            var g = game; int pov = vsAi ? 0 : g.Active; // casella vuota: colore dal punto di vista di chi è di turno (del giocatore umano contro l'AI)
            bool heal = g.Preset.C("towerHealEnter") == "si" || g.Preset.C("towerHealStart") == "si";
            bool dmg = g.Preset.C("towerDmgEnter") == "si" || g.Preset.C("towerDmgStart") == "si";
            if (!heal && !dmg) return;
            for (int i = 0; i < 2; i++)
            {
                if (!g.TowerActive(i)) continue;
                foreach (var c in g.Board.All().Where(c => !g.Board.Code(c).StartsWith("T") && g.NearTower(c, i)))
                {
                    var u = g.Units.FirstOrDefault(x => x.Placed && x.Pos.Equals(c));
                    int who = u != null ? u.Owner : pov;
                    bool green = who == i ? heal : !dmg;
                    bool strong = u != null && (u.Owner == i ? heal : dmg);
                    float a = strong ? 0.75f : 0.25f;
                    view.Mark(new[] { c }, green ? new Color(0.1f, 0.9f, 0.3f, a) : new Color(1, 0.15f, 0.1f, a), false, true);
                }
            }
        }

        void RefreshMarks()
        {
            view.ClearMarks(); markedTargets = new List<int>();
            TowerMarks();
            if (game.Phase == Phase.Over || game.Phase == Phase.Mulligan) return;
            int me = game.Actor;
            if (game.Phase == Phase.Setup) { view.Mark(game.StartOptions(me), new Color(1, 0.85f, 0.1f, 0.8f)); return; }
            if (!HandVisible(me)) return;
            var sel = SelDef(me);
            if (sel != null)
            {
                var c = sel;
                if (SelWhy(me) != null) return;
                if (selTarget < 0 && game.NeedsTarget(c))
                {
                    markedTargets = game.TargetsFor(me, c);
                    view.Mark(markedTargets.Select(t => game.Units[t].Pos), new Color(1, 0.1f, 0.1f, 0.9f), true);
                    foreach (var t in markedTargets) if (t != me) { var l = game.Board.LosLine(game.Gm(me).Pos, game.Units[t].Pos, x => false, game.Preset.C("losLine")); if (l != null) view.Line(l.Value, Color.red); }
                }
                else view.Mark(game.DestsFor(c, selTarget, me), new Color(0.2f, 0.5f, 1, 0.8f));
                if (hover != null)
                {   // anteprima: la casella puntata, oppure tutta l'area d'effetto (Croce)
                    var hv = hover.Value;
                    view.Mark((c.Effects.Any(x => x.Kind == "damageCross") ? Game.Cross(hv) : new[] { hv }).Where(game.Board.InMap), new Color(1, 0.55f, 0.1f, 0.9f), true);
                }
            }
            else if (selUnit >= 0)
            {
                var u = game.Units[selUnit];
                view.Mark(new[] { u.Pos }, new Color(1, 1, 1, 0.85f), true);
                if (dodgeUnit >= 0) view.Mark(new[] { dodgeDest }, new Color(0.2f, 0.5f, 1, 0.9f));
                else if (game.WhyNotMove(me, selUnit) == null) view.Mark(game.MovesFor(selUnit).Keys, new Color(0.1f, 0.9f, 0.3f, 0.75f));
                // linea di vista dell'unità selezionata verso i nemici
                foreach (var e in game.Units.Where(e => e.Owner != u.Owner && e.Placed))
                {
                    var l = game.Board.LosLine(u.Pos, e.Pos, x => false, game.Preset.C("losLine"));
                    if (l != null && game.Sees(u, e.Pos)) view.Line(l.Value, new Color(1, 0.9f, 0.2f));
                }
            }
        }

        void BoardClick(Cell c)
        {
            int me = game.Actor;
            if (game.Phase == Phase.Over) return;
            if (me == Ai) { Say("Tocca all'AI (G2): aspetta la sua mossa."); return; }
            if (debugTeleport && selUnit >= 0)
            {
                var u = game.Units[selUnit]; debugTeleport = false;
                if (game.Board.BlocksMove(c) || game.UnitAt(c) != null) { Say("Debug: casella non valida."); return; }
                game.DebugEdit($"{game.Def(u).Name} spostato da {u.Pos} a {c}", g => u.Pos = c); dirty = true; return;
            }
            if (game.Phase == Phase.Mulligan) { Say("Prima decidete il Mulligan con i pulsanti in basso."); return; }
            if (game.Phase == Phase.Setup) { Act(game.ChooseStart(me, c)); shownTo = -1; return; }
            if (!HandVisible(me)) { Say($"Tocca a G{me + 1}: conferma il passaggio del controllo in basso."); return; }
            var at = game.UnitAt(c);
            var card = SelDef(me);
            if (card != null)
            {
                var why = SelWhy(me);
                if (why != null) { Say($"Non puoi giocare {card.Name}: {why}"); return; }
                bool pickUnit = game.NeedsTarget(card) && card.Target != "self";
                int fixedTarget = card.Target == "self" ? me : -1;
                if (pickUnit && selTarget < 0)
                {
                    if (at == null || !markedTargets.Contains(at.Id)) { Say("Bersaglio non valido: i bersagli possibili hanno il riquadro rosso e la scritta BERSAGLIO."); return; }
                    if (game.NeedsDest(card)) { selTarget = at.Id; dirty = true; } else Act(SelPlay(me, at.Id, null));
                    return;
                }
                if (game.NeedsDest(card)) { Act(SelPlay(me, pickUnit ? selTarget : fixedTarget, c)); return; }
                // nessuna scelta da fare: il clic sul proprio Guildmaster vale come «Gioca»
                if (at != null && at.Id == me) Act(SelPlay(me, fixedTarget, null)); else Say($"{card.Name} non richiede di scegliere un bersaglio: premi GIOCA in basso.");
                return;
            }
            if (at != null && (selUnit < 0 || at.Id == selUnit || at.Owner == me))
            {
                selUnit = at.Id; dirty = true;
                var why = at.Owner == me ? game.WhyNotMove(me, at.Id) : null; if (why != null) Say(why);
                return;
            }
            if (selUnit >= 0 && game.Units[selUnit].Owner == me)
            {
                if (game.Phase == Phase.Response && game.Preset.C("dodge") == "SCH-SCARTO")
                {   // spostamento in risposta: prima la casella, poi la carta da scartare
                    var w = game.WhyNotMove(me, selUnit);
                    if (w != null) { Say(w); return; }
                    if (!game.MovesFor(selUnit).ContainsKey(c)) { Say("Casella non raggiungibile: scegli una casella verde."); return; }
                    dodgeUnit = selUnit; dodgeDest = c; selCard = -1; dirty = true;
                }
                else Act(game.Move(me, selUnit, c, -1, prefer));
            }
        }

        /// <summary>Disegna ruotato di 90° ciò che è tappato, come una carta sul tavolo.</summary>
        void Turned(Rect r, bool tapped, System.Action draw)
        {
            var m = GUI.matrix; if (tapped) GUIUtility.RotateAroundPivot(90, G.R(r).center);
            draw(); GUI.matrix = m;
        }

        void DrawSmallCard(Rect r, CardDef c, bool tapped, System.Action click = null) => Turned(r, tapped, () =>
        {
            Fill(r, tapped ? new Color(0.35f, 0.35f, 0.35f) : Color.white);
            Fill(new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4), Color.Lerp(assets.GuildColor(c.Guild), Color.black, tapped ? 0.55f : 0.15f));
            var tex = assets.CardArt(c.Id) ?? assets.Symbol(Names.SymbolId(c.Guild, game.Preset));
            if (tex != null) G.DrawTexture(new Rect(r.x + 5, r.y + 16, r.width - 10, r.height - 21), tex, ScaleMode.ScaleToFit);
            G.Label(new Rect(r.x + 3, r.y, r.width - 4, 18), c.Name.Replace(" (test)", "").Split(' ')[0], tiny);
            if (G.Button(r, GUIContent.none, GUIStyle.none)) { if (click != null) click(); else peek = c; }
        });

        /// <summary>Il tavolo di un giocatore accanto alla board: Guildmaster, oggetti assegnati e carte risorsa, pronti o tappati.</summary>
        void DrawTable(Rect r, int pl)
        {
            var g = game; var p = g.Players[pl]; var u = g.Gm(pl); var d = g.Def(u);
            Fill(r, new Color(0.16f, 0.13f, 0.1f)); Fill(new Rect(r.x, r.y, r.width, 26), Color.Lerp(assets.GuildColor(d.Guild), Color.black, 0.4f));
            G.Label(new Rect(r.x + 8, r.y + 2, r.width, 24), $"Tavolo di G{pl + 1}", bold);
            var gr = new Rect(r.x + (r.width - 134) / 2, r.y + 34, 134, 188);
            Turned(gr, u.Tapped, () => DrawCard(gr, d, g.Preset, false));
            if (G.Button(gr, GUIContent.none, GUIStyle.none)) { selUnit = u.Id; selCard = selTarget = -1; selAbility = false; dirty = true; }
            float y = r.y + 228;
            G.Label(new Rect(r.x + 8, y, r.width - 12, 22), (u.Tapped ? "<color=#ffb060><b>TAPPATO</b></color>" : "<color=#90ff90><b>pronto</b></color>") + $" · Salute {g.Health(u)}/{g.MaxHealth(u)}", small); y += 22;
            string extra = (u.Venom > 0 ? $"<color=#c0ff60>Veleno ×{u.Venom}</color>  " : "") + (g.Rooted(u) ? "<color=#ff9090>Root</color>  " : "") + (g.AbilityOf(pl) != null ? (u.AbilityUsedTurn == g.Turn ? "Abilità usata" : "Abilità pronta") : "");
            G.Label(new Rect(r.x + 8, y, r.width - 12, 22), extra, tiny); y += 24;
            G.Label(new Rect(r.x + 8, y, r.width - 12, 20), $"Oggetti assegnati: {u.Items.Count}", tiny); y += 20;
            for (int i = 0; i < u.Items.Count; i++) DrawSmallCard(new Rect(r.x + 8 + i * Mathf.Min(48, (r.width - 70) / Mathf.Max(1, u.Items.Count - 1 + 0.01f)), y, 56, 76), g.Def(u.Items[i]), false);
            y += 82;
            int ready = p.Resources.Count(x => !x.Tapped);
            G.Label(new Rect(r.x + 8, y, r.width - 12, 20), $"Carte risorsa: {p.Resources.Count} (pronte {ready})", tiny); y += 22;
            bool bld = g.Preset.C("buildings") == "si";
            for (int i = 0; i < p.Resources.Count; i++)
            {
                int bi = i; var rd = g.Def(p.Resources[i].CardId); var rr = new Rect(r.x + 10 + i % 3 * 64, y + i / 3 * 64 + 2, 44, 60);
                DrawSmallCard(rr, rd, p.Resources[i].Tapped, () =>
                {   // variante Edifici: con una carta di danno selezionata, il clic sceglie l'edificio come bersaglio
                    int me = g.Actor; var sel = SelDef(me);
                    if (bld && sel != null && g.NeedsTarget(sel) && me != Ai && HandVisible(me)) Act(SelPlay(me, Game.BuildingTarget(pl, bi), null)); else peek = rd;
                });
                if (bld) { Fill(new Rect(rr.x, rr.yMax - 16, 44, 16), new Color(0, 0, 0, 0.7f)); G.Label(new Rect(rr.x + 2, rr.yMax - 17, 44, 18), $"Str {g.Preset.N("buildingHp") - p.Resources[i].Damage}", tiny); }
            }
        }

        string StackText(StackItem it) => it.Kind == "end" ? $"Fine del turno di G{it.Controller + 1}" : it.Kind == "attack" ? $"Attacco senza movimento di {game.Def(game.Units[it.UnitId]).Name}" : it.Kind == "move" ? $"Movimento di {game.Def(game.Units[it.UnitId]).Name} → poi " + (it.Assault ? $"Assalto contro {game.Def(game.Units[it.Prefer]).Name}" : "combattimento automatico")
            : $"{game.Def(it.CardId).Name} (G{it.Controller + 1})" + (it.TargetId >= 0 ? $" su {game.Def(game.Units[it.TargetId]).Name}" : it.BuildingOwner >= 0 ? $" su un edificio di G{it.BuildingOwner + 1}" : it.HasDest ? $" in {it.Dest}" : "");

        void PlayGUI()
        {
            var g = game; int me = g.Actor; var e = Event.current; var mouse = e.mousePosition / scale;
            if (peek != null && e.type == EventType.MouseDown) { peek = null; e.Use(); }
            PlaceCamera(BoardRect);
            // senza passaggio del controllo (o contro l'AI) la mano di chi deve agire è subito visibile; quella dell'AI mai
            if (me >= 0 && me != Ai && g.Phase != Phase.Over && (!handoff || vsAi) && shownTo != me) { shownTo = me; Deselect(); mullSel.Clear(); }

            // --- barra superiore ---
            Fill(new Rect(0, 0, W, TopH), new Color(0.08f, 0.08f, 0.1f));
            string head = g.Phase == Phase.Over ? (g.Winner >= 0 ? $"VINCE G{g.Winner + 1} — {g.WinReason}" : $"NESSUN VINCITORE — {g.WinReason}")
                : g.Phase == Phase.Mulligan || g.Phase == Phase.Setup ? $"Preparazione — {PhaseNames[(int)g.Phase]} — tocca a G{me + 1}"
                : $"Turno {g.Turn} di G{g.Active + 1} ({g.Def(g.Gm(g.Active)).Name}) — {PhaseNames[(int)g.Phase]}" + (g.Phase == Phase.Response ? $" — priorità a G{g.Priority + 1}" : "");
            G.Label(new Rect(12, 10, W - 900, 40), head, title);
            float bx = W - 880;
            foreach (var (name, id) in new[] { ("Regole attive", "regole"), ("Note", "note"), ("Mazzi", "carte") })
            { if (G.Button(new Rect(bx, 10, 125, 36), name)) overlay = overlay == id ? null : id; bx += 130; }
            if (G.Button(new Rect(bx, 10, 150, 36), "Salva situazione")) Say("Situazione salvata in " + SaveSituation()); bx += 155;
            if (G.Button(new Rect(bx, 10, 110, 36), "Resoconto")) Say("Resoconto esportato in " + (exported = ExportReport())); bx += 115;
            if (G.Button(new Rect(bx, 10, 100, 36), "Riavvia")) { seed++; StartGame(); return; } bx += 105;
            if (G.Button(new Rect(bx, 10, 100, 36), "Menu")) { view.Destroy(); view = null; game = null; mode = Mode.Config; return; }

            // --- striscia di fase: chi agisce, e se nel proprio turno o in risposta ---
            if (g.Phase != Phase.Over)
            {
                bool resp = g.Phase == Phase.Response, prep = g.Phase == Phase.Mulligan || g.Phase == Phase.Setup;
                string who = $"G{me + 1}" + (me == Ai ? " (AI)" : "");
                string line = prep ? $"PREPARAZIONE — {who}: {PhaseNames[(int)g.Phase]}"
                    : resp ? $"RISPOSTA — {who} risponde {(me == g.Active ? "nel PROPRIO turno" : $"nel turno di G{g.Active + 1}")} a: {StackText(g.Stack[g.Stack.Count - 1])}"
                    : g.Phase == Phase.Discard ? $"TURNO {g.Turn} DI {who} — scarto di fine turno"
                    : $"TURNO {g.Turn} DI {who} — fase principale: agisce nel proprio turno";
                string[] steps = { "Mulligan", "Partenza", "Fase principale", "Risposta", "Scarto di fine turno" };
                int cur = g.Phase == Phase.Mulligan ? 0 : g.Phase == Phase.Setup ? 1 : resp ? 3 : g.Phase == Phase.Discard ? 4 : 2;
                string track = string.Join("  ›  ", steps.Select((s, i) => i == cur ? $"<color=#ffe060><b>[ {s} ]</b></color>" : s));
                var br = new Rect(BoardRect.x, TopH, BoardRect.width, 62);
                Fill(br, resp ? new Color(0.7f, 0.33f, 0.05f, 0.95f) : prep ? new Color(0.12f, 0.25f, 0.5f, 0.95f) : new Color(0.1f, 0.4f, 0.15f, 0.95f));
                G.Label(new Rect(br.x + 10, br.y + 4, br.width - 20, 30), line, bold);
                G.Label(new Rect(br.x + 10, br.y + 34, br.width - 20, 24), track, small);
            }

            // --- sinistra: giocatori e registro ---
            Fill(new Rect(0, TopH, LeftW, H), new Color(0.1f, 0.1f, 0.13f, 0.97f));
            G.Label(new Rect(12, TopH + 2, 380, 22), $"{g.Preset.Id} — TEST PARZIALE — {g.Map.Id} — seed {g.Seed}" + (g.DebugTouched ? " — NON VALIDA" : ""), tiny);
            float y = TopH + 26, tw = LeftW - 30;
            for (int i = 0; i < 2; i++)
            {
                var p = g.Players[i]; var u = g.Gm(i); var d = g.Def(u);
                Fill(new Rect(6, y, LeftW - 12, 196), Color.Lerp(assets.GuildColor(d.Guild), Color.black, i == me ? 0.45f : 0.75f));
                if (i == me) Fill(new Rect(6, y, 5, 196), Color.white);
                G.Label(new Rect(16, y + 2, LeftW - 30, 26), $"G{i + 1}{(i == Ai ? " [AI]" : "")} — {d.Name}" + (i == me ? (g.Phase == Phase.Response ? "   (agisce IN RISPOSTA)" : i == g.Active ? "   (agisce nel suo turno)" : "   (deve agire)") : ""), bold);
                G.Label(new Rect(16, y + 28, tw, 22), u.Placed ? $"Salute {g.Health(u)}/{g.MaxHealth(u)} · Attacco {g.Attack(u)} ({(d.Range == "ranged" ? "a distanza" : "mischia")})" : "non ancora in campo", small);
                G.Label(new Rect(16, y + 50, tw, 22), u.Placed ? $"Movimento {g.MoveMax(u)} · casella {u.Pos}" : "", small);
                string flags = (u.Tapped ? "<color=#ffb060><b>TAPPATA</b> fino al suo prossimo turno</color>" : "<color=#90ff90>stappata</color>") + (g.Rooted(u) ? " · <color=#ff9090>ROOT</color>" : "") + (u.HasAttacked ? " · ha attaccato" : "");
                G.Label(new Rect(16, y + 72, tw, 22), flags, small);
                G.Label(new Rect(16, y + 94, tw, 22), u.Items.Count > 0 ? "Oggetti: " + string.Join(", ", u.Items.Select(x => g.Def(x).Name.Replace(" (test)", ""))) : "Nessun oggetto", tiny);
                G.Label(new Rect(16, y + 116, tw, 22), $"Mazzo {p.Deck.Count} · Mano {p.Hand.Count} · Scarti {p.Discard.Count}" + (g.Preset.C("towerHp") == "si" ? $" · Torre {Mathf.Max(0, g.TowerHp(i))}/{g.Preset.N("towerHpMax")}" : "") + (g.Preset.C("monsters") == "attivi" ? $" · Mostri {p.MonsterKills}" : ""), small);
                G.Label(new Rect(16, y + 140, LeftW - 30, 22), $"Carte risorsa sul tavolo: {p.Resources.Count}  (si tappano per pagare)", small);
                G.Label(new Rect(16, y + 166, 170, 22), "Da spendere ora:", small);
                int tot = g.AvailableTotal(i); float px = 150;
                foreach (Guild k in System.Enum.GetValues(typeof(Guild))) { int n = g.Available(i, k); if (n == 0) continue; DrawSymbols(new Rect(px, y + 164, 220, 24), k, n, 0, g.Preset, false); px += n * 26 + 8; }
                G.Label(new Rect(px, y + 166, 120, 22), tot == 0 ? "nessuna" : "= " + tot, small);
                y += 202;
            }
            G.Label(new Rect(12, y, 300, 24), "Registro degli eventi", bold); y += 26;
            var logView = new Rect(6, y, LeftW - 12, H - y - 6);
            string logText = string.Join("\n", g.Log.Select(l => (l.Debug ? "<color=#ff8080>" : l.Auto ? "<color=#9fb4c8>" : "<color=#ffffff>") + l.Text + "</color>"));
            float lh = G.Height(small, logText, LeftW - 44);
            if (g.Log.Count != lastLog) { lastLog = g.Log.Count; logScroll.y = 1e9f; }
            logScroll = G.BeginScrollView(logView, logScroll, new Rect(0, 0, LeftW - 34, lh));
            G.Label(new Rect(0, 0, LeftW - 44, lh), logText, small);
            G.EndScrollView();

            // --- destra: camera, pila, carta/unità selezionata, debug ---
            float rx = W - RightW; Fill(new Rect(rx, TopH, RightW, H), new Color(0.1f, 0.1f, 0.13f, 0.97f));
            y = TopH + 8;
            G.Label(new Rect(rx + 10, y, RightW - 20, 40), "Vista: trascina col tasto destro per ruotare e inclinare, rotella per lo zoom.", tiny); y += 38;
            topView = G.Toggle(new Rect(rx + 10, y, 170, 28), topView, "Dall'alto (T)");
            if (G.Button(new Rect(rx + 190, y, 170, 28), "Vista iniziale")) { yaw = 0; tilt = 62; zoom = 1; topView = false; }
            y += 36;
            if (g.Stack.Count > 0)
            {
                G.Label(new Rect(rx + 10, y, RightW - 20, 24), "Pila (si risolve dall'alto):", bold); y += 24;
                foreach (var it in Enumerable.Reverse(g.Stack))
                {
                    string s = StackText(it);
                    G.Label(new Rect(rx + 10, y, RightW - 20, 44), "• " + s, small);
                    if (it.Kind == "card" && G.Button(new Rect(rx + 10, y, RightW - 20, 40), GUIContent.none, GUIStyle.none)) peek = g.Def(it.CardId);
                    y += 40;
                }
            }
            var hand = me >= 0 && HandVisible(me) ? g.Players[me].Hand : null;
            var sc = hand != null && g.Phase != Phase.Mulligan ? SelDef(me) : null;
            bool cardSel = sc != null;
            if (cardSel && !selAbility)
            {
                DrawCard(new Rect(rx + 20, y, 340, 470), sc, g.Preset, true); y += 480;
                if (sc.Type == CardType.Ally && g.Preset.C("moveStyle") == "scelta")
                {   // pezzo degli scacchi con cui l'alleato entra
                    var roles = g.AvailableRoles(me, sc); if (!roles.Contains(selRole)) selRole = roles.FirstOrDefault();
                    G.Label(new Rect(rx + 20, y, 340, 24), "Entra come (pezzi ancora disponibili):", small); y += 24;
                    for (int i = 0; i < roles.Count; i++)
                    {
                        var oc = GUI.color; if (roles[i] == selRole) GUI.color = new Color(0.6f, 1, 0.6f);
                        if (G.Button(new Rect(rx + 20 + i % 3 * 114, y + i / 3 * 34, 110, 30), Names.Role(roles[i]))) selRole = roles[i];
                        GUI.color = oc;
                    }
                    y += (roles.Count + 2) / 3 * 34 + 4;
                }
                var same = sc.Type == CardType.Ally && g.Preset.C("sameName") == "sostituisce" ? g.Namesakes(me, sc) : new List<Unit>();
                if (same.Count == 1 && same[0].Mods.Count > 0)
                {   // sostituzione: il giocatore decide quali effetti delle carte giocate sulla pedina togliere
                    G.Label(new Rect(rx + 20, y, 340, 24), "Effetti da togliere dalla pedina sostituita:", small); y += 24;
                    for (int i = 0; i < same[0].Mods.Count; i++)
                    {
                        var m = same[0].Mods[i]; string src = g.Cards.TryGetValue(m.Source ?? "", out var sd) ? sd.Name : m.Source;
                        bool on = G.Toggle(new Rect(rx + 20, y, 340, 26), selDrop.Contains(i), $"{src}: {(m.Stat == "root" ? "Root" : m.Stat + " " + m.Amount.ToString("+#;-#"))}");
                        if (on) selDrop.Add(i); else selDrop.Remove(i);
                        y += 26;
                    }
                }
            }
            else if (selUnit >= 0)
            {
                DrawCard(new Rect(rx + 20, y, 340, 470), g.Def(g.Units[selUnit]), g.Preset, true); y += 476;
                if (hand != null && selUnit == me && g.AbilityOf(me) != null && (g.Phase == Phase.Main || g.Phase == Phase.Response))
                {   // abilità propria del Guildmaster: una volta per turno
                    string why = g.WhyNotAbility(me);
                    GUI.enabled = why == null && !selAbility;
                    if (G.Button(new Rect(rx + 20, y, 340, 44), selAbility ? "Abilità selezionata: scegli sulla board" : $"USA L'ABILITÀ  (costo {CardRules.Cost(g.AbilityOf(me), g.Preset)})", wrapBtn)) { selAbility = true; selCard = selTarget = -1; dirty = true; }
                    GUI.enabled = true; y += 48;
                    if (why != null) { G.Label(new Rect(rx + 20, y, 340, 44), "<color=#ff9090>" + why + "</color>", tiny); y += 44; }
                }
            }
            if (!cardSel && hand != null && selUnit >= 0 && g.Units[selUnit].Owner == me && g.Units[selUnit].Placed && g.Phase == Phase.Main)
            {   // scelta del bersaglio con più nemici; varianti: attacco senza movimento, restare fermi come mossa
                var choices = g.TargetChoices(me);
                if (choices.Count > 1)
                {
                    int at = choices.FindIndex(t => t.Code == prefer);
                    if (G.Button(new Rect(rx + 20, y, 340, 48), "Se più bersagli sono validi attacca:\n" + (at < 0 ? "scelta automatica (unità, poi Torre, poi mostro)" : choices[at].Label), wrapBtn))
                        prefer = at + 1 < choices.Count ? choices[at + 1].Code : -1;
                    y += 52;
                }
                if (g.Preset.C("attackNoMove") == "si")
                {
                    string w = g.WhyNotAttack(me, selUnit); GUI.enabled = w == null;
                    if (G.Button(new Rect(rx + 20, y, 340, 36), "ATTACCA SENZA MUOVERE")) Act(g.DeclareAttack(me, selUnit, prefer));
                    GUI.enabled = true; y += 40;
                    if (w != null) { G.Label(new Rect(rx + 20, y, 340, 40), "<color=#ff9090>" + w + "</color>", tiny); y += 40; }
                }
                if (g.Preset.C("stayMove") == "si")
                {
                    GUI.enabled = g.WhyNotMove(me, selUnit) == null;
                    if (G.Button(new Rect(rx + 20, y, 340, 36), "RESTA FERMO (vale come movimento)")) Act(g.Move(me, selUnit, g.Units[selUnit].Pos, -1, prefer));
                    GUI.enabled = true; y += 40;
                }
            }
            if ((cardSel || selUnit >= 0) && G.Button(new Rect(rx + 20, y, 340, 32), "Annulla selezione (Esc)")) Deselect();
            if (debugMode) DebugGUI(new Rect(rx + 10, H - 366, RightW - 20, 356));

            // --- basso: che cosa fare adesso, pulsante d'azione, mano ---
            var hr = new Rect(LeftW, H - HandH, W - LeftW - RightW, HandH); Fill(hr, new Color(0.07f, 0.07f, 0.09f, 0.97f));
            var btn = new Rect(hr.xMax - 330, hr.y + 8, 320, 56);
            string hint;
            if (g.Phase == Phase.Over) hint = "Partita finita. Resoconto salvato in " + exported;
            else if (me == Ai) hint = "Tocca all'AI (G2): sta scegliendo la sua mossa. La trovi nel registro degli eventi.";
            else if (hand == null && g.Phase != Phase.Setup) hint = $"Tocca a G{me + 1}. La mano è coperta: premi il pulsante qui sotto quando G{me + 1} è davanti allo schermo.";
            else if (g.Phase == Phase.Setup) hint = $"G{me + 1}: clicca una casella gialla, adiacente alla tua Torre, per scegliere la partenza.";
            else if (g.Phase == Phase.Mulligan)
            {
                hint = $"G{me + 1} — Mulligan (una volta sola): clicca le carte che vuoi cambiare. Vanno in fondo al mazzo e ne peschi altrettante MENO UNA.";
                if (G.Button(new Rect(btn.x, btn.y, 150, btn.height), "Tieni la mano", wrapBtn)) { g.Mulligan(me, new List<int>()); mullSel.Clear(); shownTo = -1; dirty = true; }
                GUI.enabled = mullSel.Count > 0;
                if (G.Button(new Rect(btn.x + 160, btn.y, 160, btn.height), $"Cambia {mullSel.Count}\n(ne peschi {Mathf.Max(0, mullSel.Count - 1)})", wrapBtn)) { g.Mulligan(me, mullSel.ToList()); mullSel.Clear(); shownTo = -1; dirty = true; }
                GUI.enabled = true;
            }
            else if (g.Phase == Phase.Discard) hint = $"G{me + 1}: hai più di {g.Preset.N("handLimit")} carte. Clicca una carta per scartarla.";
            else if (dodgeUnit >= 0) hint = $"<color=#ffcc66>Spostamento in risposta verso {dodgeDest}: clicca nella mano la carta da SCARTARE per pagarlo.</color> Esc per annullare.";
            else if (cardSel)
            {
                var c = sc; string why = SelWhy(me);
                if (why != null) hint = $"<color=#ff9090>Non puoi giocare {c.Name}: {why}</color>";
                else if (g.NeedsTarget(c) && c.Target != "self") hint = selTarget < 0 ? $"{c.Name}: clicca sulla board un'unità col riquadro rosso e la scritta BERSAGLIO." : $"{c.Name}: ora clicca la casella di destinazione (blu).";
                else if (g.NeedsDest(c)) hint = c.Type == CardType.Ally && g.Preset.C("sameName") == "sostituisce" && g.Namesakes(me, c).Count > 0 ? $"{c.Name}: hai già una pedina con questo nome. Clicca la sua casella (blu) per sostituirla: resta dov'è, torna a vita piena e perde il veleno."
                    : c.Type == CardType.Ally ? $"{c.Name}: clicca una casella blu per evocarlo lì." + (g.Preset.C("summonSick") == "si" && !c.Keywords.Contains("swift") ? " Entra tappato: agirà dal tuo prossimo turno." : " Entra pronto.") : c.Target == "self" ? $"{c.Name}: clicca la casella di destinazione (blu)." : $"{c.Name}: clicca una casella blu (in linea di vista). Colpisce quella casella e le 4 adiacenti in orizzontale e verticale, anche le tue unità.";
                else
                {
                    hint = c.Type == CardType.Resource ? $"{c.Name} è una carta risorsa: giocarla è gratis (una per turno). Resta in gioco e dal tuo PROSSIMO turno produce 1 risorsa a ogni inizio turno."
                        : c.Type == CardType.Item ? $"{c.Name}: oggetto, verrà assegnato al tuo Guildmaster. Costa le risorse indicate sulla carta." : $"{c.Name}: premi GIOCA. Costa le risorse indicate sulla carta.";
                    if (G.Button(btn, $"GIOCA\n{c.Name}", wrapBtn)) Act(SelPlay(me, c.Target == "self" ? me : -1, null));
                }
            }
            else if (g.Phase == Phase.Response)
            {
                hint = $"RISPOSTA — G{me + 1} può giocare un'istantanea (anche col Guildmaster tappato)" + (g.Preset.C("gmAbilities") == "si" ? ", usare l'abilità del Guildmaster" : "") + ", spostare una sua unità STAPPATA tappandola" + (g.Preset.C("dodge") == "SCH-SCARTO" ? " e scartando una carta (clic sull'unità, su una casella verde, poi sulla carta)" : " (clic sull'unità, poi su una casella verde)") + (g.Preset.C("dodgeOnlyVsAttack") == "si" ? ", solo se è sotto attacco" : "") + ", oppure passare. Quando passa si risolve la cima della pila." + (g.Stack[g.Stack.Count - 1].Kind == "end" ? " <color=#ffcc66>È la finestra di FINE TURNO: se passi, il turno finisce.</color>" : "");
                if (G.Button(btn, "PASSA (Spazio)", wrapBtn)) Act(g.Pass(me));
            }
            else
            {
                var gm = g.Gm(me); string end = g.WhyNotEndTurn(me);
                hint = "Nel tuo turno: 1) clicca una carta in mano per giocarla (le carte risorsa sono gratis, una per turno; le altre tappano le tue carte risorsa pronte). Per l'abilità clicca il tuo Guildmaster. "
                    + (gm.MovedThisTurn ? "2) Hai già mosso. " : end != null ? "2) <color=#ffcc66>Devi ancora muovere: clicca il tuo Guildmaster, poi una casella verde.</color> " : g.Preset.C("mandatoryMove") == "no" ? "2) Muovere è facoltativo. " : "2) Il Guildmaster non può muoversi: l'obbligo decade. ")
                    + "3) Termina il turno.";
                GUI.enabled = end == null;
                if (G.Button(btn, end == null ? "TERMINA IL TURNO" : "Termina il turno\n(prima devi muovere)", wrapBtn)) Act(g.EndTurn(me));
                GUI.enabled = true;
            }
            G.Label(new Rect(hr.x + 10, hr.y + 6, hr.width - 350, 72), hint, lbl);

            if (me >= 0 && g.Phase != Phase.Setup)
            {
                float cy = hr.y + 84;
                if (hand == null)
                {   // passaggio del controllo: la mano resta coperta finché il giocatore giusto non conferma
                    for (int i = 0; i < g.Players[me].Hand.Count; i++)
                    {
                        var r = new Rect(hr.x + 10 + i * 60, cy + 10, 130, 190); Fill(r, Color.black);
                        var back = assets.CardBack; if (back != null) G.DrawTexture(r, back); else { Fill(new Rect(r.x + 3, r.y + 3, r.width - 6, r.height - 6), new Color(0.2f, 0.15f, 0.3f)); G.Label(new Rect(r.x + 10, r.y + 80, 120, 30), "GUILDS", bold); }
                    }
                    if (me != Ai && G.Button(new Rect(hr.x + hr.width / 2 - 260, cy + 60, 520, 80), $"PASSAGGIO DEL CONTROLLO\nSono G{me + 1}: mostra la mia mano", wrapBtn)) { shownTo = me; Deselect(); }
                }
                else
                {
                    float cw = Mathf.Min(156, (hr.width - 20) / Mathf.Max(1, hand.Count));
                    for (int i = 0; i < hand.Count; i++)
                    {
                        bool picked = g.Phase == Phase.Mulligan ? mullSel.Contains(i) : i == selCard;
                        var r = new Rect(hr.x + 10 + i * cw, cy + (picked ? 0 : 14), 150, 208);
                        bool ok = g.Phase == Phase.Discard || g.Phase == Phase.Mulligan || g.WhyNotPlay(me, i) == null;
                        if (picked) Fill(new Rect(r.x - 3, r.y - 3, r.width + 6, r.height + 6), Color.yellow);
                        var o = GUI.color; if (!ok) GUI.color = new Color(1, 1, 1, 0.5f);
                        DrawCard(r, g.Def(hand[i]), g.Preset, false); GUI.color = o;
                        if (G.Button(r, GUIContent.none, GUIStyle.none))
                        {
                            if (g.Phase == Phase.Mulligan) { if (!mullSel.Remove(i)) mullSel.Add(i); }
                            else if (g.Phase == Phase.Discard) Act(g.DiscardCard(me, i));
                            else if (dodgeUnit >= 0) { int du = dodgeUnit; Act(g.Move(me, du, dodgeDest, i)); }
                            else { selCard = selCard == i ? -1 : i; selTarget = -1; selUnit = -1; selAbility = false; selDrop.Clear(); dirty = true; }
                        }
                    }
                }
            }
            if (shownTo != me && shownTo >= 0 && !showBoth) { shownTo = -1; selCard = selTarget = dodgeUnit = -1; selAbility = false; mullSel.Clear(); dirty = true; } // l'attore è cambiato: copri la mano

            DrawTable(new Rect(LeftW, TopH, TableW, H - TopH - HandH), 0);
            DrawTable(new Rect(W - RightW - TableW, TopH, TableW, H - TopH - HandH), 1);
            BoardLabels();
            if (peek != null)
            {
                var pr = new Rect(BoardRect.center.x - 170, BoardRect.center.y - 250, 340, 470);
                Fill(new Rect(pr.x - 10, pr.y - 10, 360, 520), new Color(0, 0, 0, 0.85f));
                DrawCard(pr, peek, g.Preset, true);
                G.Label(new Rect(pr.x, pr.yMax + 6, 340, 22), "Clic per chiudere", small);
            }
            if (overlay != null) OverlayGUI();

            // --- input sulla board e tastiera ---
            bool onBoard = overlay == null && BoardRect.Contains(mouse);
            if (e.type == EventType.Repaint)
            {
                var hc = onBoard ? view.Pick(cam.ScreenPointToRay(new Vector3(e.mousePosition.x, Screen.height - e.mousePosition.y))) : null;
                if (!System.Nullable.Equals(hc, hover)) { hover = hc; if (SelDef(me) != null) dirty = true; }
            }
            if (onBoard && e.type == EventType.MouseDown && e.button == 0)
            {
                var c = view.Pick(cam.ScreenPointToRay(new Vector3(e.mousePosition.x, Screen.height - e.mousePosition.y)));
                if (c != null) BoardClick(c.Value); else Deselect();
                e.Use();
            }
            if (onBoard && e.type == EventType.MouseDrag && e.button == 1)
            {   // tasto destro: orizzontale = ruota attorno alla board, verticale = inclina
                yaw += e.delta.x * 0.4f; tilt = Mathf.Clamp(tilt + e.delta.y * 0.3f, 25, 89); topView = false; e.Use();
            }
            if (onBoard && e.type == EventType.ScrollWheel) { zoom = Mathf.Clamp(zoom + e.delta.y * 0.03f, 0.5f, 2f); e.Use(); }
            if (e.type == EventType.KeyDown && GUIUtility.keyboardControl == 0)
            {
                if (e.keyCode == KeyCode.T) topView = !topView;
                if (e.keyCode == KeyCode.Escape) { overlay = null; Deselect(); }
                if (e.keyCode == KeyCode.Space && g.Phase == Phase.Response && HandVisible(me)) Act(g.Pass(me));
            }
        }

        void BoardLabels()
        {
            var b = game.Board;
            void Tag(Vector3 world, string text, Color bg)
            {
                var sp = cam.WorldToScreenPoint(view.Root.transform.TransformPoint(world)); if (sp.z < 0) return;
                var size = G.Size(small, text);
                var r = new Rect(sp.x / scale - size.x / 2 - 4, (Screen.height - sp.y) / scale - 11, size.x + 8, 22);
                if (!BoardRect.Contains(r.center)) return;
                if (bg.a > 0) Fill(r, bg);
                G.Label(new Rect(r.x + 4, r.y, r.width, 22), text, small);
            }
            for (int x = 0; x < b.W; x++) Tag(view.World(new Cell(x, -1), 0) + new Vector3(0, 0, 0.25f), ((char)('A' + x)).ToString(), Color.clear);
            for (int yy = 0; yy < b.H; yy++) Tag(view.World(new Cell(-1, yy), 0) + new Vector3(0.25f, 0, 0), (yy + 1).ToString(), Color.clear);
            foreach (var c in b.All())
            {
                string k = b.Code(c);
                if (k == "T1" || k == "T2") Tag(view.World(c, 1.5f), "Torre G" + k[1] + (game.Preset.C("towerHp") == "si" ? $" · vita {Mathf.Max(0, game.TowerHp(k[1] - '1'))}/{game.Preset.N("towerHpMax")}" : ""), new Color(0, 0, 0, 0.6f));
                else if (k == "M") Tag(view.World(c, 1f), game.Preset.C("monsters") != "attivi" ? "Mostro (inerte)" : game.MonsterHp(c) > 0 ? $"Mostro «{game.MonsterKind[c.Name]}» · Att {game.Preset.N("monsterAtk_" + game.MonsterKind[c.Name])} · vita {game.MonsterHp(c)}/{game.MonsterMax(c)} · mazzo {game.MonsterDeck.Count}" : "Mostro sconfitto", new Color(0, 0, 0, 0.6f));
                if (k == "M" && game.Preset.C("monsters") == "attivi" && game.MonsterHp(c) > 0 && game.Phase != Phase.Over)
                {   // spunta per mostro: il giocatore che agisce sceglie di non attaccarlo nel combattimento automatico
                    var sp = cam.WorldToScreenPoint(view.Root.transform.TransformPoint(view.World(c, 1f)));
                    var tr = new Rect(sp.x / scale - 70, (Screen.height - sp.y) / scale + 12, 150, 24);
                    var skip = game.Players[Ai >= 0 ? 1 - Ai : game.Actor].SkipMonsters;
                    if (sp.z > 0 && BoardRect.Contains(tr.center))
                    {
                        Fill(tr, new Color(0, 0, 0, 0.6f));
                        bool on = skip.Contains(c.Name), now = G.Toggle(tr, on, " non attaccare");
                        if (now != on) { if (now) skip.Add(c.Name); else skip.Remove(c.Name); }
                    }
                }
                else if (k == "J") Tag(view.World(c, 0.3f), "Jungle", new Color(0, 0, 0, 0.6f));
            }
            foreach (var u in game.Units.Where(u => u.Placed))
            {
                Tag(view.World(u.Pos, view.Top(u.Pos) + 1.55f), $"G{u.Owner + 1} {game.Def(u).Name.Split(' ')[0]} · Att {game.Attack(u)} · Sal {game.Health(u)}/{game.MaxHealth(u)}" + (game.Preset.C("moveStyle") != "passi" ? " · " + Names.Role(game.RoleOf(u)) : "") + (u.Tapped ? " TAPPATA" : ""),
                    Color.Lerp(assets.GuildColor(game.Def(u).Guild), Color.black, 0.45f));
                if (selTarget < 0 && markedTargets.Contains(u.Id)) Tag(view.World(u.Pos, view.Top(u.Pos) + 1.95f), "BERSAGLIO", new Color(0.8f, 0, 0, 0.9f));
            }
        }

        void OverlayGUI()
        {
            var r = new Rect(LeftW + 20, TopH + 20, W - LeftW - RightW - 40, H - TopH - 60); Fill(r, new Color(0.05f, 0.05f, 0.07f, 0.98f));
            if (G.Button(new Rect(r.xMax - 110, r.y + 8, 100, 32), "Chiudi")) { overlay = null; return; }
            var inner = new Rect(r.x + 14, r.y + 46, r.width - 28, r.height - 56);
            if (overlay == "note")
            {
                G.Label(new Rect(inner.x, r.y + 8, 600, 30), "Note del tester (finiscono nel resoconto)", bold);
                notes = G.TextArea(inner, notes); return;
            }
            if (overlay == "carte")
            {
                G.Label(new Rect(inner.x, r.y + 8, 800, 30), "Mazzi di questa partita (versioni bloccate all'avvio)", bold);
                overlayScroll = G.BeginScrollView(inner, overlayScroll, new Rect(0, 0, inner.width - 24, 2400));
                float y = 0;
                for (int i = 0; i < 2; i++) y += DrawDeck(0, y, inner.width - 30, game.Decks[i], game.Preset, $"G{i + 1} — ");
                G.EndScrollView(); return;
            }
            G.Label(new Rect(inner.x, r.y + 8, 800, 30), $"Regole di questa partita — {game.Preset.Id} {game.Preset.Name}", bold);
            string text = RulesText(game.Preset);
            float h = G.Height(small, text, inner.width - 30);
            overlayScroll = G.BeginScrollView(inner, overlayScroll, new Rect(0, 0, inner.width - 24, h));
            G.Label(new Rect(0, 0, inner.width - 30, h), text, small);
            G.EndScrollView();
        }

        // Debug: solo per riprodurre e indagare bug. Ogni intervento è registrato e invalida la partita per il confronto. La mappa resta bloccata.
        void DebugGUI(Rect r)
        {
            Fill(r, new Color(0.3f, 0.05f, 0.05f, 0.95f));
            G.Label(new Rect(r.x + 6, r.y + 2, r.width, 24), "DEBUG (invalida la partita per il confronto)", small);
            float y = r.y + 28; var g = game;
            showBoth = G.Toggle(new Rect(r.x + 6, y, r.width, 26), showBoth, "Mostra entrambe le mani (niente passaggio)"); y += 28;
            if (showBoth && g.Actor >= 0) G.Label(new Rect(r.x + 6, y, r.width - 12, 60), $"Mano di G{2 - g.Actor}: " + string.Join(", ", g.Players[1 - g.Actor].Hand.Select(c => g.Def(c).Name)), tiny);
            y += 62;
            if (selUnit >= 0)
            {
                var u = g.Units[selUnit]; string n = g.Def(u).Name;
                if (G.Button(new Rect(r.x + 6, y, 110, 30), "+1 danno")) { g.DebugEdit($"{n}: +1 danno", x => u.Damage++); dirty = true; }
                if (G.Button(new Rect(r.x + 120, y, 110, 30), "−1 danno")) { g.DebugEdit($"{n}: −1 danno", x => u.Damage = Mathf.Max(0, u.Damage - 1)); dirty = true; }
                if (G.Button(new Rect(r.x + 234, y, 120, 30), u.Tapped ? "Stappa" : "Tappa")) { g.DebugEdit($"{n}: tap = {!u.Tapped}", x => u.Tapped = !u.Tapped); dirty = true; }
                y += 34;
                debugTeleport = G.Toggle(new Rect(r.x + 6, y, r.width, 26), debugTeleport, "Sposta l'unità selezionata al prossimo clic");
            }
            else G.Label(new Rect(r.x + 6, y, r.width, 26), "Seleziona un'unità per modificarla.", small);
            y += 34;
            if (G.Button(new Rect(r.x + 6, y, 348, 30), "Carica l'ultima situazione salvata"))
            {
                var dir = Path.Combine(ProjectRoot, "reports", "situazioni");
                var f = Directory.Exists(dir) ? Directory.GetFiles(dir, "*.json").OrderBy(x => x).LastOrDefault() : null;
                if (f == null) Say("Nessuna situazione salvata.");
                else
                {
                    game = Json.Read<Game>(f); game.DebugEdit("situazione caricata da " + Path.GetFileName(f), x => { });
                    view.Destroy(); view = new BoardView(); view.Build(game.Board, assets); Deselect(); return;
                }
            }
            y += 34;
            if (g.Actor >= 0)
            {
                var gl = g.Def(g.Gm(g.Actor)).Guild;
                if (G.Button(new Rect(r.x + 6, y, 348, 30), $"+1 risorsa a G{g.Actor + 1}")) g.DebugEdit($"G{g.Actor + 1}: +1 risorsa {Names.Guild(gl)}", x => x.Players[x.Actor].Pool[gl]++);
                y += 34;
                debugCard = G.TextField(new Rect(r.x + 6, y, 200, 30), debugCard);
                if (G.Button(new Rect(r.x + 210, y, 144, 30), "Carta in mano"))
                {
                    if (g.Cards.ContainsKey(debugCard)) { g.DebugEdit($"G{g.Actor + 1}: aggiunta in mano {debugCard}", x => x.Players[x.Actor].Hand.Add(debugCard)); dirty = true; }
                    else Say("ID carta non presente in questa partita. Esempi: " + string.Join(", ", g.Cards.Keys.Take(6)));
                }
            }
        }
    }
}
