# Domande aperte — Guilds prototipo

## Da chiedere ora (max 3)
- **D21 — Doppia sconfitta.** Se in uno scambio in mischia muoiono entrambi i Guildmaster, chi vince? Oggi la partita finisce senza vincitore.
- **D22 — Mappa: celle o Note?** Nell'Excel le celle dei fogli Mappa e il foglio Note descrivono disposizioni diverse (speculari; Torri in A1/H8 nelle celle, H1/A8 nelle Note e in F4b-11). Ho usato le celle: confermi?
- **D23 — Territori.** Il foglio Zone ha «da definire» ovunque. Quali caselle sono la metà di G1, la metà di G2 e la Jungle? Senza questo non si possono provare LOS-Z (F4C-03) né il divieto di F1-38.

## In coda
- **D37 — Che cosa conta come attacco (H24).** Oggi: un movimento nemico che porterebbe al combattimento contro quell'unità, oppure una carta o un'abilità nemica che le farebbe danni. Corretto?
- **D31 — Veleno.** Come si toglie un segnalino veleno? Oggi mai: un'unità avvelenata perde 1 a ogni suo turno fino a fine partita.
- **D33 — Croce di Fiaran.** Casella scelta + 4 adiacenti ortogonali, colpisce anche le proprie unità: corretto? La casella centrale può essere vuota (oggi sì)?
- **D34 — Abilità.** Costano 1 risorsa della gilda, non tappano il Guildmaster, si usano anche in risposta e una volta per turno anche in quello avversario: corretto?
- **D35 — Senya.** «Any enemy unit»: il nemico deve essere in linea di vista (oggi sì)?
- **D29 — Personaggi tappati.** Nel motore ogni unità si stappa all'inizio del turno del suo proprietario e resta tappata durante il turno avversario (quindi non può schivare col tap: è la conseguenza annunciata in PRESET_01). Tu hai visto un Guildmaster che non poteva muovere **nel proprio turno**, oppure che non poteva spostarsi in risposta nel turno avversario? Nel secondo caso è la regola, e la domanda diventa se va cambiata.
- **D16 — Carte risorsa (F2-09).** Una per turno, resta in gioco e produce 1 risorsa dal turno successivo (H1)? La prima carta a pagamento si gioca al turno 2.
- **D24 — Torre e mostro inerti.** In P01 bloccano il movimento; la Torre blocca anche la linea di vista (H12), il mostro no (H13). Va bene?
- **D25 — Obbligo di movimento e carte.** Essere spostati da una carta (Blink Step, Smoke Step) soddisfa F3-03? Oggi no (H15): conta solo il movimento col tap.
- **D26 — Risposte.** Oggi chi ha agito non riceve la priorità sulla propria azione: risponde l'avversario e, quando passa, si risolve (H8). Serve che anche chi ha agito possa aggiungere una risposta dopo il passo dell'avversario?
- **D28 — Contatto in mischia.** Anche in diagonale (H11)?
- **D17 — Assalto (N-03).** Vale anche contro un Guildmaster? Se muoiono entrambe la casella resta vuota? Chi vince occupa la casella?
- **D18 — Assalto e vita ripristinata (N-01).** Solo insieme, o anche con i danni persistenti?
- **D4 — F3-03 con più unità**: basta muoverne una (OBB-A)? «Bloccata» include Root e «già tappata»? (oggi sì)
- **D19 — Assalto, casi limite**: può dichiararlo un ranged? E con forza 0?
- **D20 — Nome**: «Assalto» va bene?
- **D5 — F1-27 senza segnalino attacco**: oggi il limite è un indicatore «ha attaccato» azzerato a inizio turno. Swift/Taunt come si riformulano?
- **D6 — F4b-02**: confermi «X perché ridondante, il comportamento resta»?
- **D7 — F2-01, nota**: «con nomi diversi» intende «con nomi uguali»?
- **D8 — Gilda gialla**: Lair è la sua sede? Si chiama Rogues?
- **D9 — Verde**: Hunters o Rangers?
- **D10 — Guildmaster**: Daalia (file STL) o Dalia (carta)? I GM del Core Set sono alternative o sostituiti?
- **D11 — U-06 «angoli liberi»**: quali? Sulle due mappe attuali gli angoli sono muri, Torri o fuori mappa, quindi S-A non è avviabile.
- **D12 — F4C-05 contro F4b-15**: la Torre si conquista o si distrugge?
- **D13 — F2-22**: si può finire il movimento su un muro?
- **D14 — CS-25**: resta L anche se Guard (BL-35) è vuota?
- **D15 — Mazzo esaurito**: si perde quando si deve pescare da un mazzo vuoto (H9, attuale) o quando si pesca l'ultima carta?

- **D39 — Come si colpisce la Torre (H26).** Oggi: col combattimento automatico, se non c'è un'unità nemica da colpire e la Torre è a contatto o in linea di tiro; danno pari all'Attacco; la Torre non restituisce. A 0 smette sia di curare sia di fare danno. Passare da una casella della Torre a un'altra conta come entrare. Corretto?

## Risolte
- **D36** → il movimento è sempre subordinato al tap, salvo che una carta dica altro. Spostamento in risposta: SCH-SCARTO (tap + scarto) oppure SCH-TAP (solo tap). SCH-CARTE tolta: le carte che spostano valgono sempre, secondo il proprio testo. 15 nov 2026.
- **D31 (veleno)** → due varianti da provare: i segnalini restano (P01) oppure calano di uno ogni volta che fanno danno, all'inizio del turno dell'unità avvelenata (P01l). 15 nov 2026.
- **D38** → l'attacco senza cambiare casella avviene comunque con un tap: il tap è il movimento. Ordine: tap > movimento > attacco se c'è un bersaglio legale. 15 nov 2026.
- Un'unità che cambia casella in risposta (in qualsiasi modo) non combatte: RIC-ANNULLA è il default. 14 nov 2026.
- Abilità dei Guildmaster: selezionabile «anche in risposta» o «solo nel proprio turno»; sempre una volta per turno. 14 nov 2026.
- Spostamento in risposta: solo scartando una carta, solo in risposta a un attacco (N-08). 14 nov 2026.
- Le carte risorsa entrano stappate e producono subito. 14 nov 2026.
- **D32** → le carte risorsa si tappano per pagare e si stappano all'inizio del proprio turno. Confermato il 14 nov 2026.
- **D30** → Mulligan: scegli le carte da rimettere nel mazzo, ne peschi una in meno. Confermato il 14 nov 2026. (Le carte vanno in fondo al mazzo, senza rimescolare: dettaglio mio.)
- **D29** → le istantanee si giocano anche col Guildmaster tappato; solo lo spostamento in risposta richiede l'unità stappata. 14 nov 2026.
- **D27** → abilità dei Guildmaster incluse, una volta per turno. 14 nov 2026.
- Movimento obbligatorio: deve poter essere disattivato (parametro). 14 nov 2026.
- **D1** → risorse come carte nel mazzo (F2-09). 14 nov 2026.
- **D2** → il ranged attacca solo nel proprio turno. 14 nov 2026.
- **D3** → l'obbligo decade se l'unità bloccata è l'unica in grado di muoversi. 14 nov 2026.

## Bloccanti esterni
- Apertura del progetto dal Unity Hub per la prima prova a schermo (l'avvio in finestra da riga di comando si ferma sulla licenza).
