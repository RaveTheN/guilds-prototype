// Applicazione: stato dell'interfaccia, camera, disegno condiviso. Le regole stanno tutte in Guilds.Core.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Guilds.Core;
using UnityEngine;

namespace Guilds.Unity
{
    public partial class GuildsApp : MonoBehaviour
    {
        enum Mode { Config, Play }
        Mode mode = Mode.Config;
        public Repo Repo; public string DataRoot, ProjectRoot;
        Preset preset; readonly DeckDef[] decks = new DeckDef[2];
        int seed; bool debugMode, autoPass = true, handoff = true, vsAi, randomDecks; float aiAt; Rng aiRng;
        int Ai => vsAi ? 1 : -1; // l'AI guida sempre G2
        Game game; BoardView view; Assets assets; Camera cam;
        DateTime started; string notes = "", flash = ""; float flashUntil;

        const float H = 1080; float W, scale;
        GUIStyle lbl, small, bold, title, dark, darkSmall, center, wrapBtn, tiny; float styleScale;

        void Awake()
        {
            ProjectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            DataRoot = Path.Combine(ProjectRoot, "data"); // in una build: cartella data accanto all'eseguibile
            Repo = Repo.Load(DataRoot);
            preset = Repo.PresetById("P01"); decks[0] = Repo.Deck("deck_red"); decks[1] = Repo.Deck("deck_green");
            seed = Environment.TickCount & 0xFFFFF;
            assets = new Assets(DataRoot, preset.Theme);
            cam = Camera.main;
            if (cam == null) { cam = new GameObject("Camera").AddComponent<Camera>(); cam.tag = "MainCamera"; }
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.12f, 0.13f, 0.16f);
            if (FindFirstObjectByType<Light>() == null)
            {
                var l = new GameObject("Light").AddComponent<Light>(); l.type = LightType.Directional;
                l.transform.rotation = Quaternion.Euler(55, -30, 0); l.intensity = 1.1f;
            }
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.55f);
            var shot = Environment.GetCommandLineArgs().SkipWhile(a => a != "-guildsShot").Skip(1).FirstOrDefault();
            if (shot != null) StartCoroutine(ShotRun(shot));
        }

        void InitStyles()
        {
            if (styleScale == scale) return; styleScale = scale;
            int F(float size) => Mathf.Max(8, Mathf.RoundToInt(size * scale)); // dimensione reale: niente ingrandimento di bitmap
            lbl = new GUIStyle(GUI.skin.label) { fontSize = F(17), wordWrap = true, richText = true };
            small = new GUIStyle(lbl) { fontSize = F(14) };
            tiny = new GUIStyle(lbl) { fontSize = F(12) };
            bold = new GUIStyle(lbl) { fontStyle = FontStyle.Bold };
            title = new GUIStyle(lbl) { fontSize = F(24), fontStyle = FontStyle.Bold };
            dark = new GUIStyle(lbl) { normal = { textColor = Color.black } };
            darkSmall = new GUIStyle(small) { normal = { textColor = Color.black } };
            center = new GUIStyle(title) { alignment = TextAnchor.MiddleCenter };
            wrapBtn = new GUIStyle(GUI.skin.button) { fontSize = F(16), wordWrap = true, richText = true };
            GUI.skin.button.fontSize = F(16); GUI.skin.toggle.fontSize = F(16); GUI.skin.textField.fontSize = F(16); GUI.skin.textArea.fontSize = F(16);
        }

        void OnGUI()
        {
            scale = Screen.height / H; W = Screen.width / scale; G.S = scale;
            InitStyles();
            if (mode == Mode.Config) ConfigGUI(); else PlayGUI();
            if (Time.unscaledTime < flashUntil)
            {
                var r = new Rect(W / 2 - 450, 70, 900, 64); Fill(r, new Color(0.55f, 0.1f, 0.1f, 0.95f));
                G.Label(new Rect(r.x + 10, r.y + 4, r.width - 20, r.height - 8), flash, lbl);
            }
        }

        /// <summary>Mostra il motivo di un'azione vietata (o un avviso). null = niente da dire.</summary>
        bool Say(string why) { if (why == null) return true; flash = why; flashUntil = Time.unscaledTime + 6; return false; }

        /// <summary>Carte di un mazzo con il numero di copie, risorse comprese. Restituisce l'altezza usata.</summary>
        float DrawDeck(float x, float y, float width, DeckDef d, Preset p, string owner)
        {
            G.Label(new Rect(x, y, width, 26), $"{owner}{d.Name}: {d.Cards.Sum(e => e.Count)} carte + Guildmaster", bold);
            var items = new[] { (d.Guildmaster, 0) }.Concat(d.Cards.Select(e => (e.Card, e.Count))).ToList();
            int per = Mathf.Max(1, (int)(width / 196));
            for (int i = 0; i < items.Count; i++)
            {
                var c = Repo.Card(items[i].Item1, p) ?? Repo.AnyVersion(items[i].Item1); if (c == null) continue;
                var r = new Rect(x + i % per * 196, y + 30 + i / per * 286, 190, 280);
                DrawCard(r, c, p, false);
                string badge = items[i].Item2 == 0 ? "GM" : "×" + items[i].Item2;
                Fill(new Rect(r.xMax - 52, r.yMax - 34, 48, 30), Color.black); G.Label(new Rect(r.xMax - 46, r.yMax - 33, 44, 28), badge, bold);
            }
            return 30 + (items.Count + per - 1) / per * 286 + 10;
        }

        /// <summary>Preset modificabile: se quello corrente è di base ne crea una copia e passa a quella.</summary>
        Preset Editable(bool forceCopy = false)
        {
            if (!preset.BuiltIn && !forceCopy) return preset;
            bool auto = preset.BuiltIn;
            var c = Json.Clone(preset); int n = 1; while (Repo.PresetById($"{preset.Id}-copia{n}") != null) n++;
            c.Id = $"{preset.Id}-copia{n}"; c.Name = preset.Name + " (copia)"; c.BasedOn = preset.Id; c.BuiltIn = false;
            Repo.SavePreset(c); preset = c;
            Say(auto ? $"«{c.BasedOn}» è un preset di base: ho creato la copia «{c.Id}» e la modifica vale per quella." : $"Creata la copia «{c.Id}».");
            return c;
        }

        // ---------- disegno condiviso ----------
        static void Fill(Rect r, Color c) { var o = GUI.color; GUI.color = c; G.DrawTexture(r, Texture2D.whiteTexture); GUI.color = o; }

        /// <summary>Costo con i simboli di gilda del preset (immagine esterna o sigla testuale).</summary>
        void DrawSymbols(Rect r, Guild g, int count, int generic, Preset p, bool rightAlign)
        {
            float s = r.height, w = count * (s + 2) + (generic > 0 ? s : 0);
            float x = rightAlign ? r.xMax - w : r.x;
            if (generic > 0) { Fill(new Rect(x, r.y, s - 2, s), new Color(0.85f, 0.85f, 0.85f)); G.Label(new Rect(x + 5, r.y - 2, s, s + 4), generic.ToString(), dark); x += s; }
            var t = assets.Symbol(Names.SymbolId(g, p));
            for (int i = 0; i < count; i++, x += s + 2)
            {
                if (t != null) G.DrawTexture(new Rect(x, r.y, s, s), t, ScaleMode.ScaleToFit);
                else { Fill(new Rect(x, r.y, s, s), assets.GuildColor(g)); G.Label(new Rect(x + 3, r.y - 2, s, s + 4), Names.Symbol(g, p).Substring(1, 1), dark); }
            }
        }

        /// <summary>Carta generata dai dati: nome, costo, illustrazione (file esterno o segnaposto), tipo, testo effettivo.</summary>
        void DrawCard(Rect r, CardDef c, Preset p, bool big)
        {
            var gc = assets.GuildColor(c.Guild);
            Fill(r, Color.black); var n = new Rect(r.x + 3, r.y + 3, r.width - 6, r.height - 6); Fill(n, Color.Lerp(gc, Color.black, 0.3f));
            float th = big ? 36 : 40;
            G.Label(new Rect(n.x + 6, n.y + 2, n.width - (big ? 110 : 12), th), c.Name, big ? bold : small);
            if (c.Type != CardType.Guildmaster && c.Type != CardType.Resource)
                DrawSymbols(new Rect(n.x, big ? n.y + 5 : n.y + th, n.width - 6, big ? 26 : 20), c.Guild, c.CostColored, c.CostGeneric, p, true);
            var art = new Rect(n.x + 6, n.y + th + (big ? 2 : 22), n.width - 12, n.height * (big ? 0.36f : 0.22f));
            Fill(art, Color.Lerp(gc, Color.white, 0.65f));
            var tex = assets.CardArt(c.Id) ?? assets.Symbol(Names.SymbolId(c.Guild, p));
            if (tex != null) G.DrawTexture(art, tex, ScaleMode.ScaleToFit);
            string type = Names.Type(c.Type) + (c.Type == CardType.Guildmaster ? $" — Att {c.Attack} / Sal {c.Health}" : "");
            G.Label(new Rect(n.x + 6, art.yMax + 1, n.width - 12, 24), type, big ? lbl : small);
            var txt = new Rect(n.x + 6, art.yMax + 26, n.width - 12, n.yMax - art.yMax - 32); Fill(txt, new Color(0.95f, 0.87f, 0.68f));
            string body = CardRules.Text(c) + (big && c.DeferredText != null ? "\n\n<i>Non attivo in questo set: " + c.DeferredText + "</i>" : "");
            G.Label(new Rect(txt.x + 5, txt.y + 3, txt.width - 10, txt.height - 6), body, big ? dark : darkSmall);
        }

        // ---------- camera ----------
        float tilt = 62, yaw = 0, zoom = 1; bool topView;
        void PlaceCamera(Rect guiRect)
        {
            cam.rect = new Rect(guiRect.x / W, 1 - guiRect.yMax / H, guiRect.width / W, guiRect.height / H);
            float t = topView ? 90 : tilt, d = (view != null ? view.Extent : 8) * 1.25f * zoom;
            cam.fieldOfView = 50;
            cam.transform.rotation = Quaternion.Euler(t, yaw, 0);
            cam.transform.position = new Vector3(0, 0.3f, 0) - cam.transform.forward * d;
        }

        // ---------- avvio / fine partita ----------
        void StartGame()
        {
            if (randomDecks)
            {   // stesso seed = stessi mazzi; «Riavvia» cambia seed e quindi li estrae di nuovo
                var r = new Rng(seed + 77); var ok = Repo.Decks.Where(d => Repo.Validate(preset, d, d).Count == 0).ToList();
                if (ok.Count > 0) for (int i = 0; i < 2; i++) decks[i] = ok[r.Next(ok.Count)];
            }
            game = Repo.NewGame(preset, decks[0], decks[1], seed);
            if (randomDecks) game.Say(-1, "info", $"Mazzi estratti a caso: G1 {decks[0].Name}, G2 {decks[1].Name}.", true);
            assets = new Assets(DataRoot, preset.Theme);
            view?.Destroy(); view = new BoardView(); view.Build(game.Board, assets);
            for (int i = 0; i < 2; i++) view.SetTowerColor(i, assets.GuildColor(game.Def(game.Gm(i)).Guild));
            started = DateTime.Now; notes = ""; exported = null; selCard = selTarget = selUnit = -1; selAbility = false; shownTo = -1; showBoth = false; overlay = null; mullSel.Clear(); yaw = 0;
            aiRng = new Rng(seed + 1000);
            if (vsAi) game.Say(-1, "info", "G2 è guidato dall'AI (giocatore automatico semplice).", true);
            mode = Mode.Play; dirty = true;
        }

        string ExportReport()
        {
            var dir = Path.Combine(ProjectRoot, "reports"); Directory.CreateDirectory(dir);
            var f = Path.Combine(dir, $"resoconto_{started:yyyyMMdd_HHmmss}_{game.Preset.Id}_seed{game.Seed}.md");
            File.WriteAllText(f, Report.Markdown(game, started, DateTime.Now, notes));
            return f;
        }

        string SaveSituation()
        {
            var dir = Path.Combine(ProjectRoot, "reports", "situazioni"); Directory.CreateDirectory(dir);
            var f = Path.Combine(dir, $"situazione_{DateTime.Now:yyyyMMdd_HHmmss}_T{game.Turn}.json");
            File.WriteAllText(f, Json.Write(game));
            return f;
        }

        // ---------- verifica automatica con schermate (riga di comando: -guildsShot <cartella>) ----------
        IEnumerator ShotRun(string dir)
        {
            Directory.CreateDirectory(dir);
            IEnumerator Shot(string name) { yield return new WaitForSeconds(0.7f); ScreenCapture.CaptureScreenshot(Path.Combine(dir, name + ".png")); yield return new WaitForSeconds(0.7f); }
            yield return Shot("1_config");
            StartGame(); shownTo = game.Actor;
            yield return Shot("2a_mulligan");
            while (game.Phase == Phase.Mulligan) game.Mulligan(game.Actor, new List<int>());
            shownTo = game.Actor; dirty = true;
            yield return Shot("2_setup");
            while (game.Phase == Phase.Setup) game.ChooseStart(game.Actor, game.StartOptions(game.Actor)[0]);
            shownTo = game.Actor; selUnit = game.Actor; dirty = true;
            yield return Shot("3_mosse");
            var r = new Rng(3);
            for (int i = 0; i < 40 && game.Phase != Phase.Over && game.Turn < 6; i++) Bot.Step(game, r);
            shownTo = game.Actor; selUnit = -1; selCard = game.Players[game.Actor].Hand.Count > 0 ? 0 : -1; dirty = true;
            yield return Shot("4_partita");
            topView = true; overlay = "regole";
            yield return Shot("5_regole_vista_alto");
            File.WriteAllText(Path.Combine(dir, "ok.txt"), ExportReport());
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(0);
#endif
        }
    }
}
