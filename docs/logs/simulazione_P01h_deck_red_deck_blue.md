# Resoconto partita — Guilds prototipo

- Versione prototipo: 0.9.2 (M4)
- Preset: P01h — Duello — restare fermi vale come mossa (clone di P01) — TEST PARZIALE
- Mappa: mappa_8x8 (originale)
- Simbolo gilda gialla: stivale
- Token delle altre unità: generico (in questo preset non ci sono altre unità)
- Tema visivo: semplice
- Seed: 20261114
- Inizio: 2026-11-14 10:00:00 — durata: 20.0 minuti
- Turni: 7 — primo giocatore: G2
- Vincitore: G1 — Nebros Clearmind è stato sconfitto (F1-54)
- Interventi di debug: 0

## Parametri del preset

- Posizione iniziale (conflitto C1): S-B
- Linea di vista (conflitto C2): LOS-G
- Linea di tiro: come si traccia la retta: angoli
- Segnalini veleno: resta
- Movimento obbligatorio ogni turno: si
- Restare fermi vale come mossa?: si
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
- Torre propria: cura chi entra: no
- X = cura all'ingresso: 1
- Torre propria: cura a inizio turno: no
- X = cura a inizio turno: 1
- Torre nemica: danno a chi entra: no
- X = danno all'ingresso: 1
- Torre nemica: danno a inizio turno: no
- X = danno a inizio turno: 1
- Torre con punti vita: no
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

### G1: Mazzo rosso di prova (Artificers) (deck_red v1) — Fiaran Rushblood gm_fiaran@v1, Salute finale 12/20

Abilità del Guildmaster usata 3 volte.

| Carta | Versione | Nel mazzo | Pescate | Giocate |
|---|---|---|---|---|
| Essenza rossa | res_red@v1 | 9 | 4 | 3 |
| Thermal Surge (test) | r_thermal_surge@v1 | 4 | 1 | 0 |
| Furia della Fucina (test) | r_fury@v1 | 4 | 1 | 1 |
| Rincorsa (test) | r_rush@v1 | 4 | 2 | 2 |
| Flame Gauntlet (test) | r_flame_gauntlet@v1 | 4 | 0 | 0 |

### G2: Mazzo blu di prova (Wizards) (deck_blue v1) — Nebros Clearmind gm_nebros@v1, Salute finale 0/20

Abilità del Guildmaster usata 4 volte.

| Carta | Versione | Nel mazzo | Pescate | Giocate |
|---|---|---|---|---|
| Essenza blu | res_blue@v1 | 9 | 3 | 3 |
| Blink Step (test) | b_blink_step@v1 | 4 | 1 | 1 |
| Dardo arcano (test) | b_arcane_dart@v1 | 4 | 2 | 0 |
| Gelo (test) | b_frost@v1 | 4 | 1 | 1 |
| Runic Staff (test) | b_runic_staff@v1 | 4 | 1 | 1 |

## Automazioni e tracciamento (da valutare per il tavolo)

- setup: 1 passaggi automatici
- turn: 7 passaggi automatici
- draw: 7 passaggi automatici
- combat: 14 passaggi automatici
- end: 1 passaggi automatici

## Interventi di debug

Nessuno.

## Note del tester

nota di prova

## Eventi

- T0 [auto] Partita iniziata. Preset P01h, mappa mappa_8x8, seed 20261114. Primo giocatore sorteggiato: G2.
- T0 G2 tiene la mano.
- T0 G1 tiene la mano.
- T0 Nebros Clearmind parte in G8.
- T0 Fiaran Rushblood parte in B2.
- T1 [auto] — Turno 1: G2. Le sue unità e le sue 0 carte risorsa si stappano.
- T1 [auto] Il primo giocatore salta la prima pesca (F1-52).
- T1 G2 gioca Essenza blu (entra pronta).
- T1 Nebros Clearmind si tappa e muove: G8 → F7 → E6 → D6.
- T1 G1 passa.
- T1 [auto] Combattimento automatico: Nebros Clearmind (ranged) infligge 2 a Fiaran Rushblood (Salute 18/20).
- T1 [auto] Fiaran Rushblood non restituisce danni (non adiacente, H6).
- T1 Nebros Clearmind usa la sua abilità su Fiaran Rushblood verso D2 (costo [Fiamma]).
- T1 G1 passa.
- T1 Fiamma di Nebros: Fiaran Rushblood è spostato da B2 a D2.
- T1 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T1 G1 passa.
- T1 G2 termina il turno. Risorse non spese perse (F1-45).
- T2 [auto] — Turno 2: G1. Le sue unità e le sue 0 carte risorsa si stappano.
- T2 [auto] G1 pesca una carta.
- T2 G1 gioca Essenza rossa (entra pronta).
- T2 G1 gioca Rincorsa (test) su Fiaran Rushblood (costo [Bomba]).
- T2 G2 passa.
- T2 Rincorsa (test): Fiaran Rushblood +2 move fino alla fine del turno 2.
- T2 Fiaran Rushblood si tappa e muove: D2 → E3 → F4 → F5 → E6.
- T2 G2 passa.
- T2 [auto] Combattimento automatico: Fiaran Rushblood (melee) infligge 2 a Nebros Clearmind (Salute 18/20).
- T2 [auto] Nebros Clearmind non restituisce danni (difensore a distanza, F1-33).
- T2 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T2 G2 passa.
- T2 G1 termina il turno. Risorse non spese perse (F1-45).
- T3 [auto] — Turno 3: G2. Le sue unità e le sue 1 carte risorsa si stappano.
- T3 [auto] G2 pesca una carta.
- T3 Nebros Clearmind usa la sua abilità su Fiaran Rushblood verso D8 (costo [Fiamma]).
- T3 G1 passa.
- T3 Fiamma di Nebros: Fiaran Rushblood è spostato da E6 a D8.
- T3 G2 gioca Essenza blu (entra pronta).
- T3 G2 gioca Gelo (test) su Fiaran Rushblood (costo [Fiamma]).
- T3 G1 passa.
- T3 Gelo (test): Fiaran Rushblood -2 move fino alla fine del turno 4.
- T3 Nebros Clearmind si tappa e muove: D6 → C7 → C8.
- T3 G1 passa.
- T3 [auto] Combattimento automatico: Nebros Clearmind (ranged) infligge 2 a Fiaran Rushblood (Salute 16/20).
- T3 [auto] Fiaran Rushblood restituisce 2 a Nebros Clearmind (Salute 16/20).
- T3 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T3 G1 passa.
- T3 G2 termina il turno. Risorse non spese perse (F1-45).
- T4 [auto] — Turno 4: G1. Le sue unità e le sue 1 carte risorsa si stappano.
- T4 [auto] G1 pesca una carta.
- T4 Fiaran Rushblood usa la sua abilità verso C7 (costo [Bomba]).
- T4 G2 passa.
- T4 Bomba di Fiaran: Croce su C7, 2 danni a Nebros Clearmind (Salute 14/20).
- T4 G1 gioca Essenza rossa (entra pronta).
- T4 G1 gioca Furia della Fucina (test) su Fiaran Rushblood (costo [Bomba]).
- T4 G2 passa.
- T4 Furia della Fucina (test): Fiaran Rushblood +2 attack fino alla fine del turno 4.
- T4 Fiaran Rushblood si tappa e resta in D8: vale come movimento.
- T4 G2 passa.
- T4 [auto] Combattimento automatico: Fiaran Rushblood (melee) infligge 4 a Nebros Clearmind (Salute 10/20).
- T4 [auto] Nebros Clearmind non restituisce danni (difensore a distanza, F1-33).
- T4 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T4 G2 passa.
- T4 G1 termina il turno. Risorse non spese perse (F1-45).
- T5 [auto] — Turno 5: G2. Le sue unità e le sue 2 carte risorsa si stappano.
- T5 [auto] G2 pesca una carta.
- T5 G2 gioca Blink Step (test) su Nebros Clearmind verso E8 (costo 1[Fiamma]).
- T5 G1 passa.
- T5 Blink Step (test): Nebros Clearmind è spostato da C8 a E8.
- T5 G2 gioca Essenza blu (entra pronta).
- T5 Nebros Clearmind usa la sua abilità su Fiaran Rushblood verso C6 (costo [Fiamma]).
- T5 G1 passa.
- T5 Fiamma di Nebros: Fiaran Rushblood è spostato da D8 a C6.
- T5 Nebros Clearmind si tappa e muove: E8 → D7 → D6 → C5.
- T5 G1 passa.
- T5 [auto] Combattimento automatico: Nebros Clearmind (ranged) infligge 2 a Fiaran Rushblood (Salute 14/20).
- T5 [auto] Fiaran Rushblood restituisce 2 a Nebros Clearmind (Salute 8/20).
- T5 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T5 G1 passa.
- T5 G2 termina il turno. Risorse non spese perse (F1-45).
- T6 [auto] — Turno 6: G1. Le sue unità e le sue 2 carte risorsa si stappano.
- T6 [auto] G1 pesca una carta.
- T6 G1 gioca Rincorsa (test) su Fiaran Rushblood (costo [Bomba]).
- T6 G2 passa.
- T6 Rincorsa (test): Fiaran Rushblood +2 move fino alla fine del turno 6.
- T6 Fiaran Rushblood usa la sua abilità verso B5 (costo [Bomba]).
- T6 G2 passa.
- T6 Bomba di Fiaran: Croce su B5, 2 danni a Nebros Clearmind (Salute 6/20).
- T6 G1 gioca Essenza rossa (entra pronta).
- T6 Fiaran Rushblood si tappa e muove: C6 → B5 → C4.
- T6 G2 passa.
- T6 [auto] Combattimento automatico: Fiaran Rushblood (melee) infligge 2 a Nebros Clearmind (Salute 4/20).
- T6 [auto] Nebros Clearmind non restituisce danni (difensore a distanza, F1-33).
- T6 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T6 G2 passa.
- T6 G1 termina il turno. Risorse non spese perse (F1-45).
- T7 [auto] — Turno 7: G2. Le sue unità e le sue 3 carte risorsa si stappano.
- T7 [auto] G2 pesca una carta.
- T7 Nebros Clearmind usa la sua abilità su Fiaran Rushblood verso C3 (costo [Fiamma]).
- T7 G1 passa.
- T7 Fiamma di Nebros: Fiaran Rushblood è spostato da C4 a C3.
- T7 G2 gioca Runic Staff (test) (costo 1[Fiamma]).
- T7 Fiaran Rushblood usa la sua abilità verso C5 (costo [Bomba]).
- T7 G2 passa.
- T7 Bomba di Fiaran: Croce su C5, 2 danni a Nebros Clearmind (Salute 2/20).
- T7 G1 passa.
- T7 Runic Staff (test) è assegnato a Nebros Clearmind.
- T7 Nebros Clearmind si tappa e muove: C5 → C4.
- T7 G1 passa.
- T7 [auto] Combattimento automatico: Nebros Clearmind (ranged) infligge 2 a Fiaran Rushblood (Salute 12/20).
- T7 [auto] Fiaran Rushblood restituisce 2 a Nebros Clearmind (Salute 0/20).
- T7 [auto] Fine partita: vince G1. Nebros Clearmind è stato sconfitto (F1-54).

## Configurazione completa (copia)

```json
{
  "id": "P01h",
  "name": "Duello — restare fermi vale come mossa",
  "basedOn": "P01",
  "description": "Come P01, ma muoversi nella propria casella è una mossa legale: tappa, soddisfa l'obbligo e fa partire il combattimento.",
  "featureSet": "P01",
  "map": "mappa_8x8",
  "theme": "semplice",
  "partial": true,
  "builtIn": true,
  "choices": {
    "stayMove": "si"
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
