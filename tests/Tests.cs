using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Guilds.Core;
using Xunit;

public class Tests
{
    static readonly string Root = FindRoot();
    static readonly Repo R = Repo.Load(Root);

    static string FindRoot()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (!Directory.Exists(Path.Combine(d.FullName, "data", "features"))) d = d.Parent;
        return Path.Combine(d.FullName, "data");
    }

    static Cell C(string s) => Cell.Parse(s);

    /// <summary>Termina il turno e chiude la finestra di fine turno senza risposte.</summary>
    static string End(Game g, int player) { var w = g.EndTurn(player); if (w == null) g.Pass(1 - player); return w; }

    /// <summary>Partita pronta al turno 1, col giocatore «first» di turno; i Guildmaster vengono messi dove serve.</summary>
    static Game New(string d0 = "deck_red", string d1 = "deck_yellow", int first = 0, string preset = "P01", Action<Preset> tweak = null, string pos0 = null, string pos1 = null)
    {
        var p = Json.Clone(R.PresetById(preset)); tweak?.Invoke(p);
        for (int seed = 1; ; seed++)
        {
            var g = R.NewGame(p, R.Deck(d0), R.Deck(d1), seed);
            if (g.FirstPlayer != first) continue;
            while (g.Phase == Phase.Mulligan) Assert.Null(g.Mulligan(g.Actor, new List<int>()));
            while (g.Phase == Phase.Setup) Assert.Null(g.ChooseStart(g.Actor, g.StartOptions(g.Actor)[0]));
            if (pos0 != null) g.Gm(0).Pos = C(pos0);
            if (pos1 != null) g.Gm(1).Pos = C(pos1);
            return g;
        }
    }

    static Board Mini(params string[] rowsTopFirst) => new Board(new MapDef
    {
        Id = "mini", Width = rowsTopFirst[0].Length, Height = rowsTopFirst.Length,
        Cells = rowsTopFirst.Select(r => r.Select(ch => ch == '_' ? "" : ch.ToString()).ToList()).ToList()
    });
    static bool Free(Cell c) => false;

    // ---------- mappe ----------
    [Fact]
    public void Mappe_convertite_dall_Excel()
    {
        var b = new Board(R.Map("mappa_8x8"));
        Assert.Equal((8, 8), (b.W, b.H));
        Assert.Equal(C("A1"), b.Tower(0)); Assert.Equal(C("H8"), b.Tower(1));
        Assert.Equal("M", b.Code(C("B7"))); Assert.Equal("W", b.Code(C("D4"))); Assert.Equal("H", b.Code(C("E1")));
        Assert.Equal(3, b.StartCells(0, "S-B").Count); Assert.Equal(3, b.StartCells(1, "S-B").Count);
        var n = new Board(R.Map("mappa_9x7"));
        Assert.Equal((9, 9), (n.W, n.H));
        Assert.False(n.InMap(C("C1"))); Assert.Equal("T2", n.Code(C("I9")));
        Assert.Equal(3, n.StartCells(0, "S-B").Count);
    }

    [Fact]
    public void Ritocco_mappa_solo_come_variante_valida()
    {
        var src = R.Map("mappa_8x8");
        var bad = MapOps.MakeVariant(src, new Dictionary<Cell, string> { [C("H8")] = "." }, "x");
        Assert.Contains(MapOps.Validate(bad), e => e.Contains("Torre"));
        var closed = MapOps.MakeVariant(src, new Dictionary<Cell, string> { [C("A2")] = "W", [C("B1")] = "W", [C("B2")] = "W" }, "x");
        Assert.Contains(MapOps.Validate(closed), e => e.Contains("partenza"));
        Assert.Throws<ArgumentException>(() => MapOps.MakeVariant(R.Map("mappa_9x7"), new Dictionary<Cell, string> { [C("C1")] = "." }, "x"));

        var tmp = Path.Combine(Path.GetTempPath(), "guilds_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(tmp, "maps"));
        var repo = Repo.Load(Root); repo.Root = tmp;
        var ok = MapOps.MakeVariant(src, new Dictionary<Cell, string> { [C("C3")] = "W" }, repo.NextMapVariantId(src));
        Assert.Equal("mappa_8x8_v2", ok.Id); Assert.Equal("mappa_8x8", ok.BaseMap); Assert.Single(ok.ChangedCells);
        Assert.Empty(repo.SaveMapVariant(ok, "S-B"));
        Assert.Contains(repo.SaveMapVariant(ok, "S-B"), e => e.Contains("non si sovrascrivono"));
        Assert.Equal(".", new Board(src).Code(C("C3"))); // l'originale non cambia
        Directory.Delete(tmp, true);
    }

    // ---------- movimento ----------
    [Fact]
    public void Movimento_ostacoli_e_occupazione()
    {
        var g = New(pos0: "B2", pos1: "C3");
        var m = g.MovesFor(0);
        Assert.All(m.Values, p => Assert.InRange(p.Count, 1, 3));
        Assert.DoesNotContain(C("A1"), m.Keys);  // Torre
        Assert.DoesNotContain(C("D4"), m.Keys);  // muro
        Assert.DoesNotContain(C("C3"), m.Keys);  // casella occupata
        Assert.Contains(C("B5"), m.Keys);        // 3 caselle
        Assert.DoesNotContain(C("B6"), m.Keys);  // 4 caselle
        Assert.Contains(C("D3"), m.Keys);        // highground attraversabile (U-04)

        var corridor = Mini("....");
        var blocked = corridor.Reach(C("A1"), 3, c => !c.Equals(C("B1")), c => true);
        Assert.Empty(blocked); // un nemico in B1 non si attraversa (F1-25)
    }

    [Fact]
    public void Movimento_tappa_e_non_si_ripete()
    {
        var g = New(pos0: "B2", pos1: "G7");
        Assert.NotNull(g.Move(0, 0, C("B6")));
        Assert.Null(g.Move(0, 0, C("B3")));
        Assert.True(g.Gm(0).Tapped);
        Assert.Null(g.Pass(1));
        Assert.Contains("tappato", g.WhyNotMove(0, 0));
        Assert.Contains("Puoi muovere solo", g.WhyNotMove(0, 1));
    }

    // ---------- linea di vista ----------
    [Fact]
    public void Linea_di_vista_geometrica()
    {
        Assert.False(Mini(".W.").Los(C("A1"), C("C1"), Free));               // muro in mezzo
        Assert.True(Mini("...").Los(C("A1"), C("C1"), Free));
        Assert.False(Mini("...").Los(C("A1"), C("C1"), c => c.Equals(C("B1")))); // unità nemica in mezzo (F1-37)
        Assert.True(Mini("...", ".W.", "...").Los(C("A1"), C("C3"), Free));   // tocca solo l'angolo del muro: valida
        Assert.False(Mini("...", "WWW", "...").Los(C("A1"), C("C3"), Free));
        Assert.True(Mini(".W", "..").Los(C("A1"), C("B1"), Free));
        // varianti della retta
        Assert.False(Mini("...", ".W.", "...").Los(C("A1"), C("C3"), Free, "angoli-netta")); // toccare l'angolo del muro non vale
        Assert.True(Mini(".W.", "...").Los(C("A1"), C("C2"), Free, "angoli-netta"));          // una retta libera esiste
        Assert.True(Mini("...", "...", "...").Los(C("A1"), C("C3"), Free, "angoli-netta"));
        Assert.False(Mini("...", "...", "...").Los(C("A1"), C("C3"), c => c.Equals(C("B2")), "angoli-netta")); // angolo di un'unità non amica
        Assert.False(Mini(".W.", "...").Los(C("A1"), C("C2"), Free, "centro"));               // dal centro la retta entra nel muro
        Assert.True(Mini(".W.", "...").Los(C("A1"), C("C2"), Free));                          // dagli angoli no
        Assert.True(Mini("...").Los(C("A1"), C("C1"), Free, "centro"));
    }

    [Fact]
    public void Linea_di_vista_e_highground()
    {
        Assert.False(Mini(".H.").Los(C("A1"), C("C1"), Free)); // non attraversa
        Assert.True(Mini(".H.").Los(C("A1"), C("B1"), Free));  // entra
        Assert.True(Mini(".HH.").Los(C("A1"), C("C1"), Free)); // entra e resta dentro
        Assert.True(Mini("HHH").Los(C("A1"), C("C1"), Free));  // resta dentro
        Assert.True(Mini(".HH.").Los(C("B1"), C("D1"), Free)); // esce
        Assert.False(Mini("H.H.H").Los(C("A1"), C("E1"), Free)); // attraversa un altro highground
    }

    // ---------- costi e risorse ----------
    [Fact]
    public void Costi_e_carte_risorsa()
    {
        var g = New(pos0: "B2", pos1: "G7"); var p = g.Players[0];
        p.Hand = new List<string> { "res_red", "res_red", "r_fury" };
        Assert.Contains("Risorse insufficienti", g.WhyNotPlay(0, 2));
        Assert.Null(g.Play(0, 0));
        Assert.Contains("H1", g.WhyNotPlay(0, 0));
        Assert.Equal(1, g.Available(0, Guild.Red)); // entra stappata: produce subito
        p.Pool[Guild.Red] = 1;
        Assert.Null(g.Play(0, 1, 0));
        Assert.Equal(0, p.Pool[Guild.Red]);
        Assert.Equal(Phase.Response, g.Phase);
        Assert.Null(g.Pass(1));
        Assert.Equal(4, g.Attack(g.Gm(0)));
        g.Move(0, 0, C("B3")); g.Pass(1); Assert.Null(End(g, 0));
        Assert.Equal(2, g.Attack(g.Gm(0)));     // il bonus scade a fine turno
        g.Move(1, 1, C("G6")); g.Pass(0); Assert.Null(End(g, 1));
        Assert.Equal(1, g.Available(0, Guild.Red)); // la carta risorsa si è stappata
    }

    // ---------- risposte, combattimento ----------
    [Fact]
    public void Combattimento_automatico_in_mischia_con_danni_reciproci()
    {
        var g = New("deck_red", "deck_yellow", pos0: "C2", pos1: "C5");
        Assert.Null(g.Move(0, 0, C("C4")));
        Assert.Equal(Phase.Response, g.Phase); Assert.Equal(1, g.Actor);
        Assert.Equal(0, g.Gm(1).Damage); // niente danni prima che l'avversario passi
        Assert.Null(g.Pass(1));
        Assert.Equal(2, g.Gm(1).Damage); Assert.Equal(2, g.Gm(0).Damage);
        Assert.Equal(Phase.Main, g.Phase);
    }

    [Fact]
    public void Difensore_a_distanza_non_restituisce_e_il_ranged_colpisce_in_linea_di_vista()
    {
        var g = New("deck_red", "deck_green", pos0: "C2", pos1: "C5");
        g.Move(0, 0, C("C4")); g.Pass(1);
        Assert.Equal(2, g.Gm(1).Damage); Assert.Equal(0, g.Gm(0).Damage);
        Assert.Null(End(g, 0));
        g.Move(1, 1, C("C6")); g.Pass(0); // Senya si allontana: attacca in linea di vista, Fiaran non è adiacente
        Assert.Equal(2, g.Gm(0).Damage); Assert.Equal(2, g.Gm(1).Damage);
    }

    [Fact]
    public void Ranged_senza_linea_di_vista_non_attacca()
    {
        var g = New("deck_green", "deck_red", pos0: "C4", pos1: "F5");
        g.Move(0, 0, C("C5")); g.Pass(1); // i muri D5-E5 coprono
        Assert.Equal(0, g.Gm(1).Damage);
        Assert.Contains(g.Log, l => l.Text.Contains("Nessun combattimento"));
    }

    [Fact]
    public void Schivata_col_tap_e_varianti_di_ricontrollo()
    {
        var g = New("deck_red", "deck_yellow", tweak: x => x.Choices["dodge"] = "SCH-TAP", pos0: "C2", pos1: "C5");
        g.Move(0, 0, C("C4"));
        Assert.Null(g.Move(1, 1, C("C7")));           // SCH-TAP: Dalia è stappata al turno 1 dell'avversario
        Assert.True(g.Gm(1).Tapped);
        Assert.Null(g.Pass(0));
        Assert.Equal(0, g.Gm(1).Damage);              // RIC-GEO: non più a contatto

        g = New("deck_red", "deck_yellow", preset: "P01d", tweak: x => x.Choices["dodge"] = "SCH-TAP", pos0: "C2", pos1: "C5");
        g.Move(0, 0, C("C4")); g.Move(1, 1, C("B5")); g.Pass(0);
        Assert.Equal(2, g.Gm(1).Damage);              // RIC-GEO: ancora a contatto, si combatte

        g = New("deck_red", "deck_yellow", tweak: x => x.Choices["dodge"] = "SCH-TAP", pos0: "C2", pos1: "C5");
        g.Move(0, 0, C("C4")); g.Move(1, 1, C("B5")); g.Pass(0);
        Assert.Equal(0, g.Gm(1).Damage);              // RIC-ANNULLA

        // una carta che sposta segue il proprio testo: funziona anche con l'unità tappata
        g = New("deck_red", "deck_yellow", pos0: "C2", pos1: "C5");
        g.Move(0, 0, C("C4")); g.Gm(1).Tapped = true;
        g.Players[1].Hand = new List<string> { "y_smoke_step" }; g.Players[1].Pool[Guild.Yellow] = 1;
        Assert.Null(g.Play(1, 0, 1, C("C7")));        // schivata con carta
        Assert.Null(g.Pass(0));                       // si risolve Smoke Step
        Assert.Equal(C("C7"), g.Gm(1).Pos);
        Assert.Null(g.Pass(1));                       // poi il movimento: nessun contatto
        Assert.Equal(0, g.Gm(1).Damage);
    }

    [Fact]
    public void Il_bersaglio_che_cambia_casella_evita_l_effetto()
    {
        var g = New("deck_red", "deck_yellow", tweak: x => x.Choices["dodge"] = "SCH-TAP", pos0: "C2", pos1: "C5");
        g.Players[0].Hand = new List<string> { "r_thermal_surge" }; g.Players[0].Pool[Guild.Red] = 2;
        Assert.Equal(new[] { 0, 1 }, g.TargetsFor(0, g.Def("r_thermal_surge")));
        Assert.Null(g.Play(0, 0, 1));
        Assert.Null(g.Move(1, 1, C("C6")));
        Assert.Null(g.Pass(0));
        Assert.Equal(0, g.Gm(1).Damage);
        Assert.Contains(g.Log, l => l.Text.Contains("F3-11"));
        Assert.Contains("r_thermal_surge", g.Players[0].Discard);
    }

    [Fact]
    public void Bersagli_solo_in_linea_di_vista_e_istantanee_in_risposta()
    {
        var g = New("deck_blue", "deck_red", pos0: "C4", pos1: "F5");
        g.Players[0].Hand = new List<string> { "b_arcane_dart", "b_frost" }; g.Players[0].Pool[Guild.Blue] = 3;
        Assert.Contains("linea di vista", g.WhyNotPlay(0, 0));
        g.Gm(1).Pos = C("C6");
        Assert.Null(g.WhyNotPlay(0, 0));
        g.Move(0, 0, C("B4")); g.Pass(1); End(g, 0);
        g.Players[0].Pool[Guild.Blue] = 2; g.Move(1, 1, C("C5"));
        Assert.Contains("istantanee", g.WhyNotPlay(0, 1));
        Assert.Null(g.Play(0, 0, 1));
        Assert.Equal(1, g.Actor);       // la priorità torna a chi ha mosso
        g.Pass(1);                      // Dardo arcano
        Assert.Equal(4, g.Gm(1).Damage); // 2 dal combattimento del turno 1 + 2 dal Dardo
        Assert.Equal(0, g.Actor); g.Pass(0); // poi il movimento: Fiaran è adiacente a Nebros
        Assert.Equal(2, g.Gm(0).Damage);
    }

    [Fact]
    public void Oggetti_cure_e_modificatori()
    {
        var g = New("deck_green", "deck_red", pos0: "B2", pos1: "G7"); var p = g.Players[0];
        p.Hand = new List<string> { "g_hunters_bow", "g_hunters_bow", "g_field_dressing" }; p.Pool[Guild.Green] = 6;
        g.Play(0, 0); g.Pass(1); g.Play(0, 0); g.Pass(1);
        Assert.Equal(4, g.Attack(g.Gm(0)));  // due oggetti uguali sulla stessa unità (F1-12)
        g.Gm(0).Damage = 3;
        g.Play(0, 0, 0); g.Pass(1);
        Assert.Equal(0, g.Gm(0).Damage);     // cura mai oltre il massimo (F1-67)
    }

    // ---------- movimento obbligatorio ----------
    [Fact]
    public void Movimento_obbligatorio_e_Root()
    {
        var g = New("deck_green", "deck_red", pos0: "B2", pos1: "B5");
        Assert.Contains("F3-03", End(g, 0));
        g.Players[0].Hand = new List<string> { "g_hunters_net" }; g.Players[0].Pool[Guild.Green] = 2;
        g.Play(0, 0, 1); g.Pass(1);
        g.Move(0, 0, C("B3")); g.Pass(1);
        Assert.Null(End(g, 0));
        Assert.Contains("Root", g.WhyNotMove(1, 1));
        Assert.Null(End(g, 1));           // bloccato: l'obbligo decade
        Assert.Contains(g.Log, l => l.Text.Contains("l'obbligo di movimento decade"));
        g.Move(0, 0, C("B4")); g.Pass(1); End(g, 0);
        Assert.Null(g.WhyNotMove(1, 1));     // Root è scaduto
    }

    // ---------- morte e vittoria ----------
    [Fact]
    public void Morte_e_vittoria()
    {
        var g = New("deck_red", "deck_green", pos0: "C2", pos1: "C5");
        g.Gm(1).Damage = 18;
        g.Move(0, 0, C("C4")); g.Pass(1);
        Assert.Equal(Phase.Over, g.Phase); Assert.Equal(0, g.Winner);
        Assert.NotNull(g.Move(0, 0, C("C3")));

        g = New("deck_red", "deck_yellow", pos0: "C2", pos1: "C5");
        g.Gm(0).Damage = 18; g.Gm(1).Damage = 18;
        g.Move(0, 0, C("C4")); g.Pass(1);
        Assert.Equal(-2, g.Winner); // non definito: D21
    }

    [Fact]
    public void Mazzo_esaurito_nelle_due_varianti()
    {
        var g = New(pos0: "B2", pos1: "G7");
        g.Players[1].Deck.Clear();
        g.Move(0, 0, C("B3")); g.Pass(1); End(g, 0);
        Assert.Equal(0, g.Winner);

        g = New(pos0: "B2", pos1: "G7", tweak: p => p.Choices["deckOut"] = "salta");
        g.Players[1].Deck.Clear();
        g.Move(0, 0, C("B3")); g.Pass(1); End(g, 0);
        Assert.Equal(Phase.Main, g.Phase); Assert.Equal(1, g.Active);
    }

    [Fact]
    public void Limite_di_mano_con_scarto()
    {
        var g = New(pos0: "B2", pos1: "G7");
        g.Players[0].Hand = Enumerable.Repeat("r_fury", 9).ToList();
        g.Move(0, 0, C("B3")); g.Pass(1); End(g, 0);
        Assert.Equal(Phase.Discard, g.Phase);
        g.DiscardCard(0, 0); Assert.Equal(Phase.Discard, g.Phase);
        g.DiscardCard(0, 0); Assert.Equal(1, g.Active); Assert.Equal(7, g.Players[0].Hand.Count);
    }

    // ---------- preset, mazzi, carte ----------
    [Fact]
    public void Preset_e_mazzi_validi_e_invalidi()
    {
        foreach (var d in R.Decks) Assert.Empty(R.ValidateDeck(d, R.PresetById("P01")));
        Assert.Empty(R.Validate(R.PresetById("P01"), R.Deck("deck_red"), R.Deck("deck_blue")));
        Assert.Empty(R.Validate(R.PresetById("P01-9x7"), R.Deck("deck_green"), R.Deck("deck_yellow")));
        Assert.Contains(R.Validate(R.PresetById("P01b"), R.Deck("deck_red"), R.Deck("deck_blue")), e => e.Contains("angoli"));
        var z = Json.Clone(R.PresetById("P01")); z.Choices["los"] = "LOS-Z";
        Assert.Contains(R.Validate(z, R.Deck("deck_red"), R.Deck("deck_blue")), e => e.Contains("non implementata"));
        Assert.Throws<InvalidOperationException>(() => R.NewGame(z, R.Deck("deck_red"), R.Deck("deck_blue"), 1));
        var n = Json.Clone(R.PresetById("P01")); n.Numbers["deckMin"] = 30;
        Assert.Contains(R.Validate(n, R.Deck("deck_red"), R.Deck("deck_blue")), e => e.Contains("minimo"));
    }

    [Fact]
    public void Carta_incompatibile_col_preset_viene_rifiutata()
    {
        var repo = Repo.Load(Root); var p = repo.PresetById("P01");
        repo.CardVersions["x_summon"] = new List<CardDef> { new CardDef { Id = "x_summon", Name = "Evoca alleato", Guild = Guild.Red, Type = CardType.Action,
            Effects = new List<EffectDef> { new EffectDef { Kind = "summon" } } } };
        var d = Json.Clone(repo.Deck("deck_red")); d.Cards[1].Card = "x_summon";
        Assert.Contains(repo.ValidateDeck(d, p), e => e.Contains("effect.summon"));
        Assert.Contains(repo.DeckChanges(d, null, p), e => e.Contains("NON COMPATIBILE"));
        // una versione più vecchia ma compatibile viene selezionata al posto di quella nuova incompatibile
        repo.CardVersions["x_summon"].Add(new CardDef { Id = "x_summon", Version = 0, Name = "Evoca alleato", Guild = Guild.Red, Type = CardType.Action,
            Target = "enemy", Effects = new List<EffectDef> { new EffectDef { Kind = "damage", Amount = 1 } } });
        Assert.Equal(0, repo.Card("x_summon", p).Version);
        Assert.Empty(repo.ValidateDeck(d, p));
    }

    [Fact]
    public void Registro_feature_coerente_col_preset()
    {
        var p = R.PresetById("P01"); var fs = R.FeaturesFor(p);
        Assert.InRange(fs.Count, 75, 90);
        Assert.All(fs, f => Assert.Contains(f.Choice, new[] { "L", "T" }));
        Assert.Contains(fs, f => f.Code == "F4b-16"); Assert.DoesNotContain(fs, f => f.Code == "U-06");
        var s = Json.Clone(p); s.Choices["deckOut"] = "salta";
        Assert.Contains(R.FeaturesFor(s), f => f.Code == "F1-55"); Assert.DoesNotContain(R.FeaturesFor(s), f => f.Code == "U-07");
        Assert.Contains(R.FeaturesFor(R.PresetById("P01b")), f => f.Code == "U-06");
    }

    [Fact]
    public void Testo_delle_carte_generato_dai_dati_e_simbolo_giallo()
    {
        var p = R.PresetById("P01");
        foreach (var c in R.CardVersions.Values.SelectMany(v => v)) Assert.False(string.IsNullOrWhiteSpace(CardRules.Text(c)));
        Assert.Contains("3 danni", CardRules.Text(R.Card("r_thermal_surge", p)));
        Assert.Equal("1[Stivale]", CardRules.Cost(R.Card("y_venom_blades", p), p));
        var d = Json.Clone(p); d.Choices["yellowSymbol"] = "pugnale";
        Assert.Equal("1[Pugnale]", CardRules.Cost(R.Card("y_venom_blades", d), d));
        Assert.Equal("yellow_dagger", Names.SymbolId(Guild.Yellow, d));
        Assert.Empty(R.DeckChanges(R.Deck("deck_yellow"), p, d)); // presentazione: non cambia carte né regole
    }

    [Fact]
    public void Lo_stesso_Guildmaster_muove_a_ogni_suo_turno()
    {
        var g = New(pos0: "B2", pos1: "G7");
        foreach (var (a, b) in new[] { ("B3", "G6"), ("B2", "G7"), ("B3", "G6") })
        {
            Assert.False(g.Gm(0).Tapped);                      // stappato all'inizio del proprio turno
            Assert.Null(g.Move(0, 0, C(a))); Assert.Null(g.Pass(1)); Assert.Null(End(g, 0));
            Assert.True(g.Gm(0).Tapped);                       // resta tappato nel turno avversario: niente schivata col tap
            Assert.Null(g.Move(1, 1, C(b))); Assert.Null(g.Pass(0)); Assert.Null(End(g, 1));
        }
    }

    [Fact]
    public void Mulligan_iniziale_e_variante_senza()
    {
        var g = R.NewGame(R.PresetById("P01"), R.Deck("deck_red"), R.Deck("deck_blue"), 5);
        Assert.Equal(Phase.Mulligan, g.Phase);
        int a = g.Actor; var deckBefore = g.Players[a].Deck.Count;
        Assert.NotNull(g.Mulligan(1 - a, new List<int>()));
        Assert.Null(g.Mulligan(a, new List<int> { 0, 1, 2 }));
        Assert.Equal(4, g.Players[a].Hand.Count); Assert.Equal(deckBefore + 1, g.Players[a].Deck.Count);
        Assert.Null(g.Mulligan(1 - a, new List<int>()));
        Assert.Equal(5, g.Players[1 - a].Hand.Count);
        Assert.Equal(Phase.Setup, g.Phase);
        var p = Json.Clone(R.PresetById("P01")); p.Choices["mulligan"] = "no";
        Assert.Equal(Phase.Setup, R.NewGame(p, R.Deck("deck_red"), R.Deck("deck_blue"), 5).Phase);
        Assert.DoesNotContain(R.FeaturesFor(p), f => f.Code == "N-06");
    }

    [Fact]
    public void Risorse_che_si_tappano_e_istantanea_in_risposta_con_Guildmaster_tappato()
    {
        var g = New("deck_red", "deck_yellow", pos0: "B2", pos1: "G7", tweak: x => x.Choices["resourceEnters"] = "tappata"); var p = g.Players[0];
        p.Hand = new List<string> { "res_red", "r_fury" };
        g.Play(0, 0); Assert.True(p.Resources[0].Tapped);            // variante: entra tappata
        g.Move(0, 0, C("B3")); g.Pass(1); End(g, 0);
        g.Move(1, 1, C("G6")); g.Pass(0); End(g, 1);
        Assert.False(p.Resources[0].Tapped);                         // si stappa all'inizio del proprio turno
        g.Move(0, 0, C("B2")); g.Pass(1); End(g, 0);              // non spesa: resta pronta nel turno avversario
        Assert.True(g.Gm(0).Tapped);
        g.Move(1, 1, C("G7"));
        Assert.Contains("tappato", g.WhyNotMove(0, 0));              // tappato: niente spostamento in risposta…
        Assert.Null(g.Play(0, 0, 0));                                // …ma l'istantanea si gioca lo stesso
        Assert.True(p.Resources[0].Tapped); Assert.Equal(0, g.Available(0, Guild.Red));

        g = New("deck_red", "deck_yellow", pos0: "B2", pos1: "G7");                // default: entra stappata
        g.Players[0].Hand = new List<string> { "res_red", "r_fury" };
        g.Play(0, 0); Assert.Null(g.Play(0, 0, 0));                  // variante: la risorsa si usa subito
    }

    [Fact]
    public void Abilita_dei_Guildmaster_una_volta_per_turno()
    {
        // Nebros: sposta il nemico in risposta al suo movimento (esempio del documento fase 2)
        var g = New("deck_blue", "deck_red", first: 1, pos0: "C6", pos1: "C3");
        g.Players[0].Pool[Guild.Blue] = 2;
        g.Move(1, 1, C("C5"));
        Assert.Null(g.UseAbility(0, 1, C("C3")));
        g.Pass(1);
        Assert.Equal(C("C3"), g.Gm(1).Pos);
        Assert.Contains("una volta per turno", g.WhyNotAbility(0));
        g.Pass(0);
        Assert.Equal(0, g.Gm(0).Damage);                             // Fiaran non è più a contatto

        // Fiaran: Croce su una casella in linea di vista, colpisce tutte le unità (anche sé stesso)
        g = New("deck_red", "deck_green", pos0: "C2", pos1: "C4"); g.Players[0].Pool[Guild.Red] = 1;
        Assert.NotNull(g.UseAbility(0));
        Assert.Null(g.UseAbility(0, -1, C("C3"))); g.Pass(1);
        Assert.Equal(2, g.Gm(1).Damage); Assert.Equal(2, g.Gm(0).Damage);
        Assert.False(g.Gm(0).Tapped);                                // l'abilità non tappa

        // Senya: 1 danno al nemico e cura 1
        g = New("deck_green", "deck_red", pos0: "C2", pos1: "C5"); g.Players[0].Pool[Guild.Green] = 1; g.Gm(0).Damage = 3;
        Assert.Null(g.UseAbility(0, 1)); g.Pass(1);
        Assert.Equal(1, g.Gm(1).Damage); Assert.Equal(2, g.Gm(0).Damage);

        // Dalia: 1 danno + Venomous (F1-65): 1 danno all'inizio del turno dell'avvelenato
        g = New("deck_yellow", "deck_red", pos0: "C2", pos1: "C5"); g.Players[0].Pool[Guild.Yellow] = 1;
        Assert.Null(g.UseAbility(0, 1)); g.Pass(1);
        Assert.Equal(1, g.Gm(1).Damage); Assert.Equal(1, g.Gm(1).Venom);
        g.Move(0, 0, C("B2")); g.Pass(1); End(g, 0);
        Assert.Equal(2, g.Gm(1).Damage);
        g.Move(1, 1, C("C6")); g.Pass(0); End(g, 1);
        Assert.Equal(1, g.Gm(1).Venom);                               // default: il segnalino resta

        // variante: cala di uno nel turno in cui fa danno
        g = New("deck_yellow", "deck_red", preset: "P01l", pos0: "C2", pos1: "C5"); g.Players[0].Pool[Guild.Yellow] = 1;
        Assert.Null(g.UseAbility(0, 1)); g.Pass(1);
        g.Gm(1).Venom = 2;
        g.Move(0, 0, C("B2")); g.Pass(1); End(g, 0);
        Assert.Equal(2, g.Gm(1).Damage); Assert.Equal(1, g.Gm(1).Venom); // 1 danno, un segnalino in meno
        g.Move(1, 1, C("C6")); g.Pass(0); End(g, 1);
        Assert.Equal(1, g.Gm(1).Venom);                               // nel turno di Dalia non cala

        // variante «solo nel proprio turno»: sì nella fase principale, no in risposta (né nel proprio turno né in quello avversario)
        g = New("deck_yellow", "deck_red", preset: "P01g", pos0: "C2", pos1: "C5"); g.Players[0].Pool[Guild.Yellow] = 2; g.Players[1].Pool[Guild.Red] = 1;
        Assert.Null(g.WhyNotAbility(0));
        g.Move(0, 0, C("B2"));
        Assert.Contains("solo nel proprio turno", g.WhyNotAbility(1)); g.Pass(1);
        Assert.Null(g.UseAbility(0, 1));
        g.Pass(1);
        Assert.Contains("una volta per turno", g.WhyNotAbility(0));

        g = New("deck_blue", "deck_red", tweak: x => x.Choices["gmAbilities"] = "no"); g.Players[g.Active].Pool[Guild.Blue] = 1;
        Assert.Contains("spente", g.WhyNotAbility(g.Active));
    }

    [Fact]
    public void Movimento_obbligatorio_disattivabile()
    {
        var p = Json.Clone(R.PresetById("P01e"));
        var g = New(preset: "P01e", pos0: "B2", pos1: "G7");
        Assert.Null(End(g, 0));
        Assert.DoesNotContain(R.FeaturesFor(p), f => f.Code == "F3-03");
        Assert.Contains(R.FeaturesFor(R.PresetById("P01")), f => f.Code == "F3-03");
    }

    [Fact]
    public void Spostamento_in_risposta_scartando_una_carta_solo_contro_un_attacco()
    {
        var g = New("deck_red", "deck_yellow", pos0: "C2", pos1: "C5");
        g.Move(0, 0, C("C4"));                                        // Fiaran arriva a contatto: attacco in arrivo
        int hand = g.Players[1].Hand.Count; var first = g.Players[1].Hand[0];
        Assert.Contains("scartare", g.Move(1, 1, C("C7")));           // serve una carta
        Assert.Null(g.Move(1, 1, C("C7"), 0));
        Assert.Equal(hand - 1, g.Players[1].Hand.Count); Assert.Contains(first, g.Players[1].Discard);
        Assert.True(g.Gm(1).Tapped);                                  // il costo è il tap E la carta
        g.Pass(0); Assert.Equal(0, g.Gm(1).Damage);

        g = New("deck_red", "deck_yellow", pos0: "B2", pos1: "G7");
        g.Move(0, 0, C("B3"));                                        // nessun attacco in arrivo
        Assert.Contains("solo a un attacco", g.WhyNotMove(1, 1));
        g = New("deck_red", "deck_yellow", pos0: "B2", pos1: "G7", tweak: x => x.Choices["dodgeOnlyVsAttack"] = "no");
        g.Move(0, 0, C("B3")); Assert.Null(g.WhyNotMove(1, 1));

        // attacco con una carta: vale come attacco; un'unità tappata non si sposta mai
        g = New("deck_red", "deck_yellow", pos0: "C2", pos1: "C5");
        g.Players[0].Hand = new List<string> { "r_thermal_surge" }; g.Players[0].Pool[Guild.Red] = 2; g.Gm(1).Tapped = true;
        g.Play(0, 0, 1);
        Assert.Contains("tappata non può", g.WhyNotMove(1, 1));
        // un valore salvato di una variante tolta ricade sul default
        var old = Json.Clone(R.PresetById("P01")); old.Choices["dodge"] = "SCH-CARTE";
        Assert.Equal("SCH-SCARTO", old.C("dodge"));
    }

    [Fact]
    public void Chi_viene_spostato_con_una_carta_non_combatte()
    {
        // esempio di Peppe: Dalia arriva accanto a Nebros, Nebros la sposta con Blink Step: colpisce il vuoto
        var g = New("deck_yellow", "deck_blue", pos0: "C2", pos1: "C5");
        g.Players[1].Hand = new List<string> { "b_blink_step" }; g.Players[1].Pool[Guild.Blue] = 2;
        g.Move(0, 0, C("C4"));
        Assert.Null(g.Play(1, 0, 0, C("C2"))); g.Pass(0); g.Pass(1);
        Assert.Equal(C("C2"), g.Gm(0).Pos); Assert.Equal(0, g.Gm(1).Damage); Assert.Equal(0, g.Gm(0).Damage);

        // spostata in una casella ancora a contatto: niente combattimento lo stesso (RIC-ANNULLA, default)
        g = New("deck_yellow", "deck_blue", pos0: "C2", pos1: "C5");
        g.Players[1].Hand = new List<string> { "b_blink_step" }; g.Players[1].Pool[Guild.Blue] = 2;
        g.Move(0, 0, C("C4"));
        Assert.Null(g.Play(1, 0, 0, C("B4"))); g.Pass(0); g.Pass(1);
        Assert.Equal(0, g.Gm(1).Damage);
        Assert.Contains(g.Log, l => l.Text.Contains("colpisce il vuoto"));

        // variante RIC-GEO: ancora a contatto, si combatte
        g = New("deck_yellow", "deck_blue", preset: "P01d", pos0: "C2", pos1: "C5");
        g.Players[1].Hand = new List<string> { "b_blink_step" }; g.Players[1].Pool[Guild.Blue] = 2;
        g.Move(0, 0, C("C4"));
        Assert.Null(g.Play(1, 0, 0, C("B4"))); g.Pass(0); g.Pass(1);
        Assert.True(g.Gm(1).Damage > 0);
    }

    [Fact]
    public void Restare_fermi_e_attacco_senza_movimento_sono_varianti()
    {
        var g = New("deck_red", "deck_yellow", pos0: "C2", pos1: "C3");
        Assert.NotNull(g.Move(0, 0, C("C2")));                        // default: bisogna cambiare casella
        Assert.Contains("solo dopo un movimento", g.DeclareAttack(0, 0));

        g = New("deck_red", "deck_yellow", tweak: x => x.Choices["stayMove"] = "si", pos0: "C2", pos1: "C3");
        Assert.Null(g.Move(0, 0, C("C2"))); Assert.True(g.Gm(0).Tapped); g.Pass(1);
        Assert.True(g.Gm(1).Damage > 0);                              // combattimento automatico anche da fermi
        Assert.Null(g.WhyNotEndTurn(0));                              // obbligo soddisfatto

        g = New("deck_red", "deck_yellow", tweak: x => x.Choices["attackNoMove"] = "si", pos0: "C2", pos1: "C3");
        Assert.Null(g.DeclareAttack(0, 0)); g.Pass(1);
        Assert.True(g.Gm(1).Damage > 0); Assert.True(g.Gm(0).Tapped); // tap > movimento > attacco
        Assert.Contains("tappato", g.WhyNotAttack(0, 0));
        Assert.Null(g.WhyNotEndTurn(0));                              // il tap vale come movimento del turno
        Assert.NotNull(g.Move(0, 0, C("B2")));                        // già tappato: non muove più

        g = New("deck_red", "deck_yellow", tweak: x => x.Choices["attackNoMove"] = "si", pos0: "C2", pos1: "C5");
        Assert.Contains("a contatto", g.WhyNotAttack(0, 0));          // serve un bersaglio valido
    }

    [Fact]
    public void I_preset_dell_utente_si_eliminano_quelli_di_base_no()
    {
        var repo = Repo.Load(Root);
        Assert.Throws<InvalidOperationException>(() => repo.DeletePreset(repo.PresetById("P01")));
        var p = Json.Clone(repo.PresetById("P01")); p.Id = "TEST-elimina"; p.BuiltIn = false;
        repo.SavePreset(p);
        var file = Path.Combine(Root, "presets", "user", "TEST-elimina.json");
        Assert.True(File.Exists(file));
        repo.DeletePreset(p);
        Assert.False(File.Exists(file)); Assert.Null(repo.PresetById("TEST-elimina"));
    }

    [Fact]
    public void Torri_attive_cura_danno_e_punti_vita()
    {
        Action<Preset> on = x =>
        {
            foreach (var k in new[] { "towerHealEnter", "towerHealStart", "towerDmgEnter", "towerDmgStart", "towerHp" }) x.Choices[k] = "si";
            x.Numbers["towerHealEnterX"] = 2; x.Numbers["towerHealStartX"] = 1; x.Numbers["towerDmgEnterX"] = 3; x.Numbers["towerDmgStartX"] = 2; x.Numbers["towerHpMax"] = 2;
        };
        // Torre propria (A1): cura all'ingresso e a inizio turno
        var g = New("deck_red", "deck_yellow", tweak: on, pos0: "C3", pos1: "E5"); g.Gm(0).Damage = 3;
        g.Move(0, 0, C("B2")); Assert.Equal(1, g.Gm(0).Damage);       // entra: cura 2
        g.Pass(1); End(g, 0);
        g.Move(1, 1, C("E6")); g.Pass(0); End(g, 1);
        Assert.Equal(0, g.Gm(0).Damage);                              // inizio turno: cura (fino al massimo)

        // Torre nemica (H8): danno all'ingresso; poi viene colpita e a 0 non fa più niente
        g = New("deck_red", "deck_yellow", tweak: on, pos0: "F6", pos1: "C3");
        g.Move(0, 0, C("G7")); Assert.Equal(3, g.Gm(0).Damage);
        g.Pass(1);                                                    // nessun nemico a contatto: Fiaran colpisce la Torre
        Assert.True(g.TowerHp(1) <= 0); Assert.False(g.TowerActive(1));
        End(g, 0); Assert.Null(g.Move(1, 1, C("C4"))); g.Pass(0); Assert.Null(End(g, 1));
        Assert.Equal(3, g.Gm(0).Damage);                              // Torre a 0: niente danno a inizio turno

        // Torre attiva: danno anche a inizio turno
        g = New("deck_red", "deck_yellow", tweak: x => { on(x); x.Numbers["towerHpMax"] = 30; }, pos0: "F6", pos1: "C3");
        g.Move(0, 0, C("G7")); g.Pass(1); End(g, 0); Assert.Null(g.Move(1, 1, C("C4"))); g.Pass(0); Assert.Null(End(g, 1));
        Assert.Equal(5, g.Gm(0).Damage);                              // 3 all'ingresso + 2 a inizio turno: X separati

        // default: le Torri restano inerti
        g = New("deck_red", "deck_yellow", pos0: "C3", pos1: "E5"); g.Gm(0).Damage = 3;
        g.Move(0, 0, C("B2")); Assert.Equal(3, g.Gm(0).Damage);
    }

    [Fact]
    public void Alla_fine_del_turno_si_puo_sempre_rispondere()
    {
        var g = New("deck_red", "deck_yellow", pos0: "C2", pos1: "C5");
        g.Move(0, 0, C("B2")); g.Pass(1);
        g.Players[1].Resources.Add(new Permanent { CardId = "res_yellow" });
        Assert.Null(g.EndTurn(0));
        Assert.Equal(Phase.Response, g.Phase); Assert.Equal(1, g.Actor); Assert.Equal(1, g.Turn);
        Assert.Null(g.WhyNotAbility(1));                              // l'avversario usa l'abilità a fine turno
        Assert.Null(g.UseAbility(1, 0)); g.Pass(0);
        Assert.Equal(1, g.Gm(0).Damage); Assert.Equal(1, g.Turn);     // risolta, il turno non è ancora finito
        g.Pass(1);
        Assert.Equal(2, g.Turn); Assert.Equal(Phase.Main, g.Phase); Assert.Equal(1, g.Active);
    }

    // ---------- partite complete ----------
    [Theory]
    [InlineData("P01", "deck_red", "deck_green")]
    [InlineData("P01", "deck_blue", "deck_yellow")]
    [InlineData("P01-9x7", "deck_red", "deck_blue")]
    [InlineData("P01-9x7", "deck_green", "deck_yellow")]
    [InlineData("P01m", "deck_yellow", "deck_red")]
    [InlineData("P01d", "deck_green", "deck_blue")]
    [InlineData("P01e", "deck_red", "deck_yellow")]
    [InlineData("P01g", "deck_blue", "deck_yellow")]
    [InlineData("P01n", "deck_red", "deck_green")]
    [InlineData("P01l", "deck_yellow", "deck_red")]
    [InlineData("P01j", "deck_green", "deck_blue")]
    [InlineData("P01k", "deck_blue", "deck_green")]
    [InlineData("P01h", "deck_red", "deck_blue")]
    [InlineData("P01i", "deck_green", "deck_yellow")]
    public void Partita_completa_simulata_con_seed_fisso(string preset, string d0, string d1)
    {
        Game Run() => Bot.Play(R.NewGame(R.PresetById(preset), R.Deck(d0), R.Deck(d1), 20261114), 7);
        var g = Run();
        Assert.Equal(Phase.Over, g.Phase);
        Assert.InRange(g.Turn, 4, 80);
        var again = Run();
        Assert.Equal(g.Winner, again.Winner); Assert.Equal(g.Log.Select(l => l.Text), again.Log.Select(l => l.Text)); // stesso seed, stessa partita

        var md = Report.Markdown(g, new DateTime(2026, 11, 14, 10, 0, 0), new DateTime(2026, 11, 14, 10, 20, 0), "nota di prova");
        Assert.Contains("Seed: 20261114", md); Assert.Contains("Simbolo gilda gialla: stivale", md); Assert.Contains("nota di prova", md);
        Assert.Contains("Pescate", md); Assert.DoesNotContain("NON VALIDA", md);
        var dir = Path.Combine(Root, "..", "docs", "logs"); Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, $"simulazione_{preset}_{d0}_{d1}.md"), md);
    }

    [Fact]
    public void Salvataggio_della_situazione_e_marcatura_debug()
    {
        var g = New("deck_red", "deck_yellow", pos0: "C2", pos1: "C5");
        g.Move(0, 0, C("C4"));
        var copy = Json.Clone(g);                     // situazione salvata a metà di una finestra di risposta
        Assert.Equal(Phase.Response, copy.Phase); Assert.Equal(C("C4"), copy.Gm(0).Pos);
        Assert.Null(copy.Pass(1)); Assert.Equal(2, copy.Gm(1).Damage);
        Assert.False(g.DebugTouched);
        g.DebugEdit("Salute di Dalia portata a 1", x => x.Gm(1).Damage = 19);
        Assert.Contains("NON VALIDA PER IL CONFRONTO", Report.Markdown(g, DateTime.Now, DateTime.Now, ""));
    }
}
