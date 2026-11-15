// Guilds.Core — caricamento dei dati (cartella data/), validazioni di preset e mazzi, resoconto della partita.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Guilds.Core
{
    public static class Json
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false } },
            Converters = { new StringEnumConverter() }
        };
        public static T Read<T>(string path) => JsonConvert.DeserializeObject<T>(File.ReadAllText(path, Encoding.UTF8), Settings);
        public static string Write(object o) => JsonConvert.SerializeObject(o, Settings);
        public static T Clone<T>(T o) => JsonConvert.DeserializeObject<T>(Write(o), Settings);
    }

    public class Repo
    {
        public const string PrototypeVersion = "0.12.3 (M4+)";
        public string Root;
        public Dictionary<string, List<CardDef>> CardVersions = new Dictionary<string, List<CardDef>>();
        public List<DeckDef> Decks = new List<DeckDef>();
        public List<MapDef> Maps = new List<MapDef>();
        public List<Preset> Presets = new List<Preset>();
        public List<FeatureInfo> Features = new List<FeatureInfo>();

        static IEnumerable<string> Files(string dir) => Directory.Exists(dir) ? Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal) : new string[0];

        public static Repo Load(string root)
        {
            var r = new Repo { Root = root };
            foreach (var f in Files(Path.Combine(root, "cards")))
                foreach (var c in Json.Read<List<CardDef>>(f))
                {
                    if (c.Ability != null)
                    {   // l'abilità è descritta come una carta istantanea senza mano: stessi effetti, stessa validazione
                        c.Ability.Id = c.Id + ":ability"; c.Ability.Version = c.Version; c.Ability.Guild = c.Guild; c.Ability.Type = CardType.Instant;
                        c.Ability.Name = c.Ability.Name ?? "Abilità di " + c.Name;
                    }
                    if (!r.CardVersions.TryGetValue(c.Id, out var l)) r.CardVersions[c.Id] = l = new List<CardDef>();
                    l.Add(c);
                }
            r.Decks = Files(Path.Combine(root, "decks")).Select(Json.Read<DeckDef>).ToList();
            r.Maps = Files(Path.Combine(root, "maps")).Select(Json.Read<MapDef>).ToList();
            r.Presets = Files(Path.Combine(root, "presets")).Select(Json.Read<Preset>).ToList();
            string hf = Path.Combine(root, "presets", "user", "hidden.txt");
            if (File.Exists(hf)) r.Hidden = new HashSet<string>(File.ReadAllLines(hf).Where(l => l.Trim() != ""));
            r.Features = JObject.Parse(File.ReadAllText(Path.Combine(root, "features", "features.json")))["features"].ToObject<List<FeatureInfo>>();
            return r;
        }

        public MapDef Map(string id) => Maps.FirstOrDefault(m => m.Id == id);
        public Preset PresetById(string id) => Presets.FirstOrDefault(p => p.Id == id);
        // Preset di base «eliminati»: restano nei dati (gen_content li rigenera, i test li usano) ma spariscono dall'elenco.
        public HashSet<string> Hidden = new HashSet<string>();
        string HiddenFile => Path.Combine(Root, "presets", "user", "hidden.txt");
        public List<Preset> Visible => Presets.Where(p => !Hidden.Contains(p.Id)).ToList();
        public void RestorePresets() { Hidden.Clear(); if (File.Exists(HiddenFile)) File.Delete(HiddenFile); }
        public DeckDef Deck(string id) => Decks.FirstOrDefault(d => d.Id == id);

        /// <summary>Versione più recente della carta compatibile col preset, oppure null.</summary>
        public CardDef Card(string id, Preset p) =>
            CardVersions.TryGetValue(id, out var l) ? l.OrderByDescending(c => c.Version).FirstOrDefault(c => CardRules.Missing(c, p).Count == 0) : null;
        public CardDef AnyVersion(string id) => CardVersions.TryGetValue(id, out var l) ? l.OrderByDescending(c => c.Version).First() : null;

        public List<string> ValidateDeck(DeckDef d, Preset p)
        {
            var err = new List<string>();
            string Problem(string id)
            {
                var any = AnyVersion(id);
                return any == null ? $"carta «{id}» inesistente" : Card(id, p) == null ? $"«{any.Name}» non ha una versione compatibile: richiede {string.Join(", ", CardRules.Missing(any, p))}" : null;
            }
            var gmProblem = Problem(d.Guildmaster); var gm = Card(d.Guildmaster, p);
            if (gmProblem != null) err.Add(gmProblem);
            else if (gm.Type != CardType.Guildmaster) err.Add($"«{gm.Name}» non è un Guildmaster.");
            int n = d.Cards.Sum(e => e.Count);
            if (n < p.N("deckMin")) err.Add($"Il mazzo ha {n} carte: il minimo è {p.N("deckMin")} (F1-04).");
            foreach (var e in d.Cards)
            {
                var pr = Problem(e.Card); if (pr != null) { err.Add(pr); continue; }
                var c = Card(e.Card, p);
                if (c.Type == CardType.Guildmaster) err.Add($"«{c.Name}»: il Guildmaster non sta nel mazzo.");
                if (c.Type != CardType.Resource && e.Count > p.N("maxCopies")) err.Add($"«{c.Name}»: {e.Count} copie, massimo {p.N("maxCopies")} (F1-05).");
                if (gm != null && c.Guild != gm.Guild) err.Add($"«{c.Name}» non è della gilda del Guildmaster (F3-02, ipotesi H16).");
            }
            return err;
        }

        /// <summary>Che cosa cambia in un mazzo passando da un preset all'altro (mai modifiche silenziose).</summary>
        public List<string> DeckChanges(DeckDef d, Preset from, Preset to)
        {
            var ch = new List<string>();
            foreach (var id in d.Cards.Select(e => e.Card).Concat(new[] { d.Guildmaster }).Distinct())
            {
                var a = from == null ? null : Card(id, from); var b = Card(id, to);
                if (b == null || a?.Key != b.Key) ch.Add($"{d.Name}: {AnyVersion(id)?.Name ?? id}: {a?.Key ?? "non compatibile"} → {b?.Key ?? "NON COMPATIBILE (variante mancante)"}");
            }
            return ch;
        }

        public List<FeatureInfo> FeaturesFor(Preset p)
        {
            var codes = new HashSet<string>(Features.Where(f => f.Presets.Contains(p.FeatureSet)).Select(f => f.Code));
            foreach (var prm in PresetSchema.Params.Where(x => !x.IsNumber))
            {
                foreach (var o in prm.Options.Where(o => o.Value != p.C(prm.Key))) foreach (var c in o.Codes) codes.Remove(c);
                foreach (var c in prm.Options.First(o => o.Value == p.C(prm.Key)).Codes) codes.Add(c);
            }
            return Features.Where(f => codes.Contains(f.Code)).ToList();
        }

        /// <summary>Errori che impediscono l'avvio. Lista vuota = configurazione valida.</summary>
        public List<string> Validate(Preset p, DeckDef d0, DeckDef d1)
        {
            var err = new List<string>(); string mapId = p.Map; var map = Map(mapId);
            if (map == null) { err.Add($"Mappa «{mapId}» inesistente."); return err; }
            foreach (var prm in PresetSchema.Params)
            {
                if (prm.IsNumber) { int v = p.N(prm.Key); if (v < prm.Min || v > prm.Max) err.Add($"{prm.Label}: {v} fuori dall'intervallo {prm.Min}–{prm.Max}."); continue; }
                var opt = prm.Options.FirstOrDefault(o => o.Value == p.C(prm.Key));
                if (opt == null) { err.Add($"{prm.Label}: valore sconosciuto «{p.C(prm.Key)}»."); continue; }
                var why = PresetSchema.Unavailable(prm.Key, opt.Value, map);
                if (why != null) err.Add($"{prm.Label} = {opt.Value}: {why}.");
            }
            if (p.N("handStart") > p.N("deckMin")) err.Add("La mano iniziale non può superare il minimo del mazzo.");
            if (err.Count == 0) err.AddRange(MapOps.Validate(map, p.C("start")));
            if (err.Count == 0 && p.C("monsters") == "attivi")
            {
                var b = new Board(map); int cells = b.All().Count(c => b.Code(c) == "M"), deck = Game.MonsterKinds.Sum(k => p.N("monsterN_" + k));
                if (deck < cells) err.Add($"Il mazzo dei mostri ha {deck} carte ma la mappa ha {cells} caselle mostro.");
            }
            foreach (var f in FeaturesFor(p))
            {
                if (f.Choice == "X" || f.Choice == "") err.Add($"La feature {f.Code} è {(f.Choice == "X" ? "esclusa (X)" : "non decisa")} e non può stare in un preset.");
                if (f.Implementation == null || f.Implementation.StartsWith("non implementat")) err.Add($"La feature {f.Code} è nel preset ma non è implementata.");
            }
            foreach (var d in new[] { d0, d1 }) if (d == null) err.Add("Scegli un mazzo per ogni giocatore."); else err.AddRange(ValidateDeck(d, p).Select(e => $"{d.Name}: {e}"));
            return err;
        }

        public Game NewGame(Preset p, DeckDef d0, DeckDef d1, int seed)
        {
            var err = Validate(p, d0, d1); string mapId = p.Map;
            if (err.Count > 0) throw new InvalidOperationException("Configurazione non valida:\n" + string.Join("\n", err));
            var ids = d0.Expand().Concat(d1.Expand()).Concat(new[] { d0.Guildmaster, d1.Guildmaster }).Distinct();
            // la partita conserva una copia della configurazione e delle versioni delle carte
            return Game.Start(Json.Clone(p), Json.Clone(Map(mapId)), ids.Select(id => Json.Clone(Card(id, p))), Json.Clone(d0), Json.Clone(d1), seed);
        }

        /// <summary>Elimina un preset dell'utente (file compreso). Un preset di base viene solo nascosto dall'elenco: si ripristina con RestorePresets.</summary>
        public void DeletePreset(Preset p)
        {
            if (p.BuiltIn)
            {
                Hidden.Add(p.Id); Directory.CreateDirectory(Path.GetDirectoryName(HiddenFile));
                File.WriteAllLines(HiddenFile, Hidden.OrderBy(x => x)); return;
            }
            File.Delete(Path.Combine(Root, "presets", "user", p.Id + ".json"));
            Presets.RemoveAll(x => x.Id == p.Id);
        }

        /// <summary>Mazzo costruito per il preset: tutte le carte compatibili della gilda (alleati compresi, se il preset li ha), in parti uguali,
        /// fino al minimo del mazzo. Le risorse sono il 36% del mazzo (9 su 25, come nei mazzi di prova) e non meno di 9 ogni 16 altre carte.</summary>
        public DeckDef OptimizedDeck(string gmId, Preset p)
        {
            var gm = AnyVersion(gmId); int target = p.N("deckMin"), max = p.N("maxCopies");
            var pool = CardVersions.Keys.Select(id => Card(id, p)).Where(c => c != null && c.Guild == gm.Guild).ToList();
            var res = pool.FirstOrDefault(c => c.Type == CardType.Resource);
            var cards = pool.Where(c => c.Type != CardType.Resource && c.Type != CardType.Guildmaster).ToList();
            int other = Math.Min(target - (int)Math.Round(target * 0.36), cards.Count * max);
            int nRes = res == null ? 0 : Math.Max((int)Math.Round(other * 9 / 16.0), target - other);
            var d = new DeckDef { Id = "ottimizzato_" + gm.Guild.ToString().ToLowerInvariant(), Guildmaster = gmId,
                Name = $"Mazzo ottimizzato, gilda {Names.Guild(gm.Guild)} ({(cards.Any(c => c.Type == CardType.Ally) ? "con alleati" : "senza alleati")})" };
            if (nRes > 0) d.Cards.Add(new DeckEntry { Card = res.Id, Count = nRes });
            for (int i = 0; i < cards.Count; i++)
            {
                int n = other / cards.Count + (i < other % cards.Count ? 1 : 0);
                if (n > 0) d.Cards.Add(new DeckEntry { Card = cards[i].Id, Count = n });
            }
            return d;
        }

        /// <summary>Salva un mazzo dell'utente in decks/user. I mazzi di base non si toccano: l'editor lavora sempre su una copia con un Id nuovo.</summary>
        public void SaveDeck(DeckDef d)
        {
            var dir = Path.Combine(Root, "decks", "user"); Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, d.Id + ".json"), Json.Write(d), Encoding.UTF8);
            Decks.RemoveAll(x => x.Id == d.Id); Decks.Add(d);
        }

        public void SavePreset(Preset p)
        {
            if (PresetById(p.Id)?.BuiltIn == true) throw new InvalidOperationException("I preset di base non si sovrascrivono: clonalo.");
            p.BuiltIn = false;
            var dir = Path.Combine(Root, "presets", "user"); Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, p.Id + ".json"), Json.Write(p), Encoding.UTF8);
            Presets.RemoveAll(x => x.Id == p.Id); Presets.Add(p);
        }

        public string NextMapVariantId(MapDef src)
        {
            string b = src.BaseMap ?? src.Id;
            for (int i = 2; ; i++) if (Map($"{b}_v{i}") == null) return $"{b}_v{i}";
        }

        public List<string> SaveMapVariant(MapDef variant, string startMode)
        {
            var err = MapOps.Validate(variant, startMode);
            if (Map(variant.Id) != null) err.Add($"Esiste già una mappa «{variant.Id}»: le mappe non si sovrascrivono.");
            if (variant.ChangedCells.Count == 0) err.Add("Nessuna casella cambiata.");
            if (err.Count > 0) return err;
            File.WriteAllText(Path.Combine(Root, "maps", variant.Id + ".json"), Json.Write(variant), Encoding.UTF8);
            Maps.Add(variant);
            return err;
        }
    }

    public static class Report
    {
        public static string Markdown(Game g, DateTime start, DateTime end, string testerNotes)
        {
            var sb = new StringBuilder();
            string who = g.Winner >= 0 ? $"G{g.Winner + 1} — {g.WinReason}" : g.Winner == -2 ? "nessuno — " + g.WinReason : "partita non conclusa";
            sb.AppendLine("# Resoconto partita — Guilds prototipo").AppendLine();
            if (g.DebugTouched) sb.AppendLine("> **NON VALIDA PER IL CONFRONTO**: lo stato è stato modificato in modalità debug.").AppendLine();
            sb.AppendLine($"- Versione prototipo: {Repo.PrototypeVersion}");
            sb.AppendLine($"- Preset: {g.Preset.Id} — {g.Preset.Name}" + (g.Preset.BasedOn != null ? $" (clone di {g.Preset.BasedOn})" : "") + (g.Preset.Partial ? " — TEST PARZIALE" : ""));
            sb.AppendLine($"- Mappa: {g.Map.Id}" + (g.Map.BaseMap != null ? $" (variante di {g.Map.BaseMap}; caselle cambiate: {string.Join("; ", g.Map.ChangedCells)})" : " (originale)"));
            sb.AppendLine($"- Simbolo gilda gialla: {g.Preset.C("yellowSymbol")}");
            sb.AppendLine($"- Token delle altre unità: {g.Preset.C("tokenMode")}");
            sb.AppendLine($"- Tema visivo: {g.Preset.Theme}");
            sb.AppendLine($"- Seed: {g.Seed}");
            sb.AppendLine($"- Inizio: {start:yyyy-MM-dd HH:mm:ss} — durata: {(end - start).TotalMinutes:0.0} minuti");
            sb.AppendLine($"- Turni: {g.Turn} — primo giocatore: G{g.FirstPlayer + 1}");
            sb.AppendLine($"- Vincitore: {who}");
            sb.AppendLine($"- Interventi di debug: {g.Log.Count(l => l.Debug)}").AppendLine();
            sb.AppendLine("## Parametri del preset").AppendLine();
            foreach (var p in PresetSchema.Params) sb.AppendLine($"- {p.Label}: {(p.IsNumber ? g.Preset.N(p.Key).ToString() : g.Preset.C(p.Key))}");
            sb.AppendLine().AppendLine("## Mazzi e carte (pescate / giocate)").AppendLine();
            for (int i = 0; i < 2; i++)
            {
                var d = g.Decks[i]; var p = g.Players[i]; var gm = g.Gm(i);
                sb.AppendLine($"### G{i + 1}: {d.Name} ({d.Id} v{d.Version}) — {g.Def(gm).Name} {g.Def(gm).Key}, Salute finale {g.Health(gm)}/{g.MaxHealth(gm)}").AppendLine();
                if (g.Def(gm).Ability != null) sb.AppendLine($"Abilità del Guildmaster usata {p.Played.Count(x => x == g.Def(gm).Ability.Id)} volte.").AppendLine();
                sb.AppendLine("| Carta | Versione | Nel mazzo | Pescate | Giocate |").AppendLine("|---|---|---|---|---|");
                foreach (var e in d.Cards)
                    sb.AppendLine($"| {g.Def(e.Card).Name} | {g.Def(e.Card).Key} | {e.Count} | {p.Drawn.Count(x => x == e.Card)} | {p.Played.Count(x => x == e.Card)} |");
                sb.AppendLine();
            }
            sb.AppendLine("## Automazioni e tracciamento (da valutare per il tavolo)").AppendLine();
            foreach (var k in g.Log.Where(l => l.Auto).GroupBy(l => l.Kind)) sb.AppendLine($"- {k.Key}: {k.Count()} passaggi automatici");
            sb.AppendLine().AppendLine("## Interventi di debug").AppendLine();
            foreach (var l in g.Log.Where(l => l.Debug)) sb.AppendLine($"- T{l.Turn}: {l.Text}");
            if (!g.Log.Any(l => l.Debug)) sb.AppendLine("Nessuno.");
            sb.AppendLine().AppendLine("## Note del tester").AppendLine().AppendLine(string.IsNullOrWhiteSpace(testerNotes) ? "(nessuna)" : testerNotes);
            sb.AppendLine().AppendLine("## Eventi").AppendLine();
            foreach (var l in g.Log) sb.AppendLine($"- T{l.Turn} {(l.Auto ? "[auto] " : "")}{l.Text}");
            sb.AppendLine().AppendLine("## Configurazione completa (copia)").AppendLine().AppendLine("```json").AppendLine(Json.Write(g.Preset)).AppendLine("```");
            return sb.ToString();
        }
    }
}
