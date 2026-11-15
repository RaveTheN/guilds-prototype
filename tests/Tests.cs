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
    /// <summary>Mette in campo un alleato già evocato (la carta deve stare in uno dei due mazzi).</summary>
    static Unit Put(Game g, int owner, string card, string pos)
    {
        var u = new Unit { Id = g.Units.Count, Owner = owner, CardId = card, Pos = C(pos), Placed = true };
        g.Units.Add(u); return u;
    }

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
        foreach (var d in R.Decks.Where(d => !d.Id.Contains("-mio")))   // solo i mazzi di base: quelli creati con l'editor sono dell'utente
        {   // i mazzi con alleati valgono solo nei preset con gli alleati
            Assert.Empty(R.ValidateDeck(d, R.PresetById("P02")));
            Assert.Equal(d.Id.EndsWith("_alleati"), R.ValidateDeck(d, R.PresetById("P01")).Count > 0);
        }
        Assert.Empty(R.Validate(R.PresetById("P01"), R.Deck("deck_red"), R.Deck("deck_blue")));
        Assert.Empty(R.Validate(R.PresetById("P01-9x7"), R.Deck("deck_green"), R.Deck("deck_yellow")));
        Assert.Contains(R.Validate(R.PresetById("P01b"), R.Deck("deck_red"), R.Deck("deck_blue")), e => e.Contains("angoli"));
        var z = Json.Clone(R.PresetById("P01")); z.Choices["tokenMode"] = "ruolo";
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
    public void I_preset_dell_utente_si_eliminano_quelli_di_base_si_nascondono()
    {
        var repo = Repo.Load(Root); string real = repo.Root;
        repo.Root = Path.Combine(Path.GetTempPath(), "guilds-hidden-" + Guid.NewGuid().ToString("N"));
        repo.RestorePresets();                                               // parte da un elenco completo, qualunque cosa abbia nascosto l'utente
        repo.DeletePreset(repo.PresetById("P04"));
        Assert.DoesNotContain(repo.Visible, x => x.Id == "P04"); Assert.NotNull(repo.PresetById("P04"));
        Assert.Contains("P04", File.ReadAllLines(Path.Combine(repo.Root, "presets", "user", "hidden.txt")));
        repo.RestorePresets();
        Assert.Contains(repo.Visible, x => x.Id == "P04");
        Directory.Delete(repo.Root, true); repo.Root = real;
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

    // ---------- evoluzioni: alleati, Assalto, scacchi, mostri, risorse, edifici, zone ----------
    [Fact]
    public void Alleati_evocazione_limiti_morte_e_obbligo()
    {
        var g = New("deck_red_alleati", "deck_yellow_alleati", preset: "P02", pos0: "B2", pos1: "G7");
        var p = g.Players[0]; p.Hand = new List<string> { "r_forge_recruit", "r_bomber" }; p.Pool[Guild.Red] = 3;
        Assert.Contains(C("C3"), g.SummonCells(0));
        Assert.DoesNotContain(C("F5"), g.SummonCells(0));             // territorio avversario
        Assert.DoesNotContain(C("D3"), g.SummonCells(0));             // highground (F1-20)
        Assert.Contains("Casella", g.Play(0, 0, -1, C("F5")));
        Assert.Null(g.Play(0, 0, -1, C("C3"))); g.Pass(1);
        var rec = g.Units[2];
        Assert.True(rec.Placed && rec.Tapped); Assert.Equal(C("C3"), rec.Pos);    // F1-30: entra tappato
        Assert.NotNull(g.WhyNotMove(0, 2));
        Assert.Null(g.Play(0, 0, -1, C("C4"))); g.Pass(1);
        Assert.False(g.Units[3].Tapped);                              // Swift (F1-62)
        Assert.Contains("OBB-A", g.WhyNotEndTurn(0));
        Assert.Null(g.Move(0, 3, C("C5"))); g.Pass(1);
        Assert.Null(g.WhyNotEndTurn(0));                              // OBB-A: basta un'unità
        g.Preset.Choices["obligation"] = "OBB-B";
        Assert.Contains("OBB-B", g.WhyNotEndTurn(0));                 // OBB-B: deve muovere anche il Guildmaster
        g.DebugEdit("prova", x => x.Units[2].Damage = 3);
        Assert.True(rec.Dead); Assert.False(rec.Placed); Assert.Contains("r_forge_recruit", p.Discard); Assert.Equal(Phase.Main, g.Phase);

        g = New("deck_red_alleati", "deck_yellow_alleati", preset: "P02", pos0: "B2", pos1: "G7", tweak: x => x.Numbers["maxUnits"] = 2);
        g.Players[0].Hand = new List<string> { "r_forge_recruit", "r_bomber" }; g.Players[0].Pool[Guild.Red] = 3;
        Assert.Null(g.Play(0, 0, -1, C("C3"))); g.Pass(1);
        Assert.Contains("massimo 2", g.WhyNotPlay(0, 0));             // F2-01
    }

    [Fact]
    public void Con_piu_nemici_a_portata_sceglie_chi_attacca()
    {
        var g = New("deck_red_alleati", "deck_yellow_alleati", preset: "P02", pos0: "B2", pos1: "B4");
        var cut = Put(g, 1, "y_cutpurse", "C4");
        g.Move(0, 0, C("B3"), prefer: cut.Id); g.Pass(1);
        Assert.True(cut.Dead); Assert.Equal(0, g.Gm(1).Damage); Assert.Equal(1, g.Gm(0).Damage);   // il Tagliaborse restituisce 1
        g = New("deck_red_alleati", "deck_yellow_alleati", preset: "P02", pos0: "B2", pos1: "B4");
        Put(g, 1, "y_cutpurse", "C4");
        g.Move(0, 0, C("B3")); g.Pass(1);
        Assert.Equal(2, g.Gm(1).Damage);                              // senza scelta: il Guildmaster per primo
    }

    [Fact]
    public void Vita_ripristinata_e_Assalto()
    {
        var g = New("deck_red_alleati", "deck_yellow_alleati", preset: "P02a", pos0: "B2", pos1: "G7");
        var thr = Put(g, 1, "y_knife_thrower", "B5");                // a distanza, Attacco 1, Salute 3
        Assert.Contains(C("B5"), g.MovesFor(0).Keys);
        Assert.Null(g.Move(0, 0, C("B5")));
        Assert.True(Board.Adjacent(g.Gm(0).Pos, C("B5"), 8));         // si ferma accanto finché l'Assalto non è risolto
        g.Pass(1);
        Assert.True(thr.Dead); Assert.Equal(C("B5"), g.Gm(0).Pos);    // due scambi: 2+2 contro 3; chi vince occupa la casella
        Assert.Equal(2, g.Gm(0).Damage);                              // N-04: il difensore a distanza fa 1 a scambio

        g = New("deck_red_alleati", "deck_yellow_alleati", preset: "P02a", pos0: "B2", pos1: "B4");
        Assert.DoesNotContain(C("B4"), g.MovesFor(0).Keys);           // non contro i Guildmaster…
        g.Preset.Choices["assault"] = "tutti";
        Assert.Contains(C("B4"), g.MovesFor(0).Keys);                 // …salvo la variante «tutti»
        g.Preset.Choices["assault"] = "nogm";
        Assert.DoesNotContain(C("B4"), g.MovesFor(0).Keys);           // «nogm»: non Guildmaster contro Guildmaster…
        Assert.Contains(C("B4"), g.MovesFor(Put(g, 0, "r_forge_recruit", "A3").Id).Keys); // …ma un alleato può assaltare il Guildmaster
        var ally = Put(g, 0, "r_forge_recruit", "C2"); ally.Damage = 1;
        g.Move(0, 2, C("C1")); g.Pass(1); End(g, 0);
        Assert.Equal(0, ally.Damage);                                 // N-01: a inizio turno torna a vita piena
    }

    [Fact]
    public void Doppio_profilo_d_attacco()
    {
        var g = New("deck_red_alleati", "deck_yellow_alleati", preset: "P02", pos0: "B2", pos1: "B5");
        Assert.False(g.InAttackRange(g.Gm(0), g.Gm(1)));              // Fiaran è in mischia: a distanza non attacca
        g.Preset.Choices["attackProfile"] = "doppio";
        Assert.True(g.InAttackRange(g.Gm(0), g.Gm(1)));
        Assert.Equal(1, g.AttackAt(g.Gm(0), C("B5"))); Assert.Equal(2, g.AttackAt(g.Gm(0), C("B3")));   // H10
        g.Move(0, 0, C("B3")); g.Pass(1);
        Assert.Equal(1, g.Gm(1).Damage); Assert.Equal(0, g.Gm(0).Damage);
    }

    [Fact]
    public void Mosse_degli_scacchi_e_un_alleato_per_ruolo()
    {
        var b = Mini(".....", ".....", ".....", ".....", ".....");
        int N(string role) => b.ChessReach(C("C3"), role, c => true).Count;
        Assert.Equal((8, 8, 8, 8, 4, 16, 24), (N("rook"), N("bishop"), N("knight"), N("king"), N("pawn"), N("queen"), N("queenknight")));
        Assert.Equal(6, b.ChessReach(C("C3"), "rook", c => !c.Equals(C("C4"))).Count);   // non si attraversano le unità
        Assert.Equal(8, b.ChessReach(C("C3"), "knight", c => !c.Equals(C("C4"))).Count); // il Cavallo salta

        var g = New("deck_red_alleati", "deck_blue_alleati", preset: "P03", pos0: "B3", pos1: "G7");
        Assert.All(g.MovesFor(0).Keys, c => Assert.True(Board.Adjacent(c, C("B3"), 8)));  // Guildmaster come il Re
        g.Players[0].Hand = new List<string> { "r_forge_recruit", "r_forge_recruit" }; g.Players[0].Pool[Guild.Red] = 4;
        Assert.Null(g.Play(0, 0, -1, C("C4"))); g.Pass(1);
        Assert.All(g.MovesFor(2).Keys, c => Assert.True(c.X == 2 || c.Y == 3));            // la Recluta è una Torre
        Assert.Contains("F4C-02", g.WhyNotPlay(0, 0));
    }

    [Fact]
    public void Mostri_attivi_e_vittoria_con_i_mostri()
    {
        var g = New("deck_red", "deck_yellow", preset: "P04", pos0: "F4", pos1: "A4", tweak: x => { x.Numbers["monsterN_cura"] = 10; x.Numbers["monsterN_attacco"] = 0; x.Numbers["monsterN_pesca"] = 0; });
        g.Gm(0).Damage = 5;
        g.Move(0, 0, C("G3")); g.Pass(1);
        Assert.Equal(2, g.MonsterHp(C("G2"))); Assert.Equal(6, g.Gm(0).Damage);            // 2 danni al mostro, che ne restituisce 1
        End(g, 0); g.Move(1, 1, C("A3")); g.Pass(0); End(g, 1);
        g.Move(0, 0, C("F3")); g.Pass(1);
        Assert.Equal(1, g.Players[0].MonsterKills); Assert.Equal(3, g.Gm(0).Damage);       // sconfitto: cura 3
        Assert.Equal(Phase.Main, g.Phase);                                                  // servono 2 mostri

        g = New("deck_red", "deck_yellow", preset: "P04", pos0: "F4", pos1: "A4", tweak: x => { x.Numbers["monsterN_cura"] = 10; x.Numbers["monsterN_attacco"] = 0; x.Numbers["monsterN_pesca"] = 0; x.Numbers["monsterHp_cura"] = 2; x.Numbers["monsterWin"] = 1; });
        g.Move(0, 0, C("G3")); g.Pass(1);
        Assert.Equal(Phase.Over, g.Phase); Assert.Equal(0, g.Winner); Assert.Contains("mostri", g.WinReason);

        g = New("deck_red", "deck_yellow", pos0: "F4", pos1: "A4");                         // P01: mostri inerti
        g.Move(0, 0, C("G3")); g.Pass(1);
        Assert.Empty(g.MonsterDamage); Assert.Empty(g.MonsterKind);
    }

    [Fact]
    public void Scelta_fra_bersagli_validi_e_mostro_da_non_attaccare()
    {
        Game New2() => New("deck_red", "deck_yellow", preset: "P04", pos0: "F4", pos1: "G4", tweak: x => { x.Numbers["monsterN_cura"] = 10; x.Numbers["monsterN_attacco"] = 0; x.Numbers["monsterN_pesca"] = 0; });
        var g = New2();                                                     // in G3 sono a portata sia il Guildmaster nemico (G4) sia il mostro (G2)
        Assert.Contains(g.TargetChoices(0), t => t.Code == Game.CellTarget(C("G2")));
        g.Move(0, 0, C("G3")); g.Pass(1);
        Assert.Equal(4, g.MonsterHp(C("G2"))); Assert.True(g.Gm(1).Damage > 0);            // senza scelta: l'unità nemica
        g = New2();
        g.Move(0, 0, C("G3"), prefer: Game.CellTarget(C("G2"))); g.Pass(1);
        Assert.Equal(2, g.MonsterHp(C("G2"))); Assert.Equal(0, g.Gm(1).Damage);            // bersaglio scelto: il mostro

        g = New("deck_red", "deck_yellow", preset: "P04", pos0: "F4", pos1: "A4", tweak: x => { x.Numbers["monsterN_cura"] = 10; x.Numbers["monsterN_attacco"] = 0; x.Numbers["monsterN_pesca"] = 0; });
        g.Players[0].SkipMonsters.Add("G2");                                               // spunta «non attaccare»
        Assert.DoesNotContain(g.TargetChoices(0), t => t.Code == Game.CellTarget(C("G2")));
        g.Move(0, 0, C("G3")); g.Pass(1);
        Assert.Equal(4, g.MonsterHp(C("G2"))); Assert.Equal(0, g.Gm(0).Damage);
    }

    [Fact]
    public void Mostri_tre_tipi_mazzo_e_vita_ripristinata()
    {
        var g = New("deck_red", "deck_yellow", preset: "P04", pos0: "F4", pos1: "A4");
        int cells = g.Board.All().Count(c => g.Board.Code(c) == "M");
        Assert.Equal(cells, g.MonsterKind.Count); Assert.Equal(10 - cells, g.MonsterDeck.Count);   // mazzo di 10: uno per casella M
        Assert.All(g.MonsterKind.Values, k => Assert.Contains(k, Game.MonsterKinds));

        // solo mostri «attacco» con 2 punti vita; sconfitto, ne esce un altro dal mazzo
        g = New("deck_red", "deck_yellow", preset: "P04", pos0: "F4", pos1: "A4", tweak: x =>
        { x.Numbers["monsterN_cura"] = 0; x.Numbers["monsterN_pesca"] = 0; x.Numbers["monsterN_attacco"] = 10; x.Numbers["monsterHp_attacco"] = 2; x.Numbers["monsterGain_attacco"] = 2; x.Numbers["monsterWin"] = 0; x.Choices["monsterNext"] = "nuovo"; });
        int left = g.MonsterDeck.Count;
        g.Move(0, 0, C("G3")); g.Pass(1);
        Assert.Single(g.Gm(0).Mods, m => m.Source == "mostro" && m.Amount == 2);                  // +2 Attacco, come impostato
        Assert.Equal(2, g.MonsterHp(C("G2"))); Assert.Equal(left - 1, g.MonsterDeck.Count);       // mostro nuovo, a vita piena

        // vita ripristinata: il danno al mostro sparisce al turno dopo
        g = New("deck_red", "deck_yellow", preset: "P04", pos0: "F4", pos1: "A4", tweak: x => { x.Numbers["monsterN_cura"] = 10; x.Numbers["monsterN_attacco"] = 0; x.Numbers["monsterN_pesca"] = 0; x.Choices["monsterLife"] = "ripristinata"; });
        g.Move(0, 0, C("G3")); g.Pass(1);
        Assert.Equal(2, g.MonsterHp(C("G2")));
        End(g, 0);
        Assert.Equal(4, g.MonsterHp(C("G2")));

        // mazzo dei mostri più piccolo delle caselle mostro: configurazione non valida
        var p = Json.Clone(R.PresetById("P04")); foreach (var k in Game.MonsterKinds) p.Numbers["monsterN_" + k] = 0;
        Assert.Contains(R.Validate(p, R.Deck("deck_red"), R.Deck("deck_yellow")), e => e.Contains("mazzo dei mostri"));
    }

    [Fact]
    public void Vittoria_distruggendo_la_Torre()
    {
        var g = New("deck_red", "deck_yellow", preset: "P01o", pos0: "F6", pos1: "C3", tweak: x => x.Numbers["towerHpMax"] = 2);
        g.Move(0, 0, C("G7")); g.Pass(1);
        Assert.Equal(Phase.Over, g.Phase); Assert.Equal(0, g.Winner); Assert.Contains("Torre", g.WinReason);
    }

    [Fact]
    public void Risorse_fuori_dal_mazzo()
    {
        var g = New("deck_red", "deck_yellow", preset: "P05");
        Assert.All(g.Players, p => Assert.Equal(3, p.Resources.Count));
        Assert.All(g.Players, p => Assert.DoesNotContain(p.Deck.Concat(p.Hand), id => g.Def(id).Type == CardType.Resource));
        Assert.Equal(3, g.AvailableTotal(1));                          // pronte anche nel turno avversario

        g = New("deck_red", "deck_yellow", preset: "P05a");
        Assert.Equal((1, 0), (g.Players[0].Resources.Count, g.Players[1].Resources.Count));
        g.Move(0, 0, g.MovesFor(0).Keys.First()); g.Pass(1); End(g, 0);
        g.Move(1, 1, g.MovesFor(1).Keys.First()); g.Pass(0); End(g, 1);
        Assert.Equal((2, 1), (g.Players[0].Resources.Count, g.Players[1].Resources.Count));

        g = New("deck_red", "deck_yellow", tweak: x => x.Choices["gmResource"] = "si");
        Assert.Equal(1, g.AvailableTotal(0)); Assert.Equal(0, g.AvailableTotal(1));        // BL-21
    }

    [Fact]
    public void Edifici_distruttibili()
    {
        var g = New("deck_red", "deck_yellow", preset: "P06", pos0: "C2", pos1: "G7");
        g.Players[1].Resources.Add(new Permanent { CardId = "res_yellow" });
        g.Players[0].Hand = new List<string> { "r_thermal_surge" }; g.Players[0].Pool[Guild.Red] = 2;
        Assert.Null(g.WhyNotPlay(0, 0));                               // nessuna unità in vista, ma c'è un edificio
        Assert.Null(g.Play(0, 0, Game.BuildingTarget(1, 0))); g.Pass(1);
        Assert.Empty(g.Players[1].Resources); Assert.Contains("res_yellow", g.Players[1].Discard);   // 3 danni contro 2 di Struttura

        g = New("deck_red", "deck_yellow", pos0: "C2", pos1: "G7");                         // P01: le risorse non si colpiscono
        g.Players[1].Resources.Add(new Permanent { CardId = "res_yellow" });
        g.Players[0].Hand = new List<string> { "r_thermal_surge" }; g.Players[0].Pool[Guild.Red] = 2;
        g.Gm(1).Pos = C("C4");
        Assert.Contains("Edificio non valido", g.Play(0, 0, Game.BuildingTarget(1, 0)));
    }

    [Fact]
    public void Linea_di_vista_a_zone()
    {
        var b = new Board(R.Map("mappa_8x8"));
        Assert.Equal(("G1", "G2"), (b.Zone(C("B2")), b.Zone(C("F6"))));
        Assert.StartsWith("H", b.Zone(C("D3"))); Assert.Equal("", b.Zone(C("D4")));
        Assert.True(b.LosZone(C("B2"), C("C5"))); Assert.True(b.LosZone(C("B2"), C("D3"))); Assert.False(b.LosZone(C("B2"), C("F6")));
        var g = New("deck_blue", "deck_yellow", preset: "P07", pos0: "B4", pos1: "F4");
        Assert.False(g.Sees(g.Gm(0), g.Gm(1).Pos));                    // territori non confinanti
        g.Gm(1).Pos = C("D3");
        Assert.True(g.Sees(g.Gm(0), g.Gm(1).Pos));                     // l'highground confina col territorio
    }

    [Fact]
    public void Evocazione_accanto_alla_Torre_debolezza_e_stesso_nome()
    {
        Game Make(string same, string pos0 = "C4") => New("deck_red_alleati", "deck_yellow_alleati", preset: "P02", pos0: pos0, pos1: "G7",
            tweak: x => { x.Choices["summonZone"] = "torre"; x.Choices["summonSick"] = "sni"; x.Choices["sameName"] = same; });
        var g = Make("nuova");
        Assert.Equal(new[] { "A2", "B1", "B2" }, g.SummonCells(0).Select(c => c.Name).OrderBy(n => n));
        g.Players[0].Hand = new List<string> { "r_forge_recruit", "r_forge_recruit", "r_bomber" }; g.Players[0].Pool[Guild.Red] = 5;
        Assert.Null(g.Play(0, 0, -1, C("B2"))); g.Pass(1);
        Assert.False(g.Units[2].Tapped); Assert.Contains("debolezza", g.WhyNotMove(0, 2));   // pronto, ma non si può tappare
        Assert.Null(g.Play(0, 1, -1, C("A2"))); g.Pass(1);
        Assert.Null(g.WhyNotMove(0, 3));                               // Swift
        Assert.Null(g.Play(0, 0, -1, C("B1"))); g.Pass(1);
        Assert.Equal(2, g.Namesakes(0, g.Def("r_forge_recruit")).Count);                      // «nuova»: due pedine con lo stesso nome
        g.Move(0, 0, C("C3")); g.Pass(1); End(g, 0); g.Move(1, 1, g.MovesFor(1).Keys.First()); g.Pass(0); End(g, 1);
        Assert.Null(g.WhyNotMove(0, 2));                               // dal turno dopo la debolezza è finita

        g = Make("rimpiazza");
        var old = Put(g, 0, "r_forge_recruit", "B3");
        g.Players[0].Hand = new List<string> { "r_forge_recruit" }; g.Players[0].Pool[Guild.Red] = 2;
        Assert.Null(g.Play(0, 0, -1, C("B2"))); g.Pass(1);
        Assert.True(old.Dead); Assert.Equal(C("B2"), Assert.Single(g.Namesakes(0, g.Def("r_forge_recruit"))).Pos);
        Assert.Contains("r_forge_recruit", g.Players[0].Discard);

        g = Make("sostituisce");
        old = Put(g, 0, "r_forge_recruit", "B3"); old.Damage = 2; old.Venom = 1;
        old.Mods.Add(new Modifier { Stat = "attack", Amount = -2, ExpiresTurn = -1, Source = "x" }); old.Mods.Add(new Modifier { Stat = "move", Amount = 1, ExpiresTurn = -1, Source = "y" });
        g.Players[0].Hand = new List<string> { "r_forge_recruit" }; g.Players[0].Pool[Guild.Red] = 2;
        Assert.Equal(new[] { C("B3") }, g.SummonDests(0, g.Def("r_forge_recruit")));
        Assert.Null(g.Play(0, 0, -1, C("B3"), null, new List<int> { 0 })); g.Pass(1);
        Assert.Equal(3, g.Units.Count); Assert.Equal((0, 0), (old.Damage, old.Venom));       // stessa pedina, rinnovata
        Assert.Equal("move", Assert.Single(old.Mods).Stat);            // tolto solo l'effetto scelto
        Assert.Contains("r_forge_recruit", g.Players[0].Discard);
    }

    [Fact]
    public void Pezzo_scelto_all_ingresso_e_Guildmaster_a_tre_passi()
    {
        var g = New("deck_red_alleati", "deck_blue_alleati", preset: "P03b", pos0: "B3", pos1: "G7", tweak: x => x.Choices["gmRole"] = "steps");
        Assert.Contains(C("B1"), g.MovesFor(0).Keys);                  // il Guildmaster muove a passi, non come il Re
        g.Players[0].Hand = new List<string> { "r_forge_recruit", "r_bomber" }; g.Players[0].Pool[Guild.Red] = 3;
        Assert.Contains("pezzo", g.Play(0, 0, -1, C("C4")));           // senza pezzo non entra
        Assert.Null(g.Play(0, 0, -1, C("C4"), "knight")); g.Pass(1);
        Assert.Equal("knight", g.RoleOf(g.Units[2]));
        var free = g.AvailableRoles(0, g.Def("r_bomber"));
        Assert.DoesNotContain("knight", free); Assert.Equal(5, free.Count);                  // restano Alfiere, Re, Regina, Torre, Pedone
        Assert.Contains("pezzo", g.Play(0, 0, -1, C("C5"), "knight"));
        Assert.Null(g.Play(0, 0, -1, C("C5"), "pawn")); g.Pass(1);
        Assert.Contains("pawn", g.AvailableRoles(0, g.Def("r_bomber")));                     // i Pedoni sono due
        Assert.Contains("king", g.AvailableRoles(0, g.Def("r_bomber")));                     // il Guildmaster non è il Re: il Re è disponibile
        g.Preset.Choices["gmRole"] = "king";
        Assert.DoesNotContain("king", g.AvailableRoles(0, g.Def("r_bomber")));               // il Re è del Guildmaster
    }

    [Fact]
    public void Mazzi_ottimizzati_e_regole_raggruppate()
    {
        var d = R.OptimizedDeck("gm_fiaran", R.PresetById("P01"));
        Assert.Empty(R.ValidateDeck(d, R.PresetById("P01")));
        Assert.Equal((25, 9), (d.Cards.Sum(e => e.Count), d.Cards[0].Count));
        Assert.DoesNotContain(d.Cards, e => R.AnyVersion(e.Card).Type == CardType.Ally);      // senza alleati
        d = R.OptimizedDeck("gm_fiaran", R.PresetById("P02"));
        Assert.Empty(R.ValidateDeck(d, R.PresetById("P02")));
        Assert.Equal((25, 9), (d.Cards.Sum(e => e.Count), d.Cards[0].Count));
        Assert.Equal(4, d.Cards.Count(e => R.AnyVersion(e.Card).Type == CardType.Ally));      // con alleati
        var big = Json.Clone(R.PresetById("P02")); big.Numbers["deckMin"] = 40;
        d = R.OptimizedDeck("gm_fiaran", big);
        Assert.Empty(R.ValidateDeck(d, big));
        Assert.Equal((41, 15), (d.Cards.Sum(e => e.Count), d.Cards[0].Count));               // più carte, più risorse

        var keys = PresetSchema.Groups.SelectMany(g => g.Keys).ToList();
        Assert.Equal(PresetSchema.Params.Select(p => p.Key).OrderBy(k => k), keys.OrderBy(k => k));   // ogni regola sta in un tema, una volta sola
    }

    [Fact]
    public void Mazzo_dell_utente_salvato_e_ricaricato()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "guilds_test_" + Guid.NewGuid().ToString("N"));
        var repo = Repo.Load(Root); repo.Root = tmp;
        var d = Json.Clone(repo.Deck("deck_red")); d.Id = "deck_red-mio1"; d.Cards[1].Count = 3;
        repo.SaveDeck(d);
        Assert.Same(d, repo.Deck("deck_red-mio1"));
        Assert.Equal(3, Json.Read<DeckDef>(Path.Combine(tmp, "decks", "user", "deck_red-mio1.json")).Cards[1].Count);
        Assert.Equal(4, repo.Deck("deck_red").Cards[1].Count);        // l'originale non cambia
        Directory.Delete(tmp, true);
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
    [InlineData("P01o", "deck_red", "deck_blue")]
    [InlineData("P02", "deck_red_alleati", "deck_green_alleati")]
    [InlineData("P02a", "deck_blue_alleati", "deck_yellow_alleati")]
    [InlineData("P02b", "deck_yellow_alleati", "deck_red_alleati")]
    [InlineData("P02c", "deck_green_alleati", "deck_blue_alleati")]
    [InlineData("P03", "deck_red_alleati", "deck_blue_alleati")]
    [InlineData("P03a", "deck_red", "deck_green")]
    [InlineData("P03b", "deck_green_alleati", "deck_yellow_alleati")]
    [InlineData("P04", "deck_red", "deck_yellow")]
    [InlineData("P05", "deck_green", "deck_blue")]
    [InlineData("P05a", "deck_red", "deck_yellow")]
    [InlineData("P06", "deck_blue", "deck_red")]
    [InlineData("P07", "deck_green", "deck_yellow")]
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
