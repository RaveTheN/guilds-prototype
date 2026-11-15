# Guilds — prototipo di playtest

Prototipo giocabile in locale (due giocatori sullo stesso computer) per provare regole trasferibili al gioco fisico.
**È un test parziale**: i preset coprono un sottoinsieme dell'inventario; P02–P07 aggiungono alleati, Assalto, scacchi, mostri, risorse fuori dal mazzo, edifici e linea di vista a zone, quasi tutti basati su ipotesi dichiarate. Stato e limiti: `docs/PROGRESS.md`.

## Avvio (Unity 6000.3.10f1)

1. Apri Unity Hub → **Add → Add project from disk** → scegli `C:\Users\peppe\source\repos\Guilds`.
2. Apri il progetto con l'editor **6000.3.10f1**.
3. Menu **Guilds → Gioca** (Ctrl+G). La scena `Assets/Guilds/Scenes/Guilds.unity` è già generata; **Guilds → Genera scena** la ricrea.
4. Nella schermata di configurazione scegli preset, mazzi e seed, poi **AVVIA PARTITA**.

Nessuna configurazione nell'Inspector.

## Comandi in partita

| Azione | Come |
|---|---|
| Mulligan iniziale | clic sulle carte da cambiare, poi **Cambia N** oppure **Tieni la mano** |
| Scegliere la partenza | clic su una casella gialla |
| Selezionare un'unità / vedere mosse e linea di vista | clic sul token (verde = destinazioni legali, linea gialla = vede il nemico) |
| Muovere | clic su una casella verde (l'unità si tappa; poi risponde l'avversario) |
| Giocare una carta | clic sulla carta in mano → **GIOCA** in basso a destra; se serve un bersaglio, clic sull'unità con la scritta BERSAGLIO, poi sull'eventuale destinazione blu |
| Risorse | le carte risorsa si giocano gratis, una per turno, e restano sul tavolo accanto alla board; si tappano da sole quando paghi e si stappano all'inizio del tuo turno. Quante ne puoi spendere è in «Da spendere ora» |
| Abilità del Guildmaster | clic sul tuo Guildmaster (token o carta sul tavolo) → **USA L'ABILITÀ**, poi il bersaglio o la casella; una volta per turno |
| Passare in risposta | **Passa** o Spazio |
| Ruotare e inclinare la board | trascina col tasto destro; zoom con la rotella; T = vista dall'alto perfetta; **Vista iniziale** per tornare indietro |
| Annullare la selezione | Esc |
| Regole attive, note del tester, carte | pulsanti in alto |
| Salvare una situazione / esportare il resoconto | pulsanti in alto → `reports/` |

Il motivo di ogni azione vietata compare in rosso. A ogni cambio di giocatore la mano viene coperta finché chi deve agire non conferma.

## Verifiche da riga di comando

```bash
dotnet test tests
```

```bash
"D:\Program Files\6000.3.10f1\Editor\Unity.exe" -batchmode -nographics -projectPath . -executeMethod Guilds.EditorTools.GuildsSetup.BatchCheck -quit -logFile docs/logs/unity_batch.log
```

## Struttura

| Cartella | Contenuto |
|---|---|
| `Assets/Guilds/Core` | Logica (assembly `Guilds.Core`, senza UnityEngine): `Model.cs` definizioni e schema dei preset, `Board.cs` percorsi e linea di vista, `Game.cs` stato e risoluzione, `Data.cs` caricamento, validazioni, resoconto |
| `Assets/Guilds/Unity` | Interfaccia: `View.cs` board 3D e tema, `ConfigScreen.cs`, `PlayScreen.cs`, `GuildsApp.cs` |
| `core/`, `tests/` | Progetti .NET che compilano e testano gli stessi sorgenti fuori da Unity |
| `data/` | Tutto ciò che si cambia senza toccare il codice (vedi sotto) |
| `tools/` | Script Python: registro delle feature, conversione mappe e STL, generazione di carte/mazzi/preset |
| `docs/` | Avanzamento, tabella delle feature, catalogo carte, domande aperte, log |
| `reference/` | Materiale originale: non modificare |
| `reports/` | Resoconti e situazioni salvate |

## Cambiare contenuti senza codice

- **Preset e regole**: nella schermata di configurazione clicca direttamente la regola da cambiare. Se il preset è di base viene creata da sola una copia tua (`data/presets/user/`). La scheda «Tutte le regole» elenca quelle attive, quelle spente ma attivabili (con il parametro da cambiare) e quelle non disponibili in questa versione, col motivo.
- **Mappe**: `data/maps/*.json`. In configurazione, **Ritocca → variante** crea `mappa_…_vN.json` senza toccare l'originale. Riconversione dall'Excel: `python tools/convert_maps.py`.
- **Mazzi tuoi**: scheda «Editor di mazzi» nella schermata di configurazione; parte da una copia di un mazzo esistente e salva in `data/decks/user/`.
- **Carte e mazzi di base**: modifica `tools/gen_content.py`, poi `python tools/gen_content.py`. Per una nuova versione di una carta aggiungi una voce con lo stesso `id` e `version` più alta: ogni preset usa la versione più recente compatibile.
- **Illustrazioni**: copia `data/art/cards/<id carta>.png` (o `.jpg`), `data/art/symbols/<red_bomb|blue_flame|green_bow|yellow_boot|yellow_dagger>.png`, `data/art/cardback.png`. Se un file manca compare un segnaposto.
- **Temi visivi**: copia `data/themes/semplice` in `data/themes/<nome>`, cambia `theme.json` (colori, altezze, `"models": {"W": "models/muro.obj"}` per sostituire la geometria di un tipo di casella) e i file OBJ. Il tema compare da solo fra quelli selezionabili.
- **Token dei Guildmaster**: campo `tokenModel` della carta, relativo alla cartella del tema; se il file manca si usa il segnalino generico. Riconversione dagli STL: `python tools/convert_stl.py`.
- **Tabella delle feature**: stato in `data/features/impl_status.json`, poi `python tools/build_feature_registry.py`.
