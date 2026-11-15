using System;
using System.IO;
using System.Linq;
using System.Text;
using Guilds.Core;
using Xunit;

public class CatalogTest
{
    /// <summary>Scrive docs/CARDS.md dagli stessi dati e dalla stessa logica usati in partita.</summary>
    [Fact]
    public void Catalogo_carte_generato_dai_dati()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (!Directory.Exists(Path.Combine(d.FullName, "data", "features"))) d = d.Parent;
        var R = Repo.Load(Path.Combine(d.FullName, "data"));
        var sb = new StringBuilder();
        sb.AppendLine("# Catalogo delle carte — Guilds prototipo").AppendLine();
        sb.AppendLine("Generato da `dotnet test` a partire da `data/cards/`. Non modificare a mano: cambia `tools/gen_content.py` e rilancia.").AppendLine();
        sb.AppendLine("Tutte le carte sono **carte di test proposte**, non canon. Il testo è generato dagli effetti eseguibili, quindi coincide con la logica.").AppendLine();
        foreach (var grp in R.CardVersions.Values.SelectMany(v => v).GroupBy(c => c.Guild))
        {
            sb.AppendLine($"## Gilda {Names.Guild(grp.Key)}").AppendLine();
            foreach (var c in grp)
            {
                sb.AppendLine($"### {c.Name}");
                sb.AppendLine($"- ID e versione: `{c.Key}` — {Names.Type(c.Type)} — costo {CardRules.Cost(c, R.PresetById("P01"))}"
                    + (c.Type == CardType.Guildmaster || c.Type == CardType.Ally ? $" — Attacco {c.Attack}, Salute {c.Health}, {c.Range}" : ""));
                sb.AppendLine($"- Testo effettivo: {CardRules.Text(c)}");
                if (c.DeferredText != null) sb.AppendLine($"- Non attivo in questo set: {c.DeferredText}");
                sb.AppendLine($"- Origine: {c.OriginKind} — {c.OriginSource}");
                sb.AppendLine($"- Serve a testare: {c.Tests}");
                sb.AppendLine($"- Motivazione dei valori: {c.Rationale}");
                sb.AppendLine($"- Regole richieste: {string.Join(", ", CardRules.Requirements(c))}");
                sb.AppendLine($"- Preset compatibili: {string.Join(", ", R.Presets.Where(p => CardRules.Missing(c, p).Count == 0).Select(p => p.Id))}");
                if (c.TokenModel != null) sb.AppendLine($"- Modello del token: {c.TokenModel}");
                sb.AppendLine();
            }
        }
        sb.AppendLine("## Mazzi").AppendLine();
        foreach (var k in R.Decks)
            sb.AppendLine($"- **{k.Name}** (`{k.Id}` v{k.Version}), Guildmaster {R.AnyVersion(k.Guildmaster).Name}: " + string.Join(", ", k.Cards.Select(e => $"{e.Count}× {R.AnyVersion(e.Card).Name}")));
        File.WriteAllText(Path.Combine(d.FullName, "docs", "CARDS.md"), sb.ToString());
        Assert.Equal(40, R.CardVersions.Count);
    }
}
