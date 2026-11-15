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

## Evoluzioni del piano, tutte come varianti (15 nov 2026, v0.10.0)
Il punto di ritorno alla fase precedente è il tag git `v0.9.2`. P01 e i preset P01x non cambiano regole: tutte le novità sono parametri spenti di default.

| Evoluzione | Parametri | Preset | Stato |
|---|---|---|---|
| Alleati sulla board: carte Alleato, evocazione nel proprio territorio o accanto al Guildmaster, ingresso tappato salvo Swift, limite di miniature, morte → scarti, scelta del bersaglio con più nemici, OBB-A / OBB-B | Unità sulla board, Dove si evocano, Alleati appena evocati, Miniature massime, Se obbligatorio con più unità | P02, P02c | implementato; territori per ipotesi H27 |
| Vita ripristinata (N-01) e Assalto (N-02..04) | Vita delle unità non Guildmaster, Assalto | P02a | implementato; casi limite per ipotesi H28 |
| Doppio profilo d'attacco (N-05, F3-10) | Profilo d'attacco | P02b | implementato; valori ipotizzati (H10) |
| Scacchi: Guildmaster come il Re; ogni unità col suo pezzo, uno per tipo | Come si muovono le pedine, Guildmaster muove come | P03, P03a | implementato; Pedone per ipotesi H29 |
| Mostri con punti vita, beneficio, vittoria coi mostri | Mostri, punti vita, danno, beneficio, vittoria con N | P04 | implementato; tutto per ipotesi H31 |
| Vittoria distruggendo la Torre (F4b-15) | Vittoria distruggendo la Torre | P01o | implementato |
| Risorse fuori dal mazzo, fisse o crescenti (F2-10); risorsa base del Guildmaster (BL-21) | Modello delle risorse, X, Risorsa base | P05, P05a | implementato |
| Edifici distruttibili (F3-12/13, F4b-18) | Edifici, punti Struttura | P06 | implementato in forma minima (H30) |
| Linea di vista a zone (F4C-03) | Linea di vista = LOS-Z | P07 | implementato; zone per ipotesi H27 |
| Editor di mazzi | scheda «Editor di mazzi» nella configurazione | — | implementato; salva in `data/decks/user` |

Carte nuove: 8 alleati di test (2 per gilda) e 4 mazzi «con alleati». Selezionando un preset con gli alleati i mazzi passano da soli a quelli con alleati, con un avviso.

Verifica: `dotnet test` 72 su 72 (23 test nuovi, di cui 12 partite simulate sui preset nuovi). Script dell'interfaccia compilati contro le librerie di Unity; **non eseguiti in Unity**. Nelle partite simulate l'automa evoca alleati e li perde, ma non ha mai dichiarato un Assalto né attaccato un mostro o un edificio: quelle regole sono coperte solo dai test mirati.

Non fatto: M5 (Blender), per scelta di Peppe. Scelta manuale del percorso (nessuna regola dipende dal percorso). Token per ruolo o per gilda. Jungle (le mappe non hanno caselle Jungle). Assegnazione libera dei pezzi degli scacchi (nota di F4C-01). Parole chiave Armor, Flying, Taunt, Unique. Punti azione e moneta.

## Opzioni aggiunte su richiesta di Peppe (15 nov 2026, v0.10.1)
- **Dove si evocano gli alleati**: anche «accanto alla propria Torre».
- **Alleati appena evocati**: anche «pronti ma con debolezza da evocazione» (non si possono tappare nel turno d'ingresso, salvo Swift; restano stappati nel turno avversario).
- **Guildmaster con gli scacchi**: anche «3 passi» (muove a passi come senza scacchi).
- **Come si muovono le pedine**: anche «pezzo scelto all'ingresso» fra 1 Cavallo, 1 Alfiere, 1 Re, 1 Regina, 1 Torre, 2 Pedoni per giocatore (preset P03b). Deciso da Peppe: se il Guildmaster muove come il Re, il Re è suo; altrimenti è fra i pezzi disponibili.
- **Evocare un alleato con lo stesso nome**: crea una nuova pedina (default) · crea la nuova e rimuove la vecchia · sostituisce la vecchia. Nella sostituzione la carta si gioca sulla pedina già in campo, che resta nella sua casella (lettura mia, da confermare): vita piena, veleno tolto, effetti delle carte tolti a scelta, pezzo cambiabile. Gli alleati oggi non portano oggetti, quindi «togliere oggetti» riguarda solo gli effetti delle carte.
- Menu: le note lunghe sotto il titolo di una regola non finiscono più dietro la voce successiva.
- Verifica: `dotnet test` 75 su 75; interfaccia compilata, non eseguita in Unity.

## Menu per temi, editor su tutti i mazzi, mazzi ottimizzati (15 nov 2026, v0.11.0)
- Le regole della configurazione sono raggruppate in 11 temi con una fascia di titolo (Preparazione, Movimento, Combattimento, Risposte, Linea di vista, Alleati, Risorse ed edifici, Torri, Mostri, Effetti, Presentazione).
- Editor di mazzi: elenca tutti i mazzi; ognuno si può assegnare a G1 o G2 oppure modificare in copia; dal mazzo modificato si salva e si assegna a G1 o G2.
- Casella «Mazzi ottimizzati per il preset»: all'avvio ogni giocatore riceve un mazzo costruito per il preset a partire dal suo Guildmaster, con gli alleati se il preset li ha e senza altrimenti. Regola (mia, modificabile): tutte le carte compatibili della gilda in parti uguali fino al minimo del mazzo; risorse = 36% del mazzo (9 su 25), mai meno di 9 ogni 16 altre carte.

## Mostri di tre tipi e mazzo dei mostri (15 nov 2026, v0.12.0)
Richiesta di Peppe. Coi mostri attivi i mostri sono di tre tipi (cura, attacco, pesca), ognuno con punti vita, danno e beneficio modificabili; escono da un mazzo dei mostri fuori dal gioco (default 4 + 3 + 3 = 10 carte, numeri modificabili), mescolato a inizio partita, uno per casella M. Opzioni nuove: «Vita dei mostri» (persistente / ripristinata) e «Quando un mostro è sconfitto» (resta / esce il prossimo). Tolti i parametri unici «punti vita», «danno» e «beneficio» dei mostri. Quando e dove esce un mostro nuovo è una mia ipotesi (D48). Verifica: test del motore; interfaccia compilata, **non eseguita in Unity**.

## Preset di base eliminabili (15 nov 2026, v0.12.1)
«Elimina» funziona anche sui preset di base: spariscono dall'elenco (l'elenco è in `data/presets/user/hidden.txt`), i file restano perché il generatore dei contenuti e i test li usano. «Ripristina i preset di base» li fa riapparire tutti. L'ultimo preset rimasto non si può eliminare.

## Scelta fra tutti i bersagli validi e mostri da non attaccare (15 nov 2026, v0.12.2)
Richiesta di Peppe. Il pulsante del bersaglio ora elenca unità nemiche, Torre e mostri: se il bersaglio scelto è a portata viene attaccato quello, anche al posto di un'unità nemica (prima Torre e mostri si colpivano solo in mancanza d'altro: H26, H31). Senza scelta resta l'ordine unità, Torre, mostro. Accanto a ogni mostro vivo c'è la spunta «non attaccare», una per giocatore: quel mostro esce dai suoi bersagli. Interfaccia compilata, **non eseguita in Unity**.

## Assalto senza Guildmaster contro Guildmaster (15 nov 2026, v0.12.3)
Nuova scelta di «Assalto»: attivo anche contro i Guildmaster, ma non Guildmaster contro Guildmaster (un alleato può assaltare un Guildmaster e viceversa).

## Otto alleati nuovi e carte in gioco in 3D (15 nov 2026, v0.13.0)
Richiesta di Peppe. Due alleati di test in più per gilda (un difensore «pedone» e un attaccante «regina» ciascuna; valori proposti, nessuna fonte li fissa): 40 carte in tutto. I mazzi «con alleati» hanno ora 9 risorse, 4 carte ×2 e 4 alleati ×2. Le carte risorsa e gli oggetti in gioco compaiono come lastre 3D accanto alla board, dal lato della Torre di chi li ha giocati: tappate se ruotate, clic per vederle in grande. I pannelli 2D del tavolo restano. Parte 3D compilata, **non eseguita in Unity**: posizione, orientamento e inquadratura sono da verificare a schermo.

## Che cosa funziona
- Logica in `Assets/Guilds/Core` (assembly con `noEngineReferences`), compilata anche da `core/Guilds.Core.csproj`.
- Preset di base: la famiglia **P01** (duello dei Guildmaster e le sue varianti) e i preset delle evoluzioni **P02–P07** elencati sopra.
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
- Jungle con regole proprie; il divieto di F1-38 fra le zone di evocazione; edifici come supporto per altre carte.
- Scelta manuale del percorso (H18).
- M5.

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
3. Prova a schermo dei preset P02–P07; poi M5.
