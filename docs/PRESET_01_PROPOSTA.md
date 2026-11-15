# Proposta del primo preset — P01 "Duello dei Guildmaster"

**Test parziale.** Non è il regolamento definitivo né l'implementazione completa dell'inventario. Usa 82 feature su 263 (63 L, 19 T); l'elenco completo è nella colonna "Preset" di `FEATURES.md`.

Etichette: **[D]** definito dalle fonti/dall'inventario · **[H]** ipotesi di test proposta da me, modificabile · **[ND]** non definito.

## Che cosa prova
Il ciclo base: movimento obbligatorio → risposta dell'avversario → combattimento automatico → danni, con mano, costi e pila. Un solo eroe per giocatore, così il nodo si osserva senza il rumore di evocazione, token e limiti alle miniature.

## Regole attive
| Area | Regola | Codici | Stato |
|---|---|---|---|
| Formato | 2 giocatori, primo giocatore sorteggiato | F1-01, F1-53 | D |
| Unità | Una pedina eroe per giocatore; GM già in gioco | F3-01, F4B-01, F1-06 | D (T) |
| Carte | Azioni (normali e istantanee), oggetti assegnati al GM | F1-09 parz., F1-11/12/13, F1-14, F2-06 | D |
| Mazzo | Min 25 carte, max 4 copie, costruito attorno al GM | F1-04, F1-05, F3-02 | D (T) |
| Mano | 5 iniziali; pesca 1 a turno; il primo salta la prima pesca; limite 7 con scarto a fine turno | F1-49, F1-50, F1-51, F1-52, F4B-10 | D (T) |
| Turno | Inizio (untap, risorse, pesca) → principale (ordine libero) → fine | F1-46, F1-47 parz., F1-48 | D |
| Risorse | Carte risorsa giocate dal mazzo (deciso il 14 nov); costi colorati e generici; risorse perse a fine turno | F2-09, F1-43, F1-44, F1-45 | D + H1, H2 |
| Board | Scacchiera 8×8, una casella per unità, muri, highground, zone | U-01, F4b-01, F1-21, F1-19, U-02..05, F4b-03..08 | D, **mappa mancante** |
| Torre | Una per giocatore, fissa, inerte in P01 | F4b-10, F4b-11 | D (posizione T) |
| Partenza | S-B: una delle tre caselle adiacenti alla propria Torre | F4b-16, F4b-17 | D — conflitto C1 |
| Movimento | Tappando la carta; da 1 a 3 caselle, anche in diagonale; percorso dichiarato, non attraverso muri o nemici | F1-24, F4B-05, F3-04, F1-22, F1-23, F1-25 | D (T) |
| Obbligo | Ogni turno il GM deve muovere; se è bloccato l'obbligo decade (deciso il 14 nov) | F3-03 (OBB-A) | D |
| Combattimento | Automatico dopo il movimento, per adiacenza (melee) o linea di vista (ranged); un attacco per unità | F2-03, F2-02, F1-27, F1-31, F1-32 | D + H6 |
| Danni | Reciproci secondo la portata; il difensore ranged non restituisce; persistenti; cure fino al massimo; morte a salute raggiunta | F1-33, F1-34, F1-35, F1-67 | D (T) |
| Risposte | Dopo il movimento la priorità passa all'avversario; pila | F2-04, F2-06 | D + H8 |
| Schivata | Spostarsi in risposta evita il danno: tappando la propria carta (SCH-TAP) o con carte | F2-19, F3-11, F4B-06 | D + H4 |
| Linea di vista | LOS-G: geometrica fra angoli + regola highground | F1-36/37/38, U-05 | D — conflitto C2 |
| Bersagli | In linea di vista; ricontrollo alla risoluzione; ci si può bersagliare | F1-57, F1-58 | D |
| Effetti | Danni diretti, cure, modificatori di attacco/salute/movimento | F2-29, F1-67, F1-68, F1-56 parz. | D |
| Vittoria | GM avversario sconfitto; oppure l'avversario termina il mazzo | F1-54, U-07, F4A-19 | D + H9 |
| PV | 20 per ogni GM, parametro per GM | P-01, F4b-19 | D + H3 |

## Varianti del ciclo base (il preset ne dichiara una per riga)
| Nodo | Varianti | P01 |
|---|---|---|
| Portata di F3-03 con più unità | **OBB-A** almeno un'unità deve muovere · **OBB-B** ogni unità deve muovere | OBB-A (con un solo eroe le due coincidono: la differenza si testerà col preset degli alleati) |
| Come ci si sposta in risposta | **SCH-TAP** tappando la propria carta se è stappata (DD2: "B taps in response") · **SCH-CARTE** solo con carte/abilità istantanee · ~~scartando una carta (BL-34)~~ non selezionabile: voce vuota | SCH-TAP + carte |
| Esito dopo lo spostamento | **RIC-GEO** si ricontrolla contatto/linea di vista: se c'è ancora, si combatte (DD2) · **RIC-ANNULLA** chi si è spostato evita comunque il danno (F3-11 alla lettera) | RIC-GEO per il combattimento automatico; F3-11 alla lettera per gli effetti con bersaglio |
| Posizione iniziale (C1) | **S-A** angoli liberi opposti (U-06) · **S-B** adiacente alla Torre (F4b-16/17) | S-B; clone **P01b** con S-A |
| Linea di vista (C2) | **LOS-G** geometrica · **LOS-Z** fra zone adiacenti (F4C-03) | LOS-G; LOS-Z quando la mappa definisce le zone |

Conseguenza da osservare, non da correggere: con SCH-TAP e movimento obbligatorio, il GM che ha mosso nel proprio turno resta tappato fino al suo turno successivo, quindi **non può schivare col tap durante il turno avversario**. In P01 la schivata nel turno avversario passa quasi solo dalle carte. È esattamente il tipo di cosa che il playtest deve mostrare.

## Ipotesi di test
- **H1** Carte risorsa: se ne può giocare una per turno; resta in gioco; a inizio turno produce 1 risorsa del colore della sua gilda. Nessuna fonte lo scrive: è il minimo per far funzionare F2-09. Quante metterne in un mazzo da 25 è bilanciamento (parto da 9).
- **H2** Le carte risorsa non sono unità: non hanno pedina, non si tappano per muovere, non possono essere attaccate in P01.
- **H3** 20 PV a tutti e quattro i GM (documentato solo per Fiaran e Senya).
- **H4** La schivata col tap usa il movimento normale dell'unità (1–3 caselle).
- ~~H5~~ **Deciso (14 nov):** il ranged attacca solo nel proprio turno. Finire nella sua linea di vista fuori dal suo turno non fa nulla, salvo effetti di carte attive sulla board.
- **H6** Il difensore melee restituisce danno solo se adiacente all'attaccante.
- ~~H7~~ **Deciso (14 nov):** se l'unità è bloccata ed è l'unica in grado di muoversi, l'obbligo decade.
- **H8** Una sola finestra di risposta dopo ogni movimento e dopo ogni carta giocata; la priorità si alterna finché entrambi passano. Uno spostamento in risposta non apre un secondo combattimento automatico.
- **H9** Si perde nel momento in cui si deve pescare da un mazzo vuoto.
- Statistiche e portata dei quattro GM, costi ed effetti delle carte: da proporre a M3 dopo aver aperto le immagini delle carte.

## Feature rinviate (visibili in tabella)
Alleati, evocazione, zone di evocazione, token delle altre unità (F1-10/17/29/30, F2-01, F4C-01/02); ruoli e mosse degli scacchi (F2-13/14, F4B-04); mostri, Jungle con regole, vittoria coi mostri (F3-06/07/08, F4b-07); Torre con PV, danno e vittoria (F4b-12/14/15, F4C-05/06); edifici e relativi modelli di risorse (F2-08, F3-12/13, F4b-18); punti azione e moneta (F4A-04..15, F2-16); parole chiave (Armor, Flying, Swift, Taunt, Unique, Venomous, token); aree d'effetto (F1-39/40); magie continuative, esilio usato da carte, cimitero, mano/mazzo avversari, ricerca nel mazzo (F2-07, F2-26/27, CS-17/20); dadi (F2-18). Root (CS-21) ora è utilizzabile in P01.

## Limiti dichiarati
- Senza la mappa il preset non è avviabile come "Guilds": la board di sviluppo sarà una 8×8 vuota marcata **SEGNAPOSTO**, usata solo per i test tecnici.
- Token delle altre unità: parametro presente ma senza effetto in P01 (non ci sono altre unità); in schermata risulterà "non applicabile", non un interruttore finto.
- Simbolo gilda gialla: parametro di presentazione stivale | pugnale, attivo fin da P01.

## Preset successivo — P02 "Alleati" (solo abbozzo)
Aggiunge alleati sulla board, evocazione e token. Qui entrano come parametri esclusivi le modalità chieste il 14 nov:

| Parametro | Valori | Codici |
|---|---|---|
| Vita delle unità non GM | persistente · **ripristinata** a inizio turno di ogni giocatore | F1-34 · N-01 |
| Assalto | spento · **attivo**: chi vuole finire sulla casella nemica ripete gli scambi finché una muore; il difensore ranged fa 1 danno a scambio | N-02, N-03, N-04 |
| Profilo d'attacco | una sola portata · **doppio** (melee e ranged con danni diversi) | N-05, F3-10 |

Ipotesi **H10**: con doppio profilo, adiacente si usa il valore melee, a distanza quello ranged.
Per confrontare le modalità, i mazzi restano gli stessi: cambia solo il parametro.
