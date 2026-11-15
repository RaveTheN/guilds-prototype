// Schermata di configurazione: preset nominati, clonabili e salvabili; scelte esclusive; opzioni non implementate indisponibili;
// validazione che blocca l'avvio; ritocco delle mappe come nuove varianti. La schermata è generata da PresetSchema.
using System.Collections.Generic;
using System.Linq;
using Guilds.Core;
using UnityEngine;

namespace Guilds.Unity
{
    public partial class GuildsApp
    {
        Vector2 presetScroll, paramScroll, infoScroll, deckScroll;
        List<string> deckChanges = new List<string>();
        string seedText, rulesKey, rulesText, deleteAsk; int infoTab;
        float paramH = 4000; // altezza dell'elenco delle regole, misurata al disegno precedente: così l'ultima voce non resta mai fuori
        // ritocco mappa
        MapDef retouchSrc; Dictionary<Cell, string> retouch;

        void SelectPreset(Preset p)
        {
            deckChanges = decks.Where(d => d != null).SelectMany(d => Repo.DeckChanges(d, preset, p)).ToList();
            preset = p; assets = new Assets(DataRoot, p.Theme);
        }

        /// <summary>Tutte le regole dell'inventario divise in: attive, spente ma attivabili da un parametro, non disponibili (col motivo).</summary>
        string RulesText(Preset p)
        {
            string key = p.Id + "|" + p.Map + "|" + string.Join(",", PresetSchema.Params.Select(d => d.IsNumber ? p.N(d.Key).ToString() : p.C(d.Key)));
            if (key == rulesKey) return rulesText;
            var map = Repo.Map(p.Map); var active = Repo.FeaturesFor(p); var on = new HashSet<string>(active.Select(f => f.Code));
            var how = new Dictionary<string, string>(); // codice → come attivarlo
            foreach (var d in PresetSchema.Params.Where(d => !d.IsNumber))
                foreach (var o in d.Options.Where(o => o.Value != p.C(d.Key) && PresetSchema.Unavailable(d.Key, o.Value, map) == null))
                    foreach (var c in o.Codes) if (!on.Contains(c)) how[c] = $"«{d.Label}» → {o.Value}";
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(p.Partial ? "<color=#ffcc66>TEST PARZIALE: non è il regolamento definitivo.</color>" : "").AppendLine();
            sb.AppendLine("<b>PARAMETRI SCELTI</b>").AppendLine($"Mappa: {p.Map} · Tema: {p.Theme}");
            foreach (var d in PresetSchema.Params) sb.AppendLine($"{d.Label}: <b>" + (d.IsNumber ? p.N(d.Key).ToString() : p.C(d.Key) + "</b> — " + d.Options.First(o => o.Value == p.C(d.Key)).Label) + (d.IsNumber ? "</b>" : ""));
            sb.AppendLine().AppendLine($"<color=#7CFC7C><b>REGOLE ATTIVE ({active.Count})</b></color>");
            foreach (var f in active) sb.AppendLine($"<color=#7CFC7C>[ON]</color> <b>{f.Code}</b> {f.Description} <color=#9fb4c8>— {f.Implementation}</color>");
            var off = Repo.Features.Where(f => how.ContainsKey(f.Code)).ToList();
            sb.AppendLine().AppendLine($"<color=#ffcc66><b>SPENTE, ATTIVABILI DAI PARAMETRI ({off.Count})</b></color>");
            foreach (var f in off) sb.AppendLine($"<color=#ffcc66>[off]</color> <b>{f.Code}</b> {f.Description} <color=#ffcc66>— per attivarla: {how[f.Code]}</color>");
            var no = Repo.Features.Where(f => !on.Contains(f.Code) && !how.ContainsKey(f.Code)).ToList();
            sb.AppendLine().AppendLine($"<color=#aaaaaa><b>NON DISPONIBILI IN QUESTA VERSIONE DEL PROTOTIPO ({no.Count})</b>");
            foreach (var f in no) sb.AppendLine($"[--] <b>{f.Code}</b> {f.Description} — {(f.Choice == "X" ? "esclusa nell'inventario (X)" : f.Choice == "" ? "non decisa nell'inventario" : "scelta " + f.Choice + ", non ancora implementata")}");
            sb.AppendLine("</color>");
            sb.AppendLine("<b>IPOTESI PROVVISORIE</b>"); foreach (var s in p.Hypotheses) sb.AppendLine("• " + s);
            sb.AppendLine().AppendLine("<b>LIMITI</b>"); foreach (var s in p.Limits) sb.AppendLine("• " + s);
            rulesKey = key; return rulesText = sb.ToString();
        }

        void ConfigGUI()
        {
            cam.rect = new Rect(0, 0, 1, 1);
            Fill(new Rect(0, 0, W, H), new Color(0.1f, 0.1f, 0.13f));
            G.Label(new Rect(20, 10, 900, 40), $"GUILDS — prototipo di playtest {Repo.PrototypeVersion}", title);
            if (retouchSrc != null) { RetouchGUI(); return; }

            // --- colonna 1: preset ---
            float c1 = 310;
            G.Label(new Rect(20, 60, c1, 28), "1. Scegli un preset", bold);
            presetScroll = G.BeginScrollView(new Rect(20, 90, c1, 520), presetScroll, new Rect(0, 0, c1 - 24, Repo.Presets.Count * 64));
            for (int i = 0; i < Repo.Presets.Count; i++)
            {
                var p = Repo.Presets[i];
                var o = GUI.color; if (p == preset) GUI.color = new Color(0.6f, 1, 0.6f);
                if (G.Button(new Rect(0, i * 64, c1 - 26, 58), $"{p.Id}{(p.BuiltIn ? "" : " (tuo)")}\n{p.Name}", wrapBtn) && p != preset) SelectPreset(p);
                GUI.color = o;
            }
            G.EndScrollView();
            if (G.Button(new Rect(20, 620, 98, 40), "Clona")) Editable(true);
            GUI.enabled = !preset.BuiltIn;
            if (G.Button(new Rect(126, 620, 98, 40), "Salva")) { Repo.SavePreset(preset); Say("Preset salvato in data/presets/user/" + preset.Id + ".json"); }
            // eliminazione definitiva: chiede un secondo clic di conferma
            bool ask = deleteAsk == preset.Id; var oc = GUI.color; if (ask) GUI.color = new Color(1, 0.5f, 0.5f);
            if (G.Button(new Rect(232, 620, 98, 40), ask ? "Confermi?" : "Elimina"))
            {
                if (!ask) { deleteAsk = preset.Id; Say($"Clicca di nuovo per eliminare definitivamente «{preset.Id}». I preset di base non si possono eliminare."); }
                else { string id = preset.Id; Repo.DeletePreset(preset); deleteAsk = null; SelectPreset(Repo.Presets[0]); Say($"Preset «{id}» eliminato."); return; }
            }
            GUI.color = oc; GUI.enabled = true;
            if (preset.BuiltIn) G.Label(new Rect(20, 668, c1, 70), "<color=#ffcc66>Preset di base. Puoi cambiare qualsiasi regola qui a destra: alla prima modifica ne creo una copia tua.</color>", small);
            else { G.Label(new Rect(20, 668, c1, 24), "Nome del tuo preset:", small); preset.Name = G.TextField(new Rect(20, 694, c1, 32), preset.Name); }
            G.Label(new Rect(20, 745, c1, 300), preset.Description + (preset.Partial ? "\n<color=#ffcc66>TEST PARZIALE</color>" : ""), small);

            // --- colonna 2: regole selezionabili (valgono dalla partita successiva) ---
            float x = 350, cw = Mathf.Clamp(W - 350 - 600, 430, 660); var map = Repo.Map(preset.Map);
            G.Label(new Rect(x, 60, cw, 28), "2. Regole: clicca per cambiarle", bold);
            paramScroll = G.BeginScrollView(new Rect(x, 90, cw, H - 110), paramScroll, new Rect(0, 0, cw - 24, paramH));
            float y = 0, rw = cw - 30; int grp = 0;
            // sfondo alternato per gruppo di regole (non per singola voce): nero, grigio molto scuro, nero…
            void Band(float h) => Fill(new Rect(0, y, cw - 24, h), grp % 2 == 0 ? Color.black : new Color(0.15f, 0.15f, 0.17f));
            void Row(string text, bool on, string why, System.Action pick)
            {
                float h = Mathf.Max(28, G.Height(small, text, rw - 30));
                Band(h + 2);
                if (on) Fill(new Rect(0, y, rw, h), new Color(0.15f, 0.45f, 0.2f));
                GUI.enabled = why == null;
                if (G.Toggle(new Rect(4, y + 2, 24, 24), on, "") && !on) pick();
                if (G.Button(new Rect(30, y, rw - 30, h), GUIContent.none, GUIStyle.none) && !on && why == null) pick();
                GUI.enabled = true;
                G.Label(new Rect(30, y + 2, rw - 34, h), (why != null ? "<color=#888888>" : on ? "<b>" : "") + text + (why != null ? "</color>" : on ? "</b>" : ""), small);
                y += h + 2;
                if (why != null) { float hh = G.Height(tiny, "Non selezionabile: " + why, rw - 40); Band(hh); G.Label(new Rect(30, y - 2, rw - 40, hh), "<color=#ff9090>Non selezionabile: " + why + "</color>", tiny); y += hh; }
            }
            Band(26); G.Label(new Rect(4, y, cw, 24), "Mappa", bold); y += 26;
            foreach (var m in Repo.Maps)
            {
                float y0 = y; rw = cw - 30 - 160;
                Row(m.Id + (m.BaseMap != null ? $" — variante di {m.BaseMap} ({m.ChangedCells.Count} caselle cambiate)" : " — originale"), preset.Map == m.Id, null, () => Editable().Map = m.Id);
                rw = cw - 30;
                if (G.Button(new Rect(cw - 30 - 152, y0, 150, 26), "Ritocca…")) { retouchSrc = m; retouch = new Dictionary<Cell, string>(); }
            }
            grp++; Band(34); y += 8; G.Label(new Rect(4, y, cw, 24), "Tema visivo (presentazione)", bold); y += 26;
            foreach (var t in Assets.ThemeNames(DataRoot)) Row(t, preset.Theme == t, null, () => { Editable().Theme = t; assets = new Assets(DataRoot, t); });
            foreach (var d in PresetSchema.Params)
            {
                grp++; Band(8); y += 8;
                if (d.IsNumber)
                {
                    int v = preset.N(d.Key);
                    Band(28); G.Label(new Rect(4, y, rw - 134, 26), d.Label, small);
                    GUI.enabled = v > d.Min; if (G.Button(new Rect(rw - 120, y, 34, 26), "−")) Editable().Numbers[d.Key] = v - 1;
                    GUI.enabled = true; G.Label(new Rect(rw - 76, y, 40, 26), v.ToString(), bold);
                    GUI.enabled = v < d.Max; if (G.Button(new Rect(rw - 40, y, 34, 26), "+")) Editable().Numbers[d.Key] = v + 1;
                    GUI.enabled = true; y += 28; continue;
                }
                Band(26); G.Label(new Rect(4, y, cw, 24), d.Label, bold); y += 26;
                if (d.Note != null) { Band(18); G.Label(new Rect(4, y - 4, rw, 20), d.Note, tiny); y += 18; }
                foreach (var o in d.Options)
                {
                    string codes = o.Codes.Length > 0 ? "  [" + string.Join(", ", o.Codes) + "]" : "";
                    var dk = d.Key; var ov = o.Value;
                    Row($"{o.Value} — {o.Label}{codes}", preset.C(d.Key) == o.Value, PresetSchema.Unavailable(d.Key, o.Value, map), () => Editable().Choices[dk] = ov);
                }
            }
            paramH = y + 40;
            G.EndScrollView();

            // --- colonna 3: mazzi, seed, avvio, elenco delle regole ---
            x = 350 + cw + 20; cw = W - x - 20;
            G.Label(new Rect(x, 60, cw, 28), "3. Mazzi e avvio", bold);
            for (int i = 0; i < 2; i++)
            {
                G.Label(new Rect(x, 92 + i * 40, 40, 30), $"G{i + 1}", bold);
                if (G.Button(new Rect(x + 44, 92 + i * 40, Mathf.Min(460, cw - 44), 34), randomDecks ? "estratto a caso all'avvio" : decks[i].Name + "  (clic per cambiare)") && !randomDecks) decks[i] = Repo.Decks[(Repo.Decks.IndexOf(decks[i]) + 1) % Repo.Decks.Count];
            }
            randomDecks = G.Toggle(new Rect(x + 44 + Mathf.Min(460, cw - 44) + 12, 95, 260, 28), randomDecks, "Mazzi casuali all'avvio");
            G.Label(new Rect(x, 176, 60, 30), "Seed", lbl);
            seedText = G.TextField(new Rect(x + 60, 176, 140, 30), seedText ?? seed.ToString());
            if (int.TryParse(seedText, out int sd)) seed = sd;
            autoPass = G.Toggle(new Rect(x + 220, 176, cw - 220, 28), autoPass, "Passa da solo se non può rispondere");
            debugMode = G.Toggle(new Rect(x, 212, cw, 28), debugMode, "Modalità debug (la partita non vale per il confronto)");

            vsAi = G.Toggle(new Rect(x, 244, cw / 2, 28), vsAi, "G2 è guidato dall'AI");
            GUI.enabled = !vsAi; // contro l'AI non c'è nessun passaggio: la mano dell'AI resta sempre coperta
            handoff = G.Toggle(new Rect(x + cw / 2, 244, cw / 2, 28), handoff && !vsAi, "Passaggio del controllo (copre la mano)") || (vsAi && handoff);

            var errors = Repo.Validate(preset, decks[0], decks[1]);
            GUI.enabled = errors.Count == 0;
            if (G.Button(new Rect(x, 284, 240, 60), errors.Count == 0 ? "AVVIA PARTITA" : "Configurazione\nnon valida", wrapBtn))
            {
                if (!preset.BuiltIn) Repo.SavePreset(preset);
                StartGame(); return;
            }
            GUI.enabled = true;
            string msg = string.Join("\n", errors.Select(s => "<color=#ff9090>Errore: " + s + "</color>")
                .Concat(map == null ? new string[0] : MapOps.Warnings(map).Select(s => "<color=#ffcc66>Nota: " + s + "</color>"))
                .Concat(deckChanges.Select(s => "<color=#9fd0ff>Cambio: " + s + "</color>")));
            if (errors.Count == 0) msg += "\n<color=#9fd0ff>Mazzi validi per questo preset.</color>";
            G.Label(new Rect(x + 250, 280, cw - 250, 116), msg, tiny);

            string[] tabs = { "Tutte le regole", "Mazzi e carte" };
            infoTab = Mathf.Min(infoTab, 1);
            infoTab = G.Toolbar(new Rect(x, 400, cw, 32), infoTab, tabs);
            var ir = new Rect(x, 438, cw, H - 450);
            if (infoTab == 1)
            {
                deckScroll = G.BeginScrollView(ir, deckScroll, new Rect(0, 0, cw - 24, 2600));
                float dy = 0;
                for (int i = 0; i < 2; i++) dy += DrawDeck(0, dy, cw - 30, decks[i], preset, $"G{i + 1} — ");
                G.EndScrollView();
            }
            else
            {
                string text = RulesText(preset);
                float h = G.Height(small, text, cw - 34);
                infoScroll = G.BeginScrollView(ir, infoScroll, new Rect(0, 0, cw - 24, h));
                G.Label(new Rect(0, 0, cw - 34, h), text, small);
                G.EndScrollView();
            }
        }

        // Ritocco fra una partita e l'altra: solo il tipo di caselle esistenti; si salva solo come nuova variante.
        void RetouchGUI()
        {
            var b = new Board(retouchSrc); float cs = Mathf.Min(70, (H - 260) / b.H), ox = 40, oy = 110;
            G.Label(new Rect(20, 56, 1200, 28), $"Ritocco di {retouchSrc.Id}: clic su una casella per cambiarne il tipo. L'originale non viene modificato.", bold);
            string Now(Cell c) => retouch.TryGetValue(c, out var v) ? v : b.Code(c);
            for (int yy = 0; yy < b.H; yy++) for (int xx = 0; xx < b.W; xx++)
            {
                var c = new Cell(xx, yy); if (!b.InMap(c)) continue;
                string k = Now(c); var r = new Rect(ox + xx * cs, oy + (b.H - 1 - yy) * cs, cs - 3, cs - 3);
                Fill(r, assets.CellColor(k)); if (k != b.Code(c)) Fill(new Rect(r.x, r.y, r.width, 5), Color.yellow);
                if (G.Button(r, GUIContent.none, GUIStyle.none))
                {
                    string nx = MapOps.CellTypes[(System.Array.IndexOf(MapOps.CellTypes, k) + 1) % MapOps.CellTypes.Length];
                    if (nx == b.Code(c)) retouch.Remove(c); else retouch[c] = nx;
                }
                G.Label(new Rect(r.x + 4, r.y + 6, cs, 24), $"{c.Name} {k}", k == "W" ? small : darkSmall);
            }
            float x = ox + b.W * cs + 40, w = W - x - 20;
            G.Label(new Rect(x, oy, w, 120), "Tipi (in ciclo): . normale · W muro · H highground · J Jungle · M mostro · T1/T2 Torre.\nNiente caselle nuove, niente cambio di dimensione. La striscia gialla segna le caselle cambiate.", lbl);
            string id = Repo.NextMapVariantId(retouchSrc);
            var variant = MapOps.MakeVariant(retouchSrc, retouch, id);
            var errs = MapOps.Validate(variant, preset.C("start"));
            if (retouch.Count == 0) errs.Add("Nessuna casella cambiata.");
            string text = $"<b>Caselle cambiate ({variant.ChangedCells.Count - retouchSrc.ChangedCells.Count})</b>\n" + string.Join("\n", variant.ChangedCells.Skip(retouchSrc.ChangedCells.Count))
                + "\n\n" + string.Join("\n", errs.Select(s => "<color=#ff9090>Errore: " + s + "</color>").Concat(MapOps.Warnings(variant).Select(s => "<color=#ffcc66>Nota: " + s + "</color>")));
            G.Label(new Rect(x, oy + 130, w, 520), text, small);
            GUI.enabled = errs.Count == 0;
            if (G.Button(new Rect(x, oy + 660, 340, 48), $"Salva come variante {id}"))
            {
                var res = Repo.SaveMapVariant(variant, preset.C("start"));
                if (res.Count > 0) Say(string.Join(" ", res));
                else { Editable().Map = id; Say($"Variante salvata in data/maps/{id}.json e selezionata nel preset «{preset.Id}»."); retouchSrc = null; }
            }
            GUI.enabled = true;
            if (G.Button(new Rect(x + 360, oy + 660, 200, 48), "Annulla")) retouchSrc = null;
        }
    }
}
