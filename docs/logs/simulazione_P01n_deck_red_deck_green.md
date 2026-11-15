# Resoconto partita — Guilds prototipo

- Versione prototipo: 0.9.2 (M4)
- Preset: P01n — Duello — Torri attive (clone di P01) — TEST PARZIALE
- Mappa: mappa_8x8 (originale)
- Simbolo gilda gialla: stivale
- Token delle altre unità: generico (in questo preset non ci sono altre unità)
- Tema visivo: semplice
- Seed: 20261114
- Inizio: 2026-11-14 10:00:00 — durata: 20.0 minuti
- Turni: 9 — primo giocatore: G2
- Vincitore: G1 — Senya Windseeker è stato sconfitto (F1-54)
- Interventi di debug: 0

## Parametri del preset

- Posizione iniziale (conflitto C1): S-B
- Linea di vista (conflitto C2): LOS-G
- Linea di tiro: come si traccia la retta: angoli
- Segnalini veleno: resta
- Movimento obbligatorio ogni turno: si
- Restare fermi vale come mossa?: no
- Attacco senza movimento: no
- Se obbligatorio, con più unità: OBB-A
- Carta risorsa appena giocata: pronta
- Abilità proprie dei Guildmaster: si
- Come ci si sposta in risposta: SCH-SCARTO
- Quando ci si può spostare in risposta: si
- Combattimento automatico se un'unità cambia casella in risposta: RIC-ANNULLA
- Mazzo esaurito: perde
- Mulligan iniziale: iniziale
- Contatto per il corpo a corpo (ipotesi H11): 8
- Token delle altre unità: generico
- Simbolo gilda gialla (presentazione): stivale
- Torre propria: cura chi entra: si
- X = cura all'ingresso: 1
- Torre propria: cura a inizio turno: si
- X = cura a inizio turno: 1
- Torre nemica: danno a chi entra: si
- X = danno all'ingresso: 1
- Torre nemica: danno a inizio turno: si
- X = danno a inizio turno: 1
- Torre con punti vita: si
- X = punti vita della Torre: 5
- Mano iniziale: 5
- Limite di mano a fine turno: 7
- Carte pescate a turno: 1
- Il primo giocatore salta la prima pesca (1 = sì): 1
- Carte risorsa giocabili a turno (H1): 1
- Movimento massimo base (minimo sempre 1, F3-04): 3
- Carte minime nel mazzo: 25
- Copie massime per carta (risorse escluse, H14): 4

## Mazzi e carte (pescate / giocate)

### G1: Mazzo rosso di prova (Artificers) (deck_red v1) — Fiaran Rushblood gm_fiaran@v1, Salute finale 8/20

Abilità del Guildmaster usata 5 volte.

| Carta | Versione | Nel mazzo | Pescate | Giocate |
|---|---|---|---|---|
| Essenza rossa | res_red@v1 | 9 | 4 | 4 |
| Thermal Surge (test) | r_thermal_surge@v1 | 4 | 2 | 1 |
| Furia della Fucina (test) | r_fury@v1 | 4 | 1 | 1 |
| Rincorsa (test) | r_rush@v1 | 4 | 2 | 2 |
| Flame Gauntlet (test) | r_flame_gauntlet@v1 | 4 | 0 | 0 |

### G2: Mazzo verde di prova (Hunters) (deck_green v1) — Senya Windseeker gm_senya@v1, Salute finale 0/20

Abilità del Guildmaster usata 3 volte.

| Carta | Versione | Nel mazzo | Pescate | Giocate |
|---|---|---|---|---|
| Essenza verde | res_green@v1 | 9 | 3 | 3 |
| Field Dressing (test) | g_field_dressing@v1 | 4 | 1 | 1 |
| Regrowth (test) | g_regrowth@v1 | 4 | 2 | 2 |
| Hunter's Net (test) | g_hunters_net@v1 | 4 | 1 | 1 |
| Hunter's Bow (test) | g_hunters_bow@v1 | 4 | 2 | 1 |

## Automazioni e tracciamento (da valutare per il tavolo)

- setup: 1 passaggi automatici
- turn: 9 passaggi automatici
- draw: 9 passaggi automatici
- combat: 15 passaggi automatici
- rule: 1 passaggi automatici
- end: 1 passaggi automatici

## Interventi di debug

Nessuno.

## Note del tester

nota di prova

## Eventi

- T0 [auto] Partita iniziata. Preset P01n, mappa mappa_8x8, seed 20261114. Primo giocatore sorteggiato: G2.
- T0 G2 tiene la mano.
- T0 G1 tiene la mano.
- T0 Senya Windseeker parte in G8.
- T0 Fiaran Rushblood parte in B2.
- T1 [auto] — Turno 1: G2. Le sue unità e le sue 0 carte risorsa si stappano.
- T1 [auto] Il primo giocatore salta la prima pesca (F1-52).
- T1 G2 gioca Essenza verde (entra pronta).
- T1 Senya Windseeker si tappa e muove: G8 → F7 → F6.
- T1 G1 passa.
- T1 [auto] Nessun combattimento: nessun nemico in linea di vista di Senya Windseeker.
- T1 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T1 G1 passa.
- T1 G2 termina il turno. Risorse non spese perse (F1-45).
- T2 [auto] — Turno 2: G1. Le sue unità e le sue 0 carte risorsa si stappano.
- T2 [auto] G1 pesca una carta.
- T2 G1 gioca Essenza rossa (entra pronta).
- T2 G1 gioca Rincorsa (test) su Fiaran Rushblood (costo [Bomba]).
- T2 G2 passa.
- T2 Rincorsa (test): Fiaran Rushblood +2 move fino alla fine del turno 2.
- T2 Fiaran Rushblood si tappa e muove: B2 → A3 → B4 → C5 → D6 → E6.
- T2 G2 passa.
- T2 [auto] Combattimento automatico: Fiaran Rushblood (melee) infligge 2 a Senya Windseeker (Salute 18/20).
- T2 [auto] Senya Windseeker non restituisce danni (difensore a distanza, F1-33).
- T2 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T2 Senya Windseeker usa la sua abilità su Fiaran Rushblood (costo [Arco]).
- T2 G1 passa.
- T2 Freccia di Senya: 1 danni a Fiaran Rushblood (Salute 19/20).
- T2 Freccia di Senya: cura 1 a Senya Windseeker (Salute 19/20).
- T2 G2 passa.
- T2 G1 termina il turno. Risorse non spese perse (F1-45).
- T3 [auto] — Turno 3: G2. Le sue unità e le sue 1 carte risorsa si stappano.
- T3 [auto] G2 pesca una carta.
- T3 G2 gioca Essenza verde (entra pronta).
- T3 Senya Windseeker usa la sua abilità su Fiaran Rushblood (costo [Arco]).
- T3 G1 passa.
- T3 Freccia di Senya: 1 danni a Fiaran Rushblood (Salute 18/20).
- T3 Freccia di Senya: cura 1 a Senya Windseeker (Salute 20/20).
- T3 G2 gioca Regrowth (test) su Senya Windseeker (costo [Arco]).
- T3 G1 passa.
- T3 Regrowth (test): cura 2 a Senya Windseeker (Salute 20/20).
- T3 Senya Windseeker si tappa e muove: F6 → E7 → D6.
- T3 G1 passa.
- T3 [auto] Combattimento automatico: Senya Windseeker (ranged) infligge 2 a Fiaran Rushblood (Salute 16/20).
- T3 [auto] Fiaran Rushblood restituisce 2 a Senya Windseeker (Salute 18/20).
- T3 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T3 G1 passa.
- T3 G2 termina il turno. Risorse non spese perse (F1-45).
- T4 [auto] — Turno 4: G1. Le sue unità e le sue 1 carte risorsa si stappano.
- T4 [auto] G1 pesca una carta.
- T4 G1 gioca Furia della Fucina (test) su Fiaran Rushblood (costo [Bomba]).
- T4 G2 passa.
- T4 Furia della Fucina (test): Fiaran Rushblood +2 attack fino alla fine del turno 4.
- T4 G1 gioca Essenza rossa (entra pronta).
- T4 Fiaran Rushblood usa la sua abilità verso D7 (costo [Bomba]).
- T4 G2 passa.
- T4 Bomba di Fiaran: Croce su D7, 2 danni a Senya Windseeker (Salute 16/20).
- T4 Fiaran Rushblood si tappa e muove: E6 → D7 → C7.
- T4 G2 passa.
- T4 [auto] Combattimento automatico: Fiaran Rushblood (melee) infligge 4 a Senya Windseeker (Salute 12/20).
- T4 [auto] Senya Windseeker non restituisce danni (difensore a distanza, F1-33).
- T4 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T4 G2 passa.
- T4 G1 termina il turno. Risorse non spese perse (F1-45).
- T5 [auto] — Turno 5: G2. Le sue unità e le sue 2 carte risorsa si stappano.
- T5 [auto] G2 pesca una carta.
- T5 G2 gioca Field Dressing (test) su Senya Windseeker (costo 1[Arco]).
- T5 G1 passa.
- T5 Field Dressing (test): cura 4 a Senya Windseeker (Salute 16/20).
- T5 G2 gioca Essenza verde (entra pronta).
- T5 Senya Windseeker si tappa e muove: D6 → C5 → B6.
- T5 G1 passa.
- T5 [auto] Combattimento automatico: Senya Windseeker (ranged) infligge 2 a Fiaran Rushblood (Salute 14/20).
- T5 [auto] Fiaran Rushblood restituisce 2 a Senya Windseeker (Salute 14/20).
- T5 Senya Windseeker usa la sua abilità su Fiaran Rushblood (costo [Arco]).
- T5 G1 passa.
- T5 Freccia di Senya: 1 danni a Fiaran Rushblood (Salute 13/20).
- T5 Freccia di Senya: cura 1 a Senya Windseeker (Salute 15/20).
- T5 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T5 G1 passa.
- T5 G2 termina il turno. Risorse non spese perse (F1-45).
- T6 [auto] — Turno 6: G1. Le sue unità e le sue 2 carte risorsa si stappano.
- T6 [auto] G1 pesca una carta.
- T6 G1 gioca Rincorsa (test) su Fiaran Rushblood (costo [Bomba]).
- T6 G2 passa.
- T6 Rincorsa (test): Fiaran Rushblood +2 move fino alla fine del turno 6.
- T6 G1 gioca Essenza rossa (entra pronta).
- T6 Fiaran Rushblood usa la sua abilità verso B6 (costo [Bomba]).
- T6 G2 passa.
- T6 Bomba di Fiaran: Croce su B6, 2 danni a Senya Windseeker (Salute 13/20).
- T6 Fiaran Rushblood si tappa e muove: C7 → C6 → B5 → A5.
- T6 G2 passa.
- T6 [auto] Combattimento automatico: Fiaran Rushblood (melee) infligge 2 a Senya Windseeker (Salute 11/20).
- T6 [auto] Senya Windseeker non restituisce danni (difensore a distanza, F1-33).
- T6 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T6 G2 passa.
- T6 G1 termina il turno. Risorse non spese perse (F1-45).
- T7 [auto] — Turno 7: G2. Le sue unità e le sue 3 carte risorsa si stappano.
- T7 [auto] G2 pesca una carta.
- T7 G2 gioca Hunter's Net (test) su Fiaran Rushblood (costo 1[Arco]).
- T7 Fiaran Rushblood usa la sua abilità verso C6 (costo [Bomba]).
- T7 G2 gioca Regrowth (test) su Senya Windseeker (costo [Arco]).
- T7 G1 passa.
- T7 Regrowth (test): cura 2 a Senya Windseeker (Salute 13/20).
- T7 G2 passa.
- T7 Bomba di Fiaran: Croce su C6, 2 danni a Senya Windseeker (Salute 11/20).
- T7 G1 passa.
- T7 Hunter's Net (test): Fiaran Rushblood non può muoversi fino alla fine del turno 8.
- T7 Senya Windseeker si tappa e muove: B6 → B5 → A4.
- T7 G1 passa.
- T7 [auto] Combattimento automatico: Senya Windseeker (ranged) infligge 2 a Fiaran Rushblood (Salute 11/20).
- T7 [auto] Fiaran Rushblood restituisce 2 a Senya Windseeker (Salute 9/20).
- T7 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T7 G1 passa.
- T7 G2 termina il turno. Risorse non spese perse (F1-45).
- T8 [auto] — Turno 8: G1. Le sue unità e le sue 3 carte risorsa si stappano.
- T8 [auto] G1 pesca una carta.
- T8 Fiaran Rushblood usa la sua abilità verso B4 (costo [Bomba]).
- T8 G2 passa.
- T8 Bomba di Fiaran: Croce su B4, 2 danni a Senya Windseeker (Salute 7/20).
- T8 G1 gioca Thermal Surge (test) su Senya Windseeker (costo 1[Bomba]).
- T8 G2 passa.
- T8 Thermal Surge (test): 3 danni a Senya Windseeker (Salute 4/20).
- T8 G1 gioca Essenza rossa (entra pronta).
- T8 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T8 G2 passa.
- T8 [auto] Fiaran Rushblood non poteva muoversi: l'obbligo di movimento decade (decisione del 14 nov).
- T8 G1 termina il turno. Risorse non spese perse (F1-45).
- T9 [auto] — Turno 9: G2. Le sue unità e le sue 3 carte risorsa si stappano.
- T9 [auto] G2 pesca una carta.
- T9 G2 gioca Hunter's Bow (test) (costo 1[Arco]).
- T9 G1 passa.
- T9 Hunter's Bow (test) è assegnato a Senya Windseeker.
- T9 Senya Windseeker si tappa e muove: A4 → B4.
- T9 Fiaran Rushblood usa la sua abilità verso C4 (costo [Bomba]).
- T9 G2 passa.
- T9 Bomba di Fiaran: Croce su C4, 2 danni a Senya Windseeker (Salute 2/20).
- T9 G1 passa.
- T9 [auto] Combattimento automatico: Senya Windseeker (ranged) infligge 3 a Fiaran Rushblood (Salute 8/20).
- T9 [auto] Fiaran Rushblood restituisce 2 a Senya Windseeker (Salute 0/20).
- T9 [auto] Fine partita: vince G1. Senya Windseeker è stato sconfitto (F1-54).

## Configurazione completa (copia)

```json
{
  "id": "P01n",
  "name": "Duello — Torri attive",
  "basedOn": "P01",
  "description": "Come P01, con tutte le regole delle Torri accese: cura e danno all'ingresso e a inizio turno, Torre con punti vita. I valori X si cambiano nei parametri.",
  "featureSet": "P01",
  "map": "mappa_8x8",
  "theme": "semplice",
  "partial": true,
  "builtIn": true,
  "choices": {
    "towerHealEnter": "si",
    "towerHealStart": "si",
    "towerDmgEnter": "si",
    "towerDmgStart": "si",
    "towerHp": "si"
  },
  "numbers": {},
  "hypotheses": [
    "H1 Carte risorsa: una per turno; restano in gioco sul tavolo; si TAPPANO per pagare 1 risorsa del loro colore e si stappano all'inizio del proprio turno. Entrano stappate e producono subito (deciso il 14 nov; parametro per provarle tappate). Quelle rimaste pronte servono per istantanee e abilità nel turno avversario. 9 per mazzo.",
    "H2 Le carte risorsa non sono unità, non passano dalla pila e non possono essere attaccate.",
    "H3 20 PV a tutti e quattro i Guildmaster (le quattro carte spoiler riportano 20).",
    "H4 Lo spostamento in risposta (con scarto o con tap) usa il movimento normale dell'unità. Con lo scarto l'unità non si tappa e non conta come movimento obbligatorio.",
    "H26 Torre con punti vita: si colpisce col combattimento automatico (danno = Attacco) se non c'è un'unità nemica da colpire ed è a contatto o in linea di tiro; non restituisce danni. Per le caselle della Torre conta la casella d'arrivo, non quelle attraversate.",
    "H24 «Attacco» = un movimento nemico che porterebbe al combattimento automatico contro quell'unità, oppure una carta o un'abilità nemica che le infliggerebbe danni.",
    "H6 Il difensore in mischia restituisce danno solo se adiacente all'attaccante.",
    "H8 Una finestra di risposta dopo ogni movimento e ogni carta: risponde l'avversario; quando chi ha la priorità passa, si risolve la cima della pila. Uno spostamento in risposta non apre un secondo combattimento.",
    "H9 Si perde nel momento in cui si deve pescare da un mazzo vuoto.",
    "H11 Contatto in mischia anche in diagonale (parametro).",
    "H12 La Torre inerte blocca movimento e linea di vista come un muro.",
    "H13 La casella mostro è highground con un ostacolo inerte (mostri rinviati).",
    "H14 Le carte risorsa non contano per il limite di copie.",
    "H15 L'obbligo di movimento è soddisfatto solo dal movimento col tap; decade se il Guildmaster è tappato, sotto Root o senza caselle raggiungibili.",
    "H16 Un mazzo contiene solo carte della gilda del suo Guildmaster.",
    "H17 Root e i malus «fino al prossimo turno» durano fino alla fine del primo turno del proprietario del bersaglio che termina dopo la risoluzione.",
    "H18 Il percorso del movimento è il più breve, calcolato dal programma e scritto nel registro; nessuna regola di P01 dipende dal percorso.",
    "H19 Entrare, uscire o muoversi sull'highground non costa nulla e non dà bonus.",
    "H20 Mulligan iniziale (chiesto il 14 nov): una volta per giocatore, prima di scegliere la partenza; le carte scelte vanno in fondo al mazzo e se ne pescano altrettante meno una (procedura di BL-48, da confermare).",
    "H21 Croce (abilità di Fiaran): la casella scelta più le 4 adiacenti in orizzontale e verticale; colpisce tutte le unità, anche le proprie. La forma non è scritta nelle fonti lette.",
    "H22 Abilità dei Guildmaster: costano 1 risorsa della gilda, non tappano il Guildmaster, una volta per turno (anche in quello avversario), si usano come un'istantanea (esempio di Nebros nel documento fase 2) e passano dalla pila.",
    "H23 Senya: il nemico deve essere in linea di vista; l'unità amica curata è il suo Guildmaster (non ci sono altre unità).",
    "ND Veleno (F1-65): le fonti non dicono come si toglie un segnalino: per ora non si toglie mai (D31).",
    "ND Entrambi i Guildmaster sconfitti nello stesso scambio: esito non definito (D21); la partita finisce senza vincitore."
  ],
  "limits": [
    "Test parziale: sottoinsieme delle feature dell'inventario. Non è il regolamento definitivo.",
    "Nessun alleato, evocazione, mostro, edificio; Torre e mostri sono ostacoli inerti; Jungle senza regole.",
    "Zone personali non definite nella mappa (foglio Zone: «da definire»): niente LOS-Z, niente parte di F1-38 sulle zone di evocazione.",
    "Il foglio Note dell'Excel descrive una disposizione diversa dalle celle dei fogli Mappa: usate le celle (Torri in A1 e H8, non H1 e A8 come in F4b-11)."
  ],
  "deferred": [
    "Alleati, evocazione, token (F1-10/17/29/30, F2-01, F4C-01/02)",
    "Ruoli degli scacchi (F2-13/14, F4B-04)",
    "Mostri, Jungle, vittoria coi mostri (F3-06/07/08, F4b-07)",
    "Torre con PV, danno, vittoria (F4b-12/14/15, F4C-05/06)",
    "Edifici (F2-08, F3-12/13, F4b-18)",
    "Punti azione e moneta (F4A-04..15, F2-16)",
    "Parole chiave: Armor, Flying, Swift, Taunt, Unique",
    "Aree d'effetto diverse dalla Croce (F1-39/40)",
    "Magie continuative, esilio, ricerca nel mazzo, dadi",
    "P02 Alleati: N-01..N-05 (vita ripristinata, Assalto, doppio profilo)"
  ]
}
```
