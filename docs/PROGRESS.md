# Avanzamento — prototipo Guilds

## Traguardo corrente
**M4 – Configurazione e varianti: completato per la famiglia P01** (14 nov 2026). M5 (tema curato con Blender) non iniziato.
Per avviare: `README.md`.

## Dati della sezione 0
- Unity **6000.3.10f1** in `D:\Program Files\6000.3.10f1\Editor\Unity.exe` (verificato).
- Mappa: `reference\fase 4b - mappa.xlsx` → `data/maps/mappa_8x8.json`, `mappa_9x7.json` (`tools/convert_maps.py`).
- Ambiente: Claude Code su Windows. .NET SDK 9.0.306, Python 3.11 con numpy.
- Blender: presente in `Blender\` (non usato finora).

## Stato dei traguardi
| | Stato | Verifica eseguita |
|---|---|---|
| M0 Analisi | fatto | vedi sotto |
| M1 Logica centrale | fatto | `dotnet test tests`: 36 test su 36 |
| M2 Scena Unity | fatto | compilazione e `BatchCheck` in Unity 6000.3.10f1: OK |
| M3 Partita minima | fatto | Play mode avviato in batch: avvio, board, token, partita guidata, export del resoconto senza eccezioni |
| M4 Configurazione e varianti | fatto per P01 | validazioni e varianti coperte dai test; **schermate non viste** |
| M5 Board curata | non iniziato | — |

## Correzioni dopo la prima prova di Peppe (14 nov 2026, v0.4.1)
| Difetto segnalato | Che cosa è cambiato |
|---|---|
| La board non ruota | Trascinamento col tasto destro: ruota e inclina; pulsante «Vista iniziale» |
| Testo sgranato | L'interfaccia non ingrandisce più l'immagine: i caratteri sono disegnati alla dimensione reale |
| Regole poco leggibili, non modificabili | Ogni opzione è una riga cliccabile (verde = attiva); modificare un preset di base ne crea da sola una copia; scheda «Tutte le regole» divisa in attive / spente attivabili / non disponibili col motivo |
| Personaggi sempre tappati | Il motore stappa a inizio turno (nuovo test: lo stesso Guildmaster muove ai turni 1, 3, 5). Non riprodotto: vedi domanda D29. Ora lo stato è scritto in chiaro e la carta del Guildmaster ruota |
| Niente Mulligan | Parametro «Mulligan iniziale» (N-06, procedura di BL-48), attivo di default |
| Non ci sono le carte risorsa / mazzi da 4 carte | Il mazzo ha 9 risorse + 4 carte × 4 copie; ora la scheda «Mazzi e carte» mostra il numero di copie su ogni carta e il totale |
| Manca la carta del Guildmaster | Mini-carta nel riquadro del giocatore, ruotata di 90° quando è tappata |
| «Bersaglio non valido» senza unità evidenziate | Il riquadro rosso era nascosto sotto la base del token: ora è più largo e c'è la scritta BERSAGLIO; se la carta non è giocabile compare il motivo vero |
| Non si capisce come giocare e dove sono le risorse | Riga di istruzioni contestuale, pulsante GIOCA in basso, righe «Carte risorsa in gioco» e «Da spendere ora» |

Verifica: `dotnet test` 32 su 32; compilazione e Play mode in batch in Unity senza eccezioni. **Le schermate continuano a non essere state viste da me.**

## Modifiche dopo la seconda prova di Peppe (14 nov 2026, v0.5.0)
Decisioni di Peppe: il Mulligan resta com'è (scegli le carte da rimettere nel mazzo, ne peschi una in meno); le istantanee si giocano anche col Guildmaster tappato, solo lo spostamento in risposta richiede l'unità stappata; le carte giocate si vedono sul tavolo con lo stato pronto/tappato; abilità proprie dei Guildmaster, una volta per turno; movimento obbligatorio disattivabile.

| Richiesta | Che cosa è cambiato |
|---|---|
| Carte sul tavolo, pronte o tappate | Ai lati della board c'è il tavolo di ogni giocatore: carta del Guildmaster, oggetti assegnati, carte risorsa; ciò che è tappato è ruotato di 90° |
| Istantanee indipendenti dal tap | Era già così nel motore, ma **chi non era di turno non aveva mai risorse**: venivano prodotte a inizio turno e perse alla fine. Ora le carte risorsa **si tappano per pagare** e si stappano all'inizio del proprio turno, quindi quelle rimaste pronte pagano istantanee e abilità nel turno avversario. Confermato da Peppe il 14 nov 2026 |
| Abilità dei Guildmaster | Fiaran (Croce, 2 danni), Senya (1 danno + cura 1), Nebros (sposta un'unità di 2), Dalia (1 danno) + Venomous (F1-65). Costo 1 risorsa della gilda, una volta per turno, anche in risposta, senza tappare. Parametro per spegnerle |
| Movimento obbligatorio disattivabile | Parametro «Movimento obbligatorio ogni turno» sì/no; preset P01e |
| Carta risorsa appena giocata | Parametro: entra tappata (default, come prima: si usa dal turno dopo) oppure pronta |

Verifica: `dotnet test` 36 su 36. Gli script dell'interfaccia compilano contro le librerie di Unity 6000.3.10f1 (controllo con `dotnet build`); il Play mode in batch **non è stato rieseguito** perché il progetto era aperto nell'editor di Peppe. Le schermate restano non viste da me.

## Decisioni successive di Peppe (14 nov 2026, v0.5.1)
- **Spostamento in risposta solo scartando una carta, e solo in risposta a un attacco** (N-08, riprende BL-34). Nuova variante di default SCH-SCARTO; SCH-TAP e SCH-CARTE restano selezionabili. «Attacco» è definito dall'ipotesi H24. Parametri: «solo in risposta a un attacco» sì/no; «l'unità deve essere stappata» sì/no (default sì, per la decisione precedente).
- **Le carte risorsa entrano stappate e producono subito** (default; la variante «entra tappata» resta come parametro).
- Conseguenza da osservare: con movimento obbligatorio e «unità stappata» entrambi attivi, il Guildmaster è tappato per tutto il turno avversario e quindi non può mai spostarsi in risposta nemmeno scartando (domanda D36).
- Verifica: `dotnet test` 37 su 37; script dell'interfaccia compilati contro le librerie di Unity; non eseguiti in Unity (progetto aperto nell'editor di Peppe).

## Decisioni successive di Peppe (14 nov 2026, v0.6.0)
- **Chi cambia casella in risposta non combatte**, comunque sia stato spostato (scarto, tap o carta; attaccante o difensore): l'attacco colpisce il vuoto. RIC-ANNULLA diventa il default di P01; RIC-GEO resta come variante (preset P01d, che ha cambiato significato).
- **Abilità dei Guildmaster**: scelta tra «anche in risposta» (default) e «solo nel proprio turno» (preset P01g); sempre una volta per turno.
- **Passaggio del controllo disattivabile** dalla schermata di configurazione.
- **Avversario AI** per G2 (casella nella configurazione): è il giocatore automatico delle partite simulate, quindi gioca in modo semplice e in parte casuale; non si sposta in risposta e non fa Mulligan.
- **Striscia di fase** sopra la board: chi agisce, se nel proprio turno o in risposta, e a che cosa risponde.
- Verifica: `dotnet test` 40 su 40; script dell'interfaccia compilati contro le librerie di Unity; non eseguiti in Unity.

## Aggiunte richieste da Peppe (14 nov 2026, v0.6.1)
- Menu: righe delle regole a colori alterni.
- Variante **restare fermi vale come mossa** (parametro, preset P01h): l'unità si tappa, l'obbligo è soddisfatto, parte il combattimento. Default: no.
- Variante **attacco senza movimento** (parametro, preset P01i), l'unità si tappa e il tap vale come movimento del turno (decisione di Peppe del 15 nov: tap > movimento > attacco se c'è un bersaglio legale). Default: no.
- **Mazzi casuali all'avvio** (casella nella configurazione): estratti dal seed fra i mazzi validi per il preset; i due giocatori possono ricevere lo stesso mazzo.

## Linea di tiro: tre varianti (15 nov 2026, v0.6.3)
Parametro «Linea di tiro: come si traccia la retta»: da angolo ad angolo com'era (default, P01); da angolo ad angolo ma senza toccare gli angoli di muri, Torri o unità non amiche (P01j); una sola retta dal centro al centro (P01k). L'highground segue sempre la regola base. Nella variante dal centro, sfiorare soltanto l'angolo di un ostacolo resta valido (scelta mia, da confermare).

## Spostamento in risposta rivisto (15 nov 2026, v0.7.0)
Decisione di Peppe: il movimento è sempre subordinato al tap, salvo che una carta dica altro. Le varianti sono ora due: **SCH-SCARTO** (tap + scarto di una carta, default P01) e **SCH-TAP** (solo tap, preset P01m). Tolte SCH-CARTE (le carte che spostano valgono sempre, secondo il proprio testo), il parametro «l'unità deve essere stappata» e i preset P01c e P01f. I preset salvati con i valori tolti ricadono sul default.
Conseguenza da osservare al tavolo: col movimento obbligatorio il Guildmaster è tappato per tutto il turno avversario, quindi lo spostamento in risposta resta possibile solo con le carte, oppure se il movimento è facoltativo (P01e) o non è stato possibile.

## Torri attive (15 nov 2026, v0.8.0)
Cinque regole accendibili una per una, con i tre valori X nei parametri (cura, danno, punti vita): cura all'ingresso e a inizio turno accanto alla propria Torre, danno all'ingresso e a inizio turno accanto alla Torre nemica, Torre con punti vita che a 0 smette di avere effetto. Preset P01n con tutto acceso. Ipotesi H26 (domanda D39): come si colpisce la Torre.

## Finestra di fine turno (15 nov 2026, v0.9.0)
Decisione di Peppe: alla fine del turno di un giocatore si può sempre rispondere. «Termina il turno» ora dichiara la fine del turno e apre una finestra di risposta (istantanee, abilità dei Guildmaster); quando l'avversario passa, il turno finisce davvero. Con «passa da solo» la finestra si chiude da sé se non ci sono risposte possibili. Non è un parametro: vale sempre.

## Torri visibili sulla board (15 nov 2026, v0.9.1)
La vita della Torre è scritta sopra la Torre. Le caselle accanto a una Torre attiva sono colorate: verde tenue dove si viene curati, rosso tenue dove si prende danno; il colore diventa intenso quando sopra c'è un'unità che subisce quell'effetto. Se sono accese sia cura sia danno, la casella vuota è colorata dal punto di vista di chi è di turno (del giocatore umano contro l'AI).

## Attacco sulla pedina e carte giocate consultabili (15 nov 2026, v0.9.2)
L'etichetta sopra ogni pedina mostra anche l'Attacco attuale (con i modificatori). Un clic sulla miniatura di un oggetto assegnato, di una carta risorsa o su una carta in pila la mostra in grande al centro; un clic qualsiasi la chiude.

## Che cosa funziona
- Logica in `Assets/Guilds/Core` (assembly con `noEngineReferences`), compilata anche da `core/Guilds.Core.csproj`.
- Preset di base: **P01** (8x8), **P01-9x7**, **P01c** (schivata solo con carte), **P01d** (RIC-ANNULLA), **P01b** (S-A: dichiarato ma non avviabile, gli angoli delle mappe non sono liberi).
- Varianti selezionabili: mappa, posizione iniziale, schivata, ricontrollo, mazzo esaurito, contatto 8/4, simbolo giallo, tema, sette parametri numerici. Opzioni non implementate visibili ma indisponibili con il motivo: LOS-Z, OBB-B, modalità token diverse da «generico».
- 24 carte di test (4 Guildmaster, 4 risorse, 16 fra azioni, istantanee, oggetti), 4 mazzi da 25. Catalogo: `docs/CARDS.md`.
- Token dei Guildmaster dai 4 STL convertiti (`docs/logs/stl_conversion.md`).
- Ritocco mappe come varianti, blocco della mappa in partita, debug separato con marcatura «non valida per il confronto», salvataggio e caricamento di una situazione, resoconto in `reports/`.
- Partite simulate con seed fisso per P01, P01-9x7, P01c, P01d: `docs/logs/simulazione_*.md` (9–20 turni).

## Che cosa è provvisorio
- Tutte le ipotesi H1–H19 elencate nel preset (visibili in «Regole attive»). Le più pesanti: H1 (risorse), H8 (una sola finestra di risposta), H12/H13 (Torre e mostro come ostacoli inerti), H15 (che cosa soddisfa l'obbligo di movimento).
- Valori di tutte le carte.
- Abilità dei Guildmaster: forma della Croce (H21), costo e velocità (H22), bersagli di Senya (H23); veleno che non si toglie mai (D31).

## Che cosa manca
- **Verifica visiva dell'interfaccia.** Unity in modalità finestra, lanciato da riga di comando, si chiude dopo la licenza («Access token is unavailable»): serve aprire il progetto dal Hub. Il codice OnGUI compila ma non è mai stato eseguito: aspettati difetti di impaginazione al primo avvio.
- P02 «Alleati» (N-01..N-05), ruoli degli scacchi, mostri, Jungle, Torre attiva, edifici, altri modelli di risorse.
- LOS-Z e la parte di F1-38 sulle zone: la mappa non definisce i territori.
- Scelta manuale del percorso (H18) e del bersaglio del combattimento (inutile con un solo nemico).
- Editor di mazzi; M5.

## Decisioni di Peppe (14 nov 2026)
- Risorse di P01: carte risorsa nel mazzo (F2-09).
- Il ranged attacca solo nel proprio turno.
- Movimento obbligatorio: decade se l'unità bloccata è l'unica in grado di muoversi.
- N-01..N-05 (T) vanno nel preset P02.
- Mappe: due, da Excel; ritocco solo fra le partite e solo come variante; mappa bloccata in partita; debug solo per indagare bug.

## Decisioni tecniche (reversibili)
- Progetto Unity nella radice del repository; dati in `data/`, letti a runtime (anche modelli OBJ e immagini): temi e illustrazioni si aggiungono copiando file.
- Interfaccia in IMGUI generata da codice: nessun prefab, nessuna configurazione nell'Inspector.
- Testo e richieste delle carte ricavati dagli effetti eseguibili (`CardRules`), non scritti a mano.
- Stato di implementazione in `data/features/impl_status.json`; un preset non parte se contiene una feature non implementata, X o non decisa.
- Risposte: quando chi ha la priorità passa si risolve la cima della pila (H8); opzione dell'app «passa da solo se non ci sono risposte possibili», registrata come automazione.

## Scoperte da segnalare
- Il foglio **Note** dell'Excel descrive una disposizione diversa dalle celle dei fogli Mappa (muri e Torri speculari). Usate le celle. Torri in **A1 e H8**, non H1 e A8 (F4b-11).
- Il foglio **Zone** ha «da definire» per tutti i territori.
- Le quattro carte spoiler dei Guildmaster riportano 2 di attacco e 20 PV; Fiaran e Dalia in mischia, Senya e Nebros a distanza. Sulla carta di Nebros il nome sembra stampato «Cleartmind» (immagine piccola: da controllare), il file si chiama «Clearmind».
- Nelle sei partite simulate ha perso cinque volte un Guildmaster a distanza. Campione minuscolo e automa ingenuo: è un indizio da verificare al tavolo, non una conclusione.

## Prossimi passi
1. Peppe apre il progetto dal Hub e prova una partita; mi riporta i difetti dell'interfaccia.
2. Risposte alle domande in `docs/OPEN_QUESTIONS.md`.
3. P02 «Alleati», poi M5.
