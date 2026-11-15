// Guilds.Core — definizioni (dati statici). Nessun riferimento a UnityEngine.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Guilds.Core
{
    public enum Guild { Red, Blue, Green, Yellow }
    public enum CardType { Guildmaster, Resource, Action, Instant, Item }
    public enum Phase { Setup, Main, Response, Discard, Over, Mulligan }

    public struct Cell : IEquatable<Cell>
    {
        public int X, Y; // Y = 0 è la riga 1 (in basso)
        public Cell(int x, int y) { X = x; Y = y; }
        public string Name => $"{(char)('A' + X)}{Y + 1}";
        public static Cell Parse(string s) => new Cell(char.ToUpperInvariant(s[0]) - 'A', int.Parse(s.Substring(1)) - 1);
        public bool Equals(Cell o) => X == o.X && Y == o.Y;
        public override bool Equals(object o) => o is Cell c && Equals(c);
        public override int GetHashCode() => X * 397 ^ Y;
        public override string ToString() => Name;
    }

    public class EffectDef
    {
        public string Kind;      // damage | heal | mod | root | reposition | damageCross | healOwn
        public int Amount;
        public string Stat;      // per mod: attack | health | move
        public string Duration;  // per mod: turn | targetNextTurn | permanent
    }

    public class CardDef
    {
        public string Id; public int Version = 1; public string Name;
        public Guild Guild; public CardType Type;
        public int CostColored, CostGeneric;
        public int Attack, Health; public string Range; // melee | ranged (solo Guildmaster)
        public string Target = "none";                  // none | self | own | enemy | any
        public List<EffectDef> Effects = new List<EffectDef>();
        public string OriginKind, OriginSource;         // fonte esistente | adattamento | nuova proposta
        public string Tests, Rationale;                 // cosa serve a testare, perché questi valori
        public string DeferredText;                     // testo della fonte non attivo in questo set
        public string TokenModel;                       // facoltativo: modello del token (relativo al tema)
        public CardDef Ability;                         // Guildmaster: abilità attivata, una volta per turno (N-07)
        public List<string> Keywords = new List<string>(); // es. venomous (F1-65)
        public bool IsAbility => Id != null && Id.EndsWith(":ability");
        public string Key => Id + "@v" + Version;
    }

    public class DeckEntry { public string Card; public int Count; }
    public class DeckDef
    {
        public string Id, Name, Guildmaster; public int Version = 1;
        public List<DeckEntry> Cards = new List<DeckEntry>();
        public IEnumerable<string> Expand() => Cards.SelectMany(e => Enumerable.Repeat(e.Card, e.Count));
    }

    public class MapDef
    {
        public string Id, Name, Source, BaseMap;
        public List<string> ChangedCells = new List<string>();
        public int Width, Height;
        public Dictionary<string, string> Legend = new Dictionary<string, string>();
        public List<List<string>> Cells = new List<List<string>>(); // prima riga = riga più alta
        public Dictionary<string, string> ExplicitZones = new Dictionary<string, string>();
        public List<string> ZoneWarnings = new List<string>();
    }

    public class FeatureInfo
    {
        public string Code, Section, Description, Choice, Note, Interpretation, Implementation, PresetNote;
        public List<string> DependsOn, IncompatibleWith, Presets = new List<string>();
    }

    public class Preset
    {
        public string Id, Name, BasedOn, Description;
        public string FeatureSet = "P01", Map = "mappa_8x8", Theme = "semplice";
        public bool Partial = true, BuiltIn;
        public Dictionary<string, string> Choices = new Dictionary<string, string>();
        public Dictionary<string, int> Numbers = new Dictionary<string, int>();
        public List<string> Hypotheses = new List<string>(), Limits = new List<string>(), Deferred = new List<string>();

        public string C(string key)
        {
            // un valore salvato che non esiste più (variante tolta) ricade sul default
            var d = PresetSchema.Params.First(p => p.Key == key);
            return Choices.TryGetValue(key, out var v) && d.Options.Any(o => o.Value == v) ? v : d.Options[0].Value;
        }
        public int N(string key) => Numbers.TryGetValue(key, out var v) ? v : PresetSchema.Params.First(p => p.Key == key).Default;
    }

    public class ParamOption { public string Value, Label; public string[] Codes = new string[0]; }
    public class ParamDef
    {
        public string Key, Label, Note; public bool Presentation;
        public List<ParamOption> Options;       // scelta esclusiva…
        public int Min, Max, Default;           // …oppure numero
        public bool IsNumber => Options == null;
    }

    /// <summary>Parametri realmente supportati dal motore. La schermata di configurazione è generata da qui.</summary>
    public static class PresetSchema
    {
        static ParamDef Ch(string key, string label, string note, params (string v, string l, string[] codes)[] o) =>
            new ParamDef { Key = key, Label = label, Note = note, Options = o.Select(x => new ParamOption { Value = x.v, Label = x.l, Codes = x.codes ?? new string[0] }).ToList() };
        static ParamDef Num(string key, string label, int def, int min, int max, string note = null) =>
            new ParamDef { Key = key, Label = label, Default = def, Min = min, Max = max, Note = note };

        public static readonly List<ParamDef> Params = new List<ParamDef>
        {
            Ch("start", "Posizione iniziale (conflitto C1)", null,
                ("S-B", "Adiacente alla propria Torre, scelta fra le caselle libere", new[] { "F4b-16", "F4b-17" }),
                ("S-A", "Angoli liberi opposti", new[] { "U-06" })),
            Ch("los", "Linea di vista (conflitto C2)", null,
                ("LOS-G", "Geometrica fra gli angoli + regola highground", new[] { "F1-36", "F1-37", "F1-38", "U-05" }),
                ("LOS-Z", "Solo fra zone adiacenti", new[] { "F4C-03" })),
            Ch("losLine", "Linea di tiro: come si traccia la retta", "Vale con la linea di vista geometrica (LOS-G). Per l'highground resta sempre la regola base.",
                ("angoli", "Da un angolo qualsiasi a un angolo qualsiasi; toccare solo l'angolo di un ostacolo è valido", null),
                ("angoli-netta", "Da angolo ad angolo, ma la retta che tocca l'angolo di un muro o di un'unità non amica non è valida", null),
                ("centro", "Una sola retta, dal centro della casella al centro della casella bersaglio", null)),
            Ch("venom", "Segnalini veleno", "Un'unità con almeno un segnalino subisce 1 danno all'inizio del proprio turno (F1-65).",
                ("resta", "Restano fino a fine partita", null),
                ("cala", "Calano di uno ogni volta che fanno danno (all'inizio del turno dell'unità avvelenata)", null)),
            Ch("mandatoryMove", "Movimento obbligatorio ogni turno", null,
                ("si", "Il Guildmaster deve muovere prima di finire il turno (decade se non può)", new[] { "F3-03" }),
                ("no", "Muovere è facoltativo", null)),
            Ch("stayMove", "Restare fermi vale come mossa?", "Muoversi nella casella in cui si è già: l'unità si tappa, l'obbligo di movimento è soddisfatto e parte il combattimento automatico.",
                ("no", "No: bisogna cambiare casella", null),
                ("si", "Sì: restare nella propria casella è una mossa legale", null)),
            Ch("attackNoMove", "Attacco senza movimento", "L'unità si tappa: il tap è il movimento, anche se la pedina non cambia casella. Ordine: tap, movimento, attacco. Vale come movimento del turno.",
                ("no", "No: si combatte solo dopo un movimento", null),
                ("si", "Sì: se c'è un bersaglio valido (adiacente o in linea di tiro) si può attaccare restando fermi", null)),
            Ch("obligation", "Se obbligatorio, con più unità", null,
                ("OBB-A", "Almeno un'unità deve muovere", null),
                ("OBB-B", "Ogni unità deve muovere", null)),
            Ch("resourceEnters", "Carta risorsa appena giocata", "Le carte risorsa si tappano per pagare e si stappano all'inizio del proprio turno.",
                ("pronta", "Entra stappata: produce fin da subito", null),
                ("tappata", "Entra tappata: si usa dal proprio turno successivo", null)),
            Ch("gmAbilities", "Abilità proprie dei Guildmaster", null,
                ("si", "Attive: una volta per turno, anche in risposta", new[] { "N-07" }),
                ("turno", "Attive: una volta per turno, solo nel proprio turno (non in risposta)", new[] { "N-07" }),
                ("no", "Spente", null)),
            Ch("dodge", "Come ci si sposta in risposta", "Il movimento richiede sempre il tap dell'unità, che quindi deve essere stappata. Le carte che spostano un'unità seguono il proprio testo.",
                ("SCH-SCARTO", "Tappando l'unità E scartando una carta dalla mano", new[] { "F2-19", "F3-11", "N-08" }),
                ("SCH-TAP", "Tappando l'unità, senza scartare", new[] { "F2-19", "F3-11", "F4B-06" })),
            Ch("dodgeOnlyVsAttack", "Quando ci si può spostare in risposta", "Riguarda lo spostamento con scarto o con tap, non le carte che spostano.",
                ("si", "Solo in risposta a un attacco contro quell'unità", new[] { "N-08" }),
                ("no", "In qualsiasi finestra di risposta", null)),
            Ch("recheck", "Combattimento automatico se un'unità cambia casella in risposta", "Vale per ogni spostamento: con scarto, con tap o con una carta, dell'attaccante o del difensore.",
                ("RIC-ANNULLA", "Niente combattimento: l'attacco colpisce il vuoto", null),
                ("RIC-GEO", "Si ricontrolla contatto / linea di vista: se c'è ancora, si combatte", null)),
            Ch("deckOut", "Mazzo esaurito", null,
                ("perde", "Chi deve pescare da un mazzo vuoto perde (H9)", new[] { "U-07", "F4A-19" }),
                ("salta", "Si salta la pesca e si continua", new[] { "F1-55" })),
            Ch("mulligan", "Mulligan iniziale", "Chiesto il 14 nov; procedura presa da BL-48.",
                ("iniziale", "Una volta, prima di iniziare: metti in fondo al mazzo le carte scelte e ne peschi altrettante meno una", new[] { "N-06" }),
                ("no", "Nessun Mulligan", null)),
            Ch("adjacency", "Contatto per il corpo a corpo (ipotesi H11)", "Nessuna fonte lo definisce.",
                ("8", "Anche in diagonale", null),
                ("4", "Solo ortogonale", null)),
            Ch("tokenMode", "Token delle altre unità", null,
                ("generico", "Segnalino generico", null), ("ruolo", "Per ruolo degli scacchi", null),
                ("gilda", "Per gilda", null), ("carta", "Modello specifico della carta", null)),
            new ParamDef { Key = "yellowSymbol", Label = "Simbolo gilda gialla (presentazione)", Presentation = true, Options = new List<ParamOption> {
                new ParamOption { Value = "stivale", Label = "Stivale alato" }, new ParamOption { Value = "pugnale", Label = "Pugnale" } } },
            Ch("towerHealEnter", "Torre propria: cura chi entra", "Entrare in una casella adiacente alla propria Torre, finché è attiva, cura il Guildmaster. Conta la casella d'arrivo, non quelle attraversate.",
                ("no", "No", null), ("si", "Sì: cura di X all'ingresso", null)),
            Num("towerHealEnterX", "X = cura all'ingresso", 1, 1, 10),
            Ch("towerHealStart", "Torre propria: cura a inizio turno", "Se il Guildmaster inizia il turno in una casella adiacente alla propria Torre attiva.",
                ("no", "No", null), ("si", "Sì: cura di X a inizio turno", null)),
            Num("towerHealStartX", "X = cura a inizio turno", 1, 1, 10),
            Ch("towerDmgEnter", "Torre nemica: danno a chi entra", "Entrare in una casella adiacente alla Torre nemica, finché è attiva, fa scattare il danno.",
                ("no", "No", null), ("si", "Sì: X danni all'ingresso", null)),
            Num("towerDmgEnterX", "X = danno all'ingresso", 1, 1, 10),
            Ch("towerDmgStart", "Torre nemica: danno a inizio turno", "Se il Guildmaster inizia il turno in una casella adiacente alla Torre nemica attiva.",
                ("no", "No", null), ("si", "Sì: X danni a inizio turno", null)),
            Num("towerDmgStartX", "X = danno a inizio turno", 1, 1, 10),
            Ch("towerHp", "Torre con punti vita", "Ipotesi H26: la Torre si colpisce col combattimento automatico, se non c'è un'unità nemica da attaccare ed è a contatto (mischia) o in linea di tiro (distanza). A 0 non è più attiva: non cura e non fa danno.",
                ("no", "No: la Torre è sempre attiva e non si può colpire", null), ("si", "Sì: la Torre ha X punti vita", null)),
            Num("towerHpMax", "X = punti vita della Torre", 5, 1, 30),
            Num("handStart", "Mano iniziale", 5, 1, 10), Num("handLimit", "Limite di mano a fine turno", 7, 1, 15),
            Num("drawPerTurn", "Carte pescate a turno", 1, 0, 3), Num("skipFirstDraw", "Il primo giocatore salta la prima pesca (1 = sì)", 1, 0, 1),
            Num("resourcesPerTurn", "Carte risorsa giocabili a turno (H1)", 1, 1, 3),
            Num("moveMax", "Movimento massimo base (minimo sempre 1, F3-04)", 3, 1, 6),
            Num("deckMin", "Carte minime nel mazzo", 25, 5, 60), Num("maxCopies", "Copie massime per carta (risorse escluse, H14)", 4, 1, 9),
        };

        /// <summary>Motivo per cui un'opzione non è selezionabile, oppure null.</summary>
        public static string Unavailable(string key, string value, MapDef map)
        {
            if (key == "los" && value == "LOS-Z") return "non implementata: la mappa non definisce i territori (foglio Zone: «da definire»)";
            if (key == "obligation" && value == "OBB-B") return "non implementata: con un solo eroe coincide con OBB-A; si distinguerà nel preset degli alleati";
            if (key == "tokenMode" && value != "generico") return "non applicabile: in questo preset non esistono unità diverse dai Guildmaster";
            if (key == "start" && value == "S-A" && map != null && new Board(map).StartCells(0, "S-A").Count == 0)
                return "su questa mappa non esiste una coppia di angoli opposti liberi (domanda D11)";
            return null;
        }

        /// <summary>Capacità del motore per la famiglia di preset: una carta è compatibile solo se tutte le sue richieste sono qui.</summary>
        public static HashSet<string> Capabilities(Preset p) => new HashSet<string> {
            "type.Guildmaster", "type.Resource", "type.Action", "type.Instant", "type.Item",
            "effect.damage", "effect.heal", "effect.mod", "effect.root", "effect.reposition", "effect.damageCross", "effect.healOwn", "ability.activated", "keyword.venomous",
            "stat.attack", "stat.health", "stat.move", "duration.turn", "duration.targetNextTurn", "duration.permanent",
            "target.none", "target.self", "target.own", "target.enemy", "target.any", "cost.resources", "range.melee", "range.ranged" };
    }

    /// <summary>Richieste e testo di una carta, ricavati dai dati eseguibili: il testo mostrato non può divergere dalla logica.</summary>
    public static class CardRules
    {
        public static List<string> Requirements(CardDef c)
        {
            var r = new List<string> { "type." + c.Type, "target." + c.Target };
            if (c.CostColored + c.CostGeneric > 0) r.Add("cost.resources");
            if (c.Type == CardType.Guildmaster) r.Add("range." + c.Range);
            foreach (var e in c.Effects)
            {
                r.Add("effect." + e.Kind);
                if (e.Kind == "mod") { r.Add("stat." + e.Stat); r.Add("duration." + e.Duration); }
            }
            foreach (var k in c.Keywords) r.Add("keyword." + k);
            if (c.Ability != null) { r.Add("ability.activated"); r.AddRange(Requirements(c.Ability).Where(x => x.StartsWith("effect.") || x.StartsWith("stat.") || x.StartsWith("duration."))); }
            return r.Distinct().ToList();
        }

        public static List<string> Missing(CardDef c, Preset p) { var caps = PresetSchema.Capabilities(p); return Requirements(c).Where(x => !caps.Contains(x)).ToList(); }

        static string StatName(string s) => s == "attack" ? "Attacco" : s == "health" ? "Salute" : "Movimento";
        static string TargetText(string t) => t == "self" ? "il tuo Guildmaster" : t == "own" ? "una tua unità in linea di vista"
            : t == "enemy" ? "un'unità nemica in linea di vista" : t == "any" ? "un'unità in linea di vista" : "";

        public static string Text(CardDef c)
        {
            switch (c.Type)
            {
                case CardType.Guildmaster:
                    return (c.Range == "ranged" ? "A distanza" : "Corpo a corpo") + $". Attacco {c.Attack}, Salute {c.Health}."
                        + (c.Keywords.Contains("venomous") ? " Venomous: quando infligge danni a un'unità le mette un segnalino veleno; un'unità avvelenata subisce 1 danno all'inizio del turno del suo proprietario." : "")
                        + (c.Ability != null ? $"\nAbilità, una volta per turno, anche in risposta (costo: 1 risorsa {Names.Guild(c.Guild)}): " + Effects(c.Ability) : "");
                case CardType.Resource: return $"Resta in gioco. All'inizio del tuo turno produce 1 risorsa {Names.Guild(c.Guild)}.";
            }
            return (c.Type == CardType.Instant && !c.IsAbility ? "Istantanea. " : c.Type == CardType.Item ? "Oggetto: assegnalo al tuo Guildmaster. " : "") + Effects(c);
        }

        static string Effects(CardDef c)
        {
            var parts = new List<string>();
            string t = c.Type == CardType.Item ? "Il Guildmaster" : Cap(TargetText(c.Target));
            foreach (var e in c.Effects)
                switch (e.Kind)
                {
                    case "damage": parts.Add($"Infligge {e.Amount} danni a {TargetText(c.Target)}."); break;
                    case "heal": parts.Add($"Cura {e.Amount} a {TargetText(c.Target)} (mai oltre la Salute massima)."); break;
                    case "root": parts.Add($"{t} non può muoversi fino alla fine del prossimo turno del suo proprietario."); break;
                    case "reposition": parts.Add($"Sposta {TargetText(c.Target)} fino a {e.Amount} caselle."); break;
                    case "damageCross": parts.Add($"Scegli una casella in linea di vista: infligge {e.Amount} danni a tutte le unità in una Croce (quella casella e le 4 adiacenti in orizzontale e verticale)."); break;
                    case "healOwn": parts.Add($"Cura {e.Amount} a una tua unità in linea di vista (qui: il tuo Guildmaster)."); break;
                    case "mod":
                        string d = e.Duration == "turn" ? " fino a fine turno" : e.Duration == "targetNextTurn" ? " fino alla fine del prossimo turno del suo proprietario" : "";
                        parts.Add($"{t} ottiene {e.Amount:+#;-#} {StatName(e.Stat)}{d}."); break;
                }
            return string.Join(" ", parts);
        }
        static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        public static string Cost(CardDef c, Preset p) =>
            c.Type == CardType.Guildmaster || c.Type == CardType.Resource ? "—"
            : (c.CostGeneric > 0 ? c.CostGeneric.ToString() : "") + string.Concat(Enumerable.Repeat(Names.Symbol(c.Guild, p), c.CostColored)) is var s && s != "" ? s : "0";
    }

    public static class Names
    {
        public static string Guild(Guild g) => g == Core.Guild.Red ? "rossa" : g == Core.Guild.Blue ? "blu" : g == Core.Guild.Green ? "verde" : "gialla";
        /// <summary>Sigla testuale del simbolo; l'immagine sta in data/art/symbols/&lt;SymbolId&gt;.png.</summary>
        public static string Symbol(Guild g, Preset p) => g == Core.Guild.Red ? "[Bomba]" : g == Core.Guild.Blue ? "[Fiamma]" : g == Core.Guild.Green ? "[Arco]"
            : (p != null && p.C("yellowSymbol") == "pugnale" ? "[Pugnale]" : "[Stivale]");
        public static string SymbolId(Guild g, Preset p) => g == Core.Guild.Red ? "red_bomb" : g == Core.Guild.Blue ? "blue_flame" : g == Core.Guild.Green ? "green_bow"
            : (p != null && p.C("yellowSymbol") == "pugnale" ? "yellow_dagger" : "yellow_boot");
        public static string Type(CardType t) => t == CardType.Guildmaster ? "Guildmaster" : t == CardType.Resource ? "Risorsa" : t == CardType.Action ? "Azione"
            : t == CardType.Instant ? "Azione istantanea" : "Oggetto";
    }
}
