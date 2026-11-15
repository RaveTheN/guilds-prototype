# Resoconto partita — Guilds prototipo

- Versione prototipo: 0.13.0 (M4+)
- Preset: P03b — Scacchi — pezzo scelto all'ingresso (clone di P03) — TEST PARZIALE
- Mappa: mappa_8x8 (originale)
- Simbolo gilda gialla: stivale
- Token delle altre unità: generico
- Tema visivo: semplice
- Seed: 20261114
- Inizio: 2026-11-14 10:00:00 — durata: 20.0 minuti
- Turni: 11 — primo giocatore: G2
- Vincitore: G1 — Dalia Shadowstep è stato sconfitto (F1-54)
- Interventi di debug: 0

## Parametri del preset

- Posizione iniziale (conflitto C1): S-B
- Linea di vista (conflitto C2): LOS-G
- Linea di tiro: come si traccia la retta: angoli
- Segnalini veleno: resta
- Unità sulla board (conflitto C13): alleati
- Dove si evocano gli alleati: territorio
- Alleati appena evocati: si
- Evocare un alleato con lo stesso nome di uno già in campo: nuova
- Miniature massime per giocatore, Guildmaster incluso (F2-01): 5
- Vita delle unità non Guildmaster: persistente
- Assalto: no
- Profilo d'attacco: singolo
- Come si muovono le pedine: scelta
- Con i ruoli degli scacchi il Guildmaster muove come: king
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
- Vittoria distruggendo la Torre: no
- Mostri (caselle M): inerti
- Vita dei mostri: persistente
- Quando un mostro è sconfitto: resta
- Mostro della cura (sconfitto: il Guildmaster di chi lo sconfigge si cura) — carte nel mazzo dei mostri: 4
- Mostro «cura»: punti vita: 4
- Mostro «cura»: danno restituito a chi è a contatto: 1
- Mostro «cura»: beneficio = punti Salute curati al Guildmaster di chi lo sconfigge: 3
- Mostro dell'attacco (sconfitto: quel Guildmaster ottiene Attacco permanente) — carte nel mazzo dei mostri: 3
- Mostro «attacco»: punti vita: 4
- Mostro «attacco»: danno restituito a chi è a contatto: 1
- Mostro «attacco»: beneficio = Attacco permanente dato al Guildmaster di chi lo sconfigge: 1
- Mostro della pesca (sconfitto: quel giocatore pesca) — carte nel mazzo dei mostri: 3
- Mostro «pesca»: punti vita: 4
- Mostro «pesca»: danno restituito a chi è a contatto: 1
- Mostro «pesca»: beneficio = carte pescate da chi lo sconfigge: 1
- Vittoria con N mostri sconfitti (0 = spenta; F3-08): 0
- Modello delle risorse (conflitto C9): carte
- X = risorse fuori dal mazzo (fisse, o massimo delle crescenti): 3
- Risorsa base del Guildmaster: no
- Edifici: no
- X = punti Struttura di un edificio: 2
- Mano iniziale: 5
- Limite di mano a fine turno: 7
- Carte pescate a turno: 1
- Il primo giocatore salta la prima pesca (1 = sì): 1
- Carte risorsa giocabili a turno (H1): 1
- Movimento massimo base (minimo sempre 1, F3-04): 3
- Carte minime nel mazzo: 25
- Copie massime per carta (risorse escluse, H14): 4

## Mazzi e carte (pescate / giocate)

### G1: Mazzo verde con alleati (Hunters) (deck_green_alleati v1) — Senya Windseeker gm_senya@v1, Salute finale 7/20

Abilità del Guildmaster usata 6 volte.

| Carta | Versione | Nel mazzo | Pescate | Giocate |
|---|---|---|---|---|
| Essenza verde | res_green@v1 | 9 | 5 | 5 |
| Field Dressing (test) | g_field_dressing@v1 | 2 | 0 | 0 |
| Regrowth (test) | g_regrowth@v1 | 2 | 2 | 2 |
| Hunter's Net (test) | g_hunters_net@v1 | 2 | 1 | 1 |
| Hunter's Bow (test) | g_hunters_bow@v1 | 2 | 0 | 0 |
| Segugio (test) | g_hound@v1 | 2 | 2 | 2 |
| Arciere dei boschi (test) | g_wood_archer@v1 | 2 | 0 | 0 |
| Orso dei boschi (test) | g_bear@v1 | 2 | 0 | 0 |
| Esploratrice (test) | g_scout@v1 | 2 | 0 | 0 |

### G2: Mazzo giallo con alleati (Rogues) (deck_yellow_alleati v1) — Dalia Shadowstep gm_dalia@v1, Salute finale 0/20

Abilità del Guildmaster usata 3 volte.

| Carta | Versione | Nel mazzo | Pescate | Giocate |
|---|---|---|---|---|
| Essenza gialla | res_yellow@v1 | 9 | 4 | 4 |
| Smoke Step (test) | y_smoke_step@v1 | 2 | 1 | 1 |
| Toxic Shiv (test) | y_toxic_shiv@v1 | 2 | 0 | 0 |
| Polvere accecante (test) | y_blinding_dust@v1 | 2 | 2 | 2 |
| Venom Blades (test) | y_venom_blades@v1 | 2 | 0 | 0 |
| Tagliaborse (test) | y_cutpurse@v1 | 2 | 0 | 0 |
| Lanciacoltelli (test) | y_knife_thrower@v1 | 2 | 1 | 1 |
| Guardaspalle (test) | y_bodyguard@v1 | 2 | 1 | 1 |
| Sicario (test) | y_assassin@v1 | 2 | 1 | 1 |

## Automazioni e tracciamento (da valutare per il tavolo)

- setup: 1 passaggi automatici
- turn: 11 passaggi automatici
- draw: 11 passaggi automatici
- combat: 34 passaggi automatici
- venom: 14 passaggi automatici
- death: 2 passaggi automatici
- end: 1 passaggi automatici

## Interventi di debug

Nessuno.

## Note del tester

nota di prova

## Eventi

- T0 [auto] Partita iniziata. Preset P03b, mappa mappa_8x8, seed 20261114. Primo giocatore sorteggiato: G2.
- T0 G2 tiene la mano.
- T0 G1 tiene la mano.
- T0 Dalia Shadowstep parte in G8.
- T0 Senya Windseeker parte in B2.
- T1 [auto] — Turno 1: G2. Le sue unità e le sue 0 carte risorsa si stappano.
- T1 [auto] Il primo giocatore salta la prima pesca (F1-52).
- T1 G2 gioca Essenza gialla (entra pronta).
- T1 G2 gioca Smoke Step (test) su Dalia Shadowstep verso E8 (costo [Stivale]).
- T1 G1 passa.
- T1 Smoke Step (test): Dalia Shadowstep è spostato da G8 a E8.
- T1 Dalia Shadowstep si tappa e muove: E8 → D7.
- T1 G1 passa.
- T1 [auto] Nessun combattimento: nessun nemico a contatto di Dalia Shadowstep.
- T1 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T1 G1 passa.
- T1 G2 termina il turno. Risorse non spese perse (F1-45).
- T2 [auto] — Turno 2: G1. Le sue unità e le sue 0 carte risorsa si stappano.
- T2 [auto] G1 pesca una carta.
- T2 G1 gioca Essenza verde (entra pronta).
- T2 Senya Windseeker usa la sua abilità su Dalia Shadowstep (costo [Arco]).
- T2 G2 passa.
- T2 Freccia di Senya: 1 danni a Dalia Shadowstep (Salute 19/20).
- T2 Freccia di Senya: cura 1 a Senya Windseeker (Salute 20/20).
- T2 Senya Windseeker si tappa e muove: B2 → A3.
- T2 G2 passa.
- T2 [auto] Combattimento automatico: Senya Windseeker (ranged) infligge 2 a Dalia Shadowstep (Salute 17/20).
- T2 [auto] Dalia Shadowstep non restituisce danni (non adiacente, H6).
- T2 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T2 G2 passa.
- T2 G1 termina il turno. Risorse non spese perse (F1-45).
- T3 [auto] — Turno 3: G2. Le sue unità e le sue 1 carte risorsa si stappano.
- T3 [auto] G2 pesca una carta.
- T3 G2 gioca Essenza gialla (entra pronta).
- T3 Dalia Shadowstep usa la sua abilità su Senya Windseeker (costo [Stivale]).
- T3 G1 passa.
- T3 Lama di Dalia: 1 danni a Senya Windseeker (Salute 19/20).
- T3 [auto] Senya Windseeker riceve un segnalino veleno da Dalia Shadowstep (Venomous).
- T3 G2 gioca Polvere accecante (test) su Senya Windseeker (costo [Stivale]).
- T3 G1 passa.
- T3 Polvere accecante (test): Senya Windseeker -2 attack fino alla fine del turno 3.
- T3 Dalia Shadowstep si tappa e muove: D7 → D6.
- T3 G1 passa.
- T3 [auto] Nessun combattimento: nessun nemico a contatto di Dalia Shadowstep.
- T3 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T3 G1 passa.
- T3 G2 termina il turno. Risorse non spese perse (F1-45).
- T4 [auto] — Turno 4: G1. Le sue unità e le sue 1 carte risorsa si stappano.
- T4 [auto] Senya Windseeker è avvelenato: subisce 1 danno (Salute 18/20).
- T4 [auto] G1 pesca una carta.
- T4 Senya Windseeker usa la sua abilità su Dalia Shadowstep (costo [Arco]).
- T4 G2 passa.
- T4 Freccia di Senya: 1 danni a Dalia Shadowstep (Salute 16/20).
- T4 Freccia di Senya: cura 1 a Senya Windseeker (Salute 19/20).
- T4 G1 gioca Essenza verde (entra pronta).
- T4 G1 gioca Segugio (test) verso C4 (costo [Arco]).
- T4 G2 passa.
- T4 Segugio (test) entra in gioco in C4 come Cavallo, pronto.
- T4 Senya Windseeker si tappa e muove: A3 → B4.
- T4 G2 passa.
- T4 [auto] Combattimento automatico: Senya Windseeker (ranged) infligge 2 a Dalia Shadowstep (Salute 14/20).
- T4 [auto] Dalia Shadowstep non restituisce danni (non adiacente, H6).
- T4 Segugio (test) si tappa e muove: C4 → B6.
- T4 G2 passa.
- T4 [auto] Nessun combattimento: nessun nemico a contatto di Segugio (test).
- T4 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T4 G2 passa.
- T4 G1 termina il turno. Risorse non spese perse (F1-45).
- T5 [auto] — Turno 5: G2. Le sue unità e le sue 2 carte risorsa si stappano.
- T5 [auto] G2 pesca una carta.
- T5 Dalia Shadowstep usa la sua abilità su Senya Windseeker (costo [Stivale]).
- T5 G1 passa.
- T5 Lama di Dalia: 1 danni a Senya Windseeker (Salute 18/20).
- T5 [auto] Senya Windseeker riceve un segnalino veleno da Dalia Shadowstep (Venomous).
- T5 G2 gioca Essenza gialla (entra pronta).
- T5 G2 gioca Lanciacoltelli (test) verso F4 (costo 1[Stivale]).
- T5 G1 passa.
- T5 Lanciacoltelli (test) entra in gioco in F4 come Cavallo, tappato: agirà dal prossimo turno (F1-30).
- T5 Dalia Shadowstep si tappa e muove: D6 → C5.
- T5 G1 passa.
- T5 [auto] Più nemici a portata: Dalia Shadowstep attacca Senya Windseeker (scelta automatica).
- T5 [auto] Senya Windseeker riceve un segnalino veleno da Dalia Shadowstep (Venomous).
- T5 [auto] Combattimento automatico: Dalia Shadowstep (melee) infligge 2 a Senya Windseeker (Salute 16/20).
- T5 [auto] Senya Windseeker non restituisce danni (difensore a distanza, F1-33).
- T5 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T5 G1 passa.
- T5 G2 termina il turno. Risorse non spese perse (F1-45).
- T6 [auto] — Turno 6: G1. Le sue unità e le sue 2 carte risorsa si stappano.
- T6 [auto] Senya Windseeker è avvelenato: subisce 1 danno (Salute 15/20).
- T6 [auto] G1 pesca una carta.
- T6 Senya Windseeker usa la sua abilità su Dalia Shadowstep (costo [Arco]).
- T6 G2 passa.
- T6 Freccia di Senya: 1 danni a Dalia Shadowstep (Salute 13/20).
- T6 Freccia di Senya: cura 1 a Senya Windseeker (Salute 16/20).
- T6 G1 gioca Segugio (test) verso C2 (costo [Arco]).
- T6 G2 passa.
- T6 Segugio (test) entra in gioco in C2 come Alfiere, pronto.
- T6 G1 gioca Essenza verde (entra pronta).
- T6 G1 gioca Regrowth (test) su Segugio (test) (costo [Arco]).
- T6 G2 passa.
- T6 Regrowth (test): cura 2 a Segugio (test) (Salute 2/2).
- T6 Senya Windseeker si tappa e muove: B4 → B5.
- T6 G2 passa.
- T6 [auto] Combattimento automatico: Senya Windseeker (ranged) infligge 2 a Dalia Shadowstep (Salute 11/20).
- T6 [auto] Senya Windseeker riceve un segnalino veleno da Dalia Shadowstep (Venomous).
- T6 [auto] Dalia Shadowstep restituisce 2 a Senya Windseeker (Salute 14/20).
- T6 Segugio (test) si tappa e muove: B6 → C4.
- T6 G2 passa.
- T6 [auto] Combattimento automatico: Segugio (test) (melee) infligge 2 a Dalia Shadowstep (Salute 9/20).
- T6 [auto] Segugio (test) riceve un segnalino veleno da Dalia Shadowstep (Venomous).
- T6 [auto] Dalia Shadowstep restituisce 2 a Segugio (test) (Salute 0/2).
- T6 [auto] Segugio (test) è sconfitto e lascia la board.
- T6 Segugio (test) si tappa e muove: C2 → B3.
- T6 G2 passa.
- T6 [auto] Nessun combattimento: nessun nemico a contatto di Segugio (test).
- T6 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T6 G2 passa.
- T6 G1 termina il turno. Risorse non spese perse (F1-45).
- T7 [auto] — Turno 7: G2. Le sue unità e le sue 3 carte risorsa si stappano.
- T7 [auto] G2 pesca una carta.
- T7 G2 gioca Sicario (test) verso H4 (costo 2[Stivale]).
- T7 G1 passa.
- T7 Sicario (test) entra in gioco in H4 come Alfiere, pronto.
- T7 Dalia Shadowstep si tappa e muove: C5 → B4.
- T7 G1 passa.
- T7 [auto] Più nemici a portata: Dalia Shadowstep attacca Senya Windseeker (scelta automatica).
- T7 [auto] Senya Windseeker riceve un segnalino veleno da Dalia Shadowstep (Venomous).
- T7 [auto] Combattimento automatico: Dalia Shadowstep (melee) infligge 2 a Senya Windseeker (Salute 12/20).
- T7 [auto] Senya Windseeker non restituisce danni (difensore a distanza, F1-33).
- T7 Lanciacoltelli (test) si tappa e muove: F4 → D3.
- T7 G1 passa.
- T7 [auto] Più nemici a portata: Lanciacoltelli (test) attacca Senya Windseeker (scelta automatica).
- T7 [auto] Combattimento automatico: Lanciacoltelli (test) (ranged) infligge 1 a Senya Windseeker (Salute 11/20).
- T7 [auto] Senya Windseeker non restituisce danni (difensore a distanza, F1-33).
- T7 Sicario (test) si tappa e muove: H4 → G5 → F6 → E7 → D8.
- T7 G1 passa.
- T7 [auto] Nessun combattimento: nessun nemico a contatto di Sicario (test).
- T7 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T7 G1 passa.
- T7 G2 termina il turno. Risorse non spese perse (F1-45).
- T8 [auto] — Turno 8: G1. Le sue unità e le sue 3 carte risorsa si stappano.
- T8 [auto] Senya Windseeker è avvelenato: subisce 1 danno (Salute 10/20).
- T8 [auto] G1 pesca una carta.
- T8 Senya Windseeker usa la sua abilità su Dalia Shadowstep (costo [Arco]).
- T8 G2 passa.
- T8 Freccia di Senya: 1 danni a Dalia Shadowstep (Salute 8/20).
- T8 Freccia di Senya: cura 1 a Senya Windseeker (Salute 11/20).
- T8 G1 gioca Hunter's Net (test) su Sicario (test) (costo 1[Arco]).
- T8 G2 passa.
- T8 Hunter's Net (test): Sicario (test) non può muoversi fino alla fine del turno 9.
- T8 G1 gioca Essenza verde (entra pronta).
- T8 G1 gioca Regrowth (test) su Senya Windseeker (costo [Arco]).
- T8 G2 passa.
- T8 Regrowth (test): cura 2 a Senya Windseeker (Salute 13/20).
- T8 Senya Windseeker si tappa e muove: B5 → C4.
- T8 G2 passa.
- T8 [auto] Più nemici a portata: Senya Windseeker attacca Dalia Shadowstep (scelta automatica).
- T8 [auto] Combattimento automatico: Senya Windseeker (ranged) infligge 2 a Dalia Shadowstep (Salute 6/20).
- T8 [auto] Senya Windseeker riceve un segnalino veleno da Dalia Shadowstep (Venomous).
- T8 [auto] Dalia Shadowstep restituisce 2 a Senya Windseeker (Salute 11/20).
- T8 Segugio (test) si tappa e muove: B3 → A4.
- T8 G2 passa.
- T8 [auto] Combattimento automatico: Segugio (test) (melee) infligge 2 a Dalia Shadowstep (Salute 4/20).
- T8 [auto] Segugio (test) riceve un segnalino veleno da Dalia Shadowstep (Venomous).
- T8 [auto] Dalia Shadowstep restituisce 2 a Segugio (test) (Salute 0/2).
- T8 [auto] Segugio (test) è sconfitto e lascia la board.
- T8 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T8 G2 passa.
- T8 G1 termina il turno. Risorse non spese perse (F1-45).
- T9 [auto] — Turno 9: G2. Le sue unità e le sue 3 carte risorsa si stappano.
- T9 [auto] G2 pesca una carta.
- T9 G2 gioca Polvere accecante (test) su Senya Windseeker (costo [Stivale]).
- T9 G1 passa.
- T9 Polvere accecante (test): Senya Windseeker -2 attack fino alla fine del turno 9.
- T9 G2 gioca Guardaspalle (test) verso F7 (costo 1[Stivale]).
- T9 G1 passa.
- T9 Guardaspalle (test) entra in gioco in F7 come Regina, tappato: agirà dal prossimo turno (F1-30).
- T9 Dalia Shadowstep si tappa e muove: B4 → B5.
- T9 G1 passa.
- T9 [auto] Senya Windseeker riceve un segnalino veleno da Dalia Shadowstep (Venomous).
- T9 [auto] Combattimento automatico: Dalia Shadowstep (melee) infligge 2 a Senya Windseeker (Salute 9/20).
- T9 [auto] Senya Windseeker non restituisce danni (difensore a distanza, F1-33).
- T9 Lanciacoltelli (test) si tappa e muove: D3 → C5.
- T9 G1 passa.
- T9 [auto] Combattimento automatico: Lanciacoltelli (test) (ranged) infligge 1 a Senya Windseeker (Salute 8/20).
- T9 [auto] Senya Windseeker non restituisce danni (difensore a distanza, F1-33).
- T9 G2 dichiara la fine del turno: finestra di risposta di fine turno.
- T9 G1 passa.
- T9 G2 termina il turno. Risorse non spese perse (F1-45).
- T10 [auto] — Turno 10: G1. Le sue unità e le sue 4 carte risorsa si stappano.
- T10 [auto] Senya Windseeker è avvelenato: subisce 1 danno (Salute 7/20).
- T10 [auto] G1 pesca una carta.
- T10 Senya Windseeker usa la sua abilità su Dalia Shadowstep (costo [Arco]).
- T10 G2 passa.
- T10 Freccia di Senya: 1 danni a Dalia Shadowstep (Salute 3/20).
- T10 Freccia di Senya: cura 1 a Senya Windseeker (Salute 8/20).
- T10 G1 gioca Essenza verde (entra pronta).
- T10 Senya Windseeker si tappa e muove: C4 → B4.
- T10 G2 passa.
- T10 [auto] Più nemici a portata: Senya Windseeker attacca Dalia Shadowstep (scelta automatica).
- T10 [auto] Combattimento automatico: Senya Windseeker (ranged) infligge 2 a Dalia Shadowstep (Salute 1/20).
- T10 [auto] Senya Windseeker riceve un segnalino veleno da Dalia Shadowstep (Venomous).
- T10 [auto] Dalia Shadowstep restituisce 2 a Senya Windseeker (Salute 6/20).
- T10 G1 dichiara la fine del turno: finestra di risposta di fine turno.
- T10 G2 passa.
- T10 G1 termina il turno. Risorse non spese perse (F1-45).
- T11 [auto] — Turno 11: G2. Le sue unità e le sue 3 carte risorsa si stappano.
- T11 [auto] G2 pesca una carta.
- T11 G2 gioca Essenza gialla (entra pronta).
- T11 Dalia Shadowstep usa la sua abilità su Senya Windseeker (costo [Stivale]).
- T11 Senya Windseeker usa la sua abilità su Dalia Shadowstep (costo [Arco]).
- T11 G2 passa.
- T11 Freccia di Senya: 1 danni a Dalia Shadowstep (Salute 0/20).
- T11 Freccia di Senya: cura 1 a Senya Windseeker (Salute 7/20).
- T11 [auto] Fine partita: vince G1. Dalia Shadowstep è stato sconfitto (F1-54).

## Configurazione completa (copia)

```json
{
  "id": "P03b",
  "name": "Scacchi — pezzo scelto all'ingresso",
  "basedOn": "P03",
  "description": "Come P03, ma ogni alleato sceglie il pezzo quando entra, fra quelli a disposizione del giocatore: 1 Cavallo, 1 Alfiere, 1 Regina, 1 Torre, 2 Pedoni. Il Re è del Guildmaster, che qui muove come il Re. Usa i mazzi «con alleati».",
  "featureSet": "P01",
  "map": "mappa_8x8",
  "theme": "semplice",
  "partial": true,
  "builtIn": true,
  "choices": {
    "units": "alleati",
    "moveStyle": "scelta"
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
    "H10 Doppio profilo (N-05): adiacente si usa il valore in mischia, altrimenti quello a distanza. Il secondo profilo dei Guildmaster vale 1 (valore ipotizzato).",
    "H27 Territori: il territorio di un giocatore sono le caselle normali collegate alle partenze accanto alla sua Torre senza passare da muri o highground; ogni blocco di highground è una zona condivisa. La mappa non li definisce (D23). Servono per l'evocazione e per la linea di vista a zone.",
    "H28 Assalto (N-03): può dichiararlo qualsiasi unità, anche a distanza; l'unità si ferma accanto e gli scambi sono simultanei; chi vince occupa la casella; se muoiono entrambe resta vuota; se nessuna può fare danno si interrompe. Contro i Guildmaster solo con la variante «tutti».",
    "H29 Scacchi: Pedone = una casella in orizzontale o verticale; non si attraversano le unità (il Cavallo salta); i bonus al movimento non valgono; il ruolo dell'alleato è scritto sulla carta.",
    "H30 Edifici: sono le carte risorsa in gioco; si colpiscono senza linea di vista con carte e abilità che fanno solo danni; a 0 punti Struttura vanno negli scarti.",
    "H31 Mostri: stanno sulle caselle M; si colpiscono col combattimento automatico solo se non c'è un'unità nemica o una Torre da colpire; restituiscono danno solo a chi è a contatto; sconfitti restano come ostacolo; il beneficio è un'ipotesi fra tre.",
    "H32 Alleati: entrano dalla pila come le azioni normali; alla morte la carta va negli scarti; gli oggetti e le abilità restano del Guildmaster; le carte vedono dal Guildmaster. Con più nemici a portata sceglie chi attacca, altrimenti il primo in ordine (Guildmaster per primo).",
    "ND Veleno (F1-65): le fonti non dicono come si toglie un segnalino: per ora non si toglie mai (D31).",
    "ND Entrambi i Guildmaster sconfitti nello stesso scambio: esito non definito (D21); la partita finisce senza vincitore."
  ],
  "limits": [
    "Test parziale: sottoinsieme delle feature dell'inventario. Non è il regolamento definitivo.",
    "Alleati, Assalto, scacchi, mostri, edifici, risorse fuori dal mazzo e linea di vista a zone esistono solo come varianti, quasi tutte basate su ipotesi (H27–H32). Jungle senza regole: le mappe non hanno caselle Jungle.",
    "Zone personali non definite nella mappa (foglio Zone: «da definire»): i territori sono ricavati per ipotesi (H27). Il divieto di F1-38 fra le zone di evocazione non è applicato.",
    "Il foglio Note dell'Excel descrive una disposizione diversa dalle celle dei fogli Mappa: usate le celle (Torri in A1 e H8, non H1 e A8 come in F4b-11)."
  ],
  "deferred": [
    "Token per ruolo o per gilda; assegnazione libera dei pezzi degli scacchi (nota di F4C-01)",
    "Jungle con regole proprie; reazione dei mostri coi dadi (F4A-16)",
    "Torre da conquistare (F4C-05/06), danno della Torre a raggio (F3-14)",
    "Edifici come supporto per altre carte, mazzo strutture (F2-08, F4A-03, CS-05..09)",
    "Punti azione e moneta (F4A-04..15, F2-16)",
    "Parole chiave: Armor, Flying, Taunt, Unique",
    "Aree d'effetto diverse dalla Croce (F1-39/40)",
    "Magie continuative, esilio, ricerca nel mazzo, dadi",
    "Scelta manuale del percorso (H18): nessuna regola dipende dal percorso"
  ]
}
```
