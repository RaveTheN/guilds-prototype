# Tabella delle feature — Guilds prototipo

Generata da `tools/build_feature_registry.py` a partire da `data/features/features.json`. Non modificare a mano.

Voci: **271** (263 dell'inventario + 8 aggiunte N-xx). Scelta: L = mantenere, T = testare, X = esclusa, (vuoto) = non deciso.
Dipendenze e incompatibilità sono analizzate solo per il primo preset e per le aree critiche; altrove: "non analizzato".
Stato di implementazione: da `data/features/impl_status.json` (aggiornato a M4, famiglia di preset P01).

## Riepilogo scelte

| Scelta | Voci |
|---|---|
| L | 95 |
| T | 97 |
| X | 21 |
| vuoto | 58 |

## Conflitti fra L e nodi critici

| ID | Tipo | Codici | Descrizione | Trattamento |
|---|---|---|---|---|
| C1 | conflitto fra L | U-06, F4b-16, F4b-17 | Posizione iniziale: angoli liberi opposti (U-06) contro casella scelta fra le tre adiacenti alla propria Torre (F4b-16/17). F1-18 (T, casella prestabilita) è una terza formulazione. | Parametro esclusivo del preset 'Posizione iniziale': S-A (U-06) \| S-B (F4b-16/17). Entrambe richiedono la mappa. |
| C2 | conflitto fra L | F1-36, F1-37, F1-38, F4C-03, U-05, F4b-03 | Linea di vista: geometrica fra angoli delle caselle, con unità nemiche che bloccano (F1-36/37/38) contro 'solo fra zone adiacenti' (F4C-03). U-05 (highground) si applica come filtro aggiuntivo a entrambe. F4b-03 (coordinate usate per la linea di vista) è coerente solo con la geometrica. | Parametro esclusivo 'Linea di vista': LOS-G (F1-36/37/38+U-05) \| LOS-Z (F4C-03+U-05). LOS-Z richiede la definizione delle zone e della loro adiacenza: non ricavabile senza mappa. |
| C3 | conflitto fra L | F1-47, F2-02, F1-27, F1-62 | F2-02 elimina il segnalino attacco, ma F1-47 ne prevede la rimozione a inizio turno, F1-27 limita a un attacco per unità e la formulazione originale di Swift (F1-62) cita i segnalini. | Interpretazione dichiarata: di F1-47 resta attivo solo il ripristino (untap); il limite di F1-27 è tracciato dal tap, perché il combattimento nasce dal movimento (F2-03) e il movimento tappa (F1-24). Da confermare: domanda aperta D5. |
| C4 | conflitto fra L | F1-48, F1-27, F2-03 | F1-48 elenca l'attacco fra le azioni eseguibili liberamente in fase principale; F2-03 rende il combattimento una conseguenza automatica del movimento, non un'azione dichiarata. | Con F2-03 attivo l'attacco non è un'azione separata. Variante futura 'attacco dichiarato' possibile solo reintroducendo un tracciamento (F1-26 è X). |
| C5 | conflitto fra L | F1-14, F2-06, F2-04 | F1-14: le azioni si giocano nella propria fase principale. F2-06/F2-04: attacchi e magie istantanei giocabili in risposta, anche nel turno avversario. | Interpretazione dichiarata: due velocità di carta (azione normale: solo propria fase principale; istantanea: anche in risposta). Coerente con 'Indecisione - riflessioni' (azioni/reazioni). |
| C6 | nodo critico (ciclo base) | F3-03, F2-03, F2-04, F1-27, F3-11, F2-19, BL-34, F1-24, F4B-06 | Ogni turno si muove per forza, il movimento innesca il combattimento, l'avversario può evitarlo spostandosi. Indefiniti: costo della schivata (BL-34 vuota), portata di F3-03 con più unità, esito se dopo la schivata c'è ancora contatto. | Varianti esplicite in docs/PRESET_01_PROPOSTA.md: OBB-A/OBB-B (portata obbligo), SCH-TAP/SCH-CARTE (schivata), RIC-GEO/RIC-ANNULLA (ricontrollo). Peppe ha indicato OBB-A (14 nov 2026). Il preset 01 dichiara OBB-A, SCH-TAP+carte, RIC-GEO. |
| C7 | conflitto fra L | F3-03, CS-21, F1-24, F1-25 | Movimento obbligatorio contro Root (CS-21) e contro i casi in cui nessun movimento legale esiste (unità circondata, o già tappata per un'abilità visto che il movimento richiede il tap). | Deciso il 14 nov 2026: se un'unità è bloccata ed è l'unica in grado di muoversi, l'obbligo decade. Resta da confermare che 'bloccata' includa Root e 'già tappata'. |
| C8 | L che dipende da T/X/vuoto | F3-08, F3-06, F4b-07, F4b-09 | F3-08 (vittoria sconfiggendo mostri, L) richiede mostri: F3-06 è T, ma F4b-07 (L) e F4b-09 (L) prevedono già mostri nelle Jungle. Quantità e benefici non definiti. | Attiva solo nei preset con mostri implementati. Rinviata: servono mappa e regole dei mostri. |
| C9 | L che dipende da T/X/vuoto | F1-44, F1-43, F1-45, F1-41, F1-42, F2-09, F2-10, F4A-08, BL-21, F3-13 | Le regole L sulle risorse (colori, generici, perdita a fine turno) richiedono un modello di produzione; tutti i modelli sono T, e F1-41 poggia su F1-42 che è X. | Parametro esclusivo 'Modello risorse'. F1-41 non è attivabile così com'è (dipende da una X). Preset 01: F2-09 (carte risorsa nel mazzo), deciso il 14 nov 2026. |
| C10 | L che dipende da T/X/vuoto | F1-62, F1-30 | Swift ha senso solo se le unità appena evocate non agiscono (F1-30, T). | Swift disponibile solo nei preset con F1-30 e con alleati sulla board. |
| C11 | L che dipende da T/X/vuoto | CS-25, BL-35, CS-29, BL-34, CS-14, CS-13, BL-56 | CS-25 richiede Guard (BL-35 vuota). CS-29 (unità che non possono fare Dodge) richiede un modello di schivata: soddisfatto da F2-19, non da BL-34. CS-14 richiede compagni (CS-13 T, BL-56 vuota). | CS-25 non attivabile finché Guard non è decisa. CS-29 e CS-14 attive nei preset con le rispettive dipendenze. |
| C12 | L che dipende da T/X/vuoto | F4b-16, F4b-17, F4b-10, F4b-11, U-06 | La partenza adiacente alla Torre richiede la Torre (F4b-10 L) e una sua posizione (F4b-11 è T). U-06 parla di 'angoli liberi': quali siano dipende da dove stanno Torri e mostri. | S-B attivabile solo con F4b-11 o altra posizione documentata. Serve la mappa. |
| C13 | conflitto fra L | F1-17, F1-29, F1-20, F1-19, U-03, F4b-05, F1-10, F3-01, F4B-01 | Zone di evocazione e alleati evocabili (L) presuppongono più unità sulla board; F1-10, F3-01 e F4B-01 (una sola pedina eroe) sono T. | Parametro 'Presenza unità': solo eroe \| alleati sulla board. Le L sull'evocazione sono attive solo nella seconda modalità. |
| C14 | conflitto fra L | F4A-19, U-07, F1-55 | Mazzo esaurito: sconfitta di chi lo termina (F4A-19, U-07, L) contro 'si continua senza pescare' (F1-55, T). | Non è un conflitto fra L: F1-55 è una variante T del parametro 'Mazzo esaurito'. Momento esatto della sconfitta non definito: ipotesi H9. |
| C15 | risolto (14 nov 2026) | F2-03, F1-32, F1-33 | Combattimento automatico a distanza: non è definito se scatta solo quando chi ha mosso è ranged e vede un nemico, o anche quando chi ha mosso entra nella linea di vista di un ranged nemico (nota di F2-03: 'ranged è da pensare meglio'). | Deciso: il ranged attacca solo nel proprio turno; entrare nella sua linea di vista fuori dal suo turno non fa nulla, salvo effetti di carte attive sulla board. |
| C16 | ambiguità | F1-25, F2-22, F1-19 | F1-25/F1-19: non si attraversano muri. F2-22: movimento sui muri (L). | Interpretazione: F2-22 è un effetto di carta che fa eccezione alla regola (F1-02). La nota di F1-25 aggiunge che il movimento non può mai finire su un muro: da chiarire se vale anche per F2-22. |
| C17 | nuova modalità T contro L esistenti | N-01, N-03, N-04, F1-21, F1-27, F1-33, F1-34, F1-25 | La modalità 'vita che si ripristina + Assalto' tocca regole L: una unità per casella (F1-21), un attacco per unità (F1-27), percorso che non attraversa nemici (F1-25); e regole T: danni persistenti (F1-34), difensore ranged che non restituisce (F1-33). | Parametri esclusivi del preset con alleati: 'Vita delle unità' = persistente (F1-34) \| ripristinata (N-01); 'Assalto' = spento \| attivo (N-03/04). Le eccezioni valgono solo dentro l'Assalto. |

## Fase 1 - Regolamento originale

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| F1-01 | Formato per 2 giocatori. | L |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
| F1-02 | Precedenza delle carte: quando una carta contraddice il regolamento, prevale la carta. | L |  | nessuna rilevata | nessuna rilevata | principio generale: nessuna carta di P01 contraddice il regolamento | P01 |
| F1-03 | Deck building: ogni giocatore costruisce il proprio mazzo. | L |  | nessuna rilevata | nessuna rilevata | parziale: quattro mazzi pronti; editor di mazzi rinviato | P01 (mazzi pronti forniti; editor di mazzi rinviato) |
| F1-04 | Mazzo di almeno 25 carte, Guildmaster escluso. | T |  | nessuna rilevata | F4A-01, F4A-02, BL-10 | implementato | P01 |
| F1-05 | Massimo 4 copie della stessa carta per nome. | T |  | nessuna rilevata | BL-12 | implementato | P01 |
| F1-06 | Guildmaster fuori dal mazzo, già in gioco all’inizio. | L |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
| F1-07 | Buildings esclusi dal mazzo principale. | T |  | F2-08 o F3-12 (edifici) | nessuna rilevata | non implementato |  (nessun edificio in P01) |
| F1-08 | Identità di gilda delle carte determinata dai simboli nel loro valore, anche più gilde sulla stessa carta. | L | **Interpretazione:** Con F1-42 esclusa, il 'valore' è solo il costo: l'identità deriva dai simboli nel costo. | F1-43 | BL-11 (mazzo mono-gilda, se attivo) | parziale: una sola gilda per carta | P01 |
| F1-09 | Tipi di carta: Guildmaster, alleati, azioni e oggetti. | L | Anche se le azioni potrebbero essere anche altro, oltre le carte. Alleati potrebbe essere anche “unità”, testerò entrambi. — **Interpretazione:** La nota lascia aperto il nome (alleati/unità) e che le azioni non siano solo carte. | nessuna rilevata | nessuna rilevata | parziale: Guildmaster, azioni, oggetti; alleati rinviati | P01 (parziale: Guildmaster, azioni, oggetti; alleati rinviati) |
| F1-10 | Guildmaster e alleati rappresentati da pedine sulla board; le carte corrispondenti stanno nella zona di gioco. | T | Sicuramente il guildmaster avrà la sua pedina, per le altre unità è da vedere — **Interpretazione:** Pedina del Guildmaster certa; per le altre unità resta aperto (parametro 'Token delle altre unità'). | nessuna rilevata | F3-01, F4B-01 | non implementato | — |
| F1-11 | Oggetti senza pedina, alcuni assegnati a un’unità. | L |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
| F1-12 | Più oggetti sulla stessa unità, anche con lo stesso nome. | L |  | F1-11 | BL-44 | implementato | P01 |
| F1-13 | Gli oggetti assegnati vanno al cimitero quando lascia il gioco il portatore. | L |  | F1-11 | nessuna rilevata | inosservabile in P01: il portatore è solo il Guildmaster e la sua morte chiude la partita | P01 (in P01 il portatore è solo il Guildmaster, quindi non si osserva) |
| F1-14 | Azioni monouso: si risolvono e vanno al cimitero; giocabili nella propria fase principale. | L | **Interpretazione:** Vale per le azioni non istantanee. | nessuna rilevata | F2-06 (vedi C5) | implementato | P01 |
| F1-15 | Zone distinte: mazzo, mano, cimitero, carte in gioco e board. | L |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
| F1-16 | Board 9×7 con due caselle aggiuntive agli angoli opposti. | T |  | mappa 9x7 (figura del regolamento fase 1, non disponibile come dati) | F4b-01, U-01 | non implementato | — |
| F1-17 | Zone di evocazione personali, una per giocatore. | L |  | alleati sulla board (F1-10) | nessuna rilevata | non implementato |  (inattiva con solo eroe (C13)) |
| F1-18 | Guildmaster su una casella iniziale prestabilita. | T |  | mappa | F4b-17, U-06 (formulazioni diverse) | non implementato | — |
| F1-19 | Muri: bloccano movimento, evocazione e linea di vista. | L |  | mappa | nessuna rilevata | parziale: movimento e linea di vista; evocazione rinviata | P01 (parte 'evocazione' inattiva con solo eroe) |
| F1-20 | Highground: attraversabile, ma non utilizzabile per evocare alleati. | L |  | mappa, alleati sulla board | nessuna rilevata | non implementato |  (inattiva con solo eroe) |
| F1-21 | Una sola unità per casella. | L |  | nessuna rilevata | BL-05, N-03 (solo durante l'Assalto: da chiarire, D17) | implementato | P01 |
| F1-22 | Movimento base di 3 caselle, modificabile dalle carte. | T |  | nessuna rilevata | F2-13, F4B-04 | implementato | P01 |
| F1-23 | Movimento anche diagonale. | T |  | nessuna rilevata | F2-13, F4B-04 (ridondante) | implementato | P01 |
| F1-24 | Movimento attivato tappando la carta. | L |  | nessuna rilevata | F2-05 (X), F4B-02 (X) | implementato | P01 |
| F1-25 | Percorso dichiarato: non si attraversano muri o unità nemiche. | L | Qualora ci dovrebbe essere un movimento tipo quello degli scacchi non c’è bisogno di dichiarare, ma il movimento non può finire su un muro — **Interpretazione:** Nota: con movimento a scacchi non serve dichiarare il percorso, ma non si può finire su un muro. | F1-22 o F3-04 | nessuna rilevata | parziale: percorso più breve calcolato dal programma e registrato (H18) | P01 |
| F1-26 | Attacco e movimento separati: il tap traccia il movimento, un segnalino traccia l’attacco. | X | **Interpretazione:** 'x' minuscola letta come X. | nessuna rilevata | F2-02 | non implementato | — |
| F1-27 | Un attacco per unità, salvo effetti che modificano la disponibilità dell’attacco. | L | **Interpretazione:** Senza segnalino (F2-02) il limite è tracciato dal tap (C3). | nessuna rilevata | nessuna rilevata | implementato | P01 |
| F1-28 | Possibilità di mettere il segnalino attacco anche senza un bersaglio valido, senza attivare gli effetti “quando attacca”. | X |  | nessuna rilevata | nessuna rilevata | non implementato | — |
| F1-29 | Alleati evocabili nella propria zona. | L |  | F1-17, alleati sulla board | nessuna rilevata | non implementato |  (inattiva con solo eroe) |
| F1-30 | Alleati appena evocati non possono muovere o attaccare, salvo eccezioni. | T |  | alleati sulla board | nessuna rilevata | non implementato | — |
| F1-31 | Mischia contro unità adiacenti. | L |  | nessuna rilevata | BL-39 | implementato | P01 |
| F1-32 | Attacchi ranged contro bersagli validi in linea di vista, senza una distanza numerica generale indicata. | L |  | modello di linea di vista (C2) | BL-42 | implementato | P01 |
| F1-33 | Danni reciproci nel combattimento secondo la portata; il difensore ranged non risponde con danni. | T |  | nessuna rilevata | N-04 (eccezione nell'Assalto) | implementato | P01 |
| F1-34 | Danni persistenti, rimossi attraverso cure. | T |  | nessuna rilevata | N-01 (per le unità non GM), variante 'costituzione fissa' (nota di F2-15) | implementato | P01 |
| F1-35 | Morte quando i danni raggiungono o superano la salute. | L |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
| F1-36 | Linea di vista geometrica tra angoli delle caselle. | L |  | F4b-03 | F4C-03 | implementato | P01 (variante LOS-G) |
| F1-37 | Unità nemiche che bloccano la linea di vista. | L |  | F1-36 | F4C-03 | implementato | P01 (variante LOS-G; con solo eroe non c'è un terzo corpo che blocchi) |
| F1-38 | Linea di vista vietata direttamente tra le due zone di evocazione; può partire o terminare nell’highground. | L |  | F1-36, zone dalla mappa | F4C-03 | parziale: regola highground implementata; divieto fra zone di evocazione inattivo (zone non definite nella mappa) | P01 (variante LOS-G; richiede la mappa) |
| F1-39 | Effetti ad area con forme e caselle interessate. | L |  | nessuna rilevata | nessuna rilevata | parziale: solo la Croce dell'abilità di Fiaran, forma ipotizzata (H21) | P01 (rinviata) |
| F1-40 | Effetti lungo una riga o colonna davanti all’unità. | L |  | nessuna rilevata | nessuna rilevata | non implementato |  (rinviata) |
| F1-41 | Risorse automatiche a inizio turno: si sommano i simboli delle carte in gioco. | T | **Interpretazione:** Non attivabile com'è: il meccanismo dei simboli che producono è F1-42, esclusa. | F1-42 (X) | F2-09, F2-10, F4A-08, BL-20 | non implementato | — |
| F1-42 | Il valore della carta determina sia il costo sia la produzione di risorse. | X |  | nessuna rilevata | nessuna rilevata | non implementato | — |
| F1-43 | Risorse colorate: i costi richiedono le gilde corrispondenti. | L |  | modello di produzione (C9) | nessuna rilevata | implementato | P01 |
| F1-44 | Valori generici: pagabili con qualsiasi risorsa; nella produzione permettono di scegliere il colore. | L | **Interpretazione:** In P01 (F2-09) la parte 'produzione' vale per eventuali carte risorsa generiche: producono una risorsa del colore scelto. | modello di produzione (C9) | nessuna rilevata | parziale: costi generici pagabili con qualsiasi risorsa; scelta del colore in produzione inattiva (risorse monocolore) | P01 |
| F1-45 | Risorse inutilizzate perse a fine turno. | L |  | modello di produzione (C9) | F4B-09 (X), CS-18 (X) | implementato per le risorse sciolte; le carte risorsa non usate restano pronte fino al proprio turno successivo (H1) | P01 |
| F1-46 | Turno con inizio, fase principale e fine. | L |  | nessuna rilevata | BL-23, F4A-05 (struttura a punti azione) | implementato | P01 |
| F1-47 | Ripristino delle carte e rimozione dei segnalini attacco a inizio turno. | L |  | nessuna rilevata | F2-02 (parte segnalini) | parziale: solo untap (il segnalino attacco è eliminato, F2-02) | P01 (solo ripristino/untap) |
| F1-48 | Azioni della fase principale eseguibili nell’ordine scelto dal giocatore. | L |  | nessuna rilevata | F2-03 (per l'attacco, C4) | implementato | P01 |
| F1-49 | Mano iniziale di 5 carte. | T |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
| F1-50 | Pesca automatica di una carta per turno. | L |  | nessuna rilevata | F4A-09 | implementato | P01 |
| F1-51 | Il primo giocatore salta la pesca del primo turno. | T |  | F1-50 | nessuna rilevata | implementato | P01 |
| F1-52 | Limite di mano di 7 carte, con scarto dell’eccesso a fine turno. | T |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
| F1-53 | Primo giocatore determinato casualmente con moneta. | L |  | nessuna rilevata | nessuna rilevata | implementato | P01 (sorteggio con seed registrato) |
| F1-54 | Vittoria sconfiggendo il Guildmaster avversario. | L |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
| F1-55 | Mazzo esaurito senza sconfitta: si continua senza pescare. | T |  | nessuna rilevata | F4A-19, U-07 | implementato come variante (deckOut = salta) | — |
| F1-56 | Abilità statiche, attivate e innescate, con eventi come ingresso, movimento e attacco. | L |  | nessuna rilevata | nessuna rilevata | parziale: modificatori statici degli oggetti e abilità attivate dei Guildmaster; nessuna abilità innescata | P01 (parziale: solo i tipi usati dalle carte di P01) |
| F1-57 | Bersagli multipli: un effetto si applica ai bersagli ancora validi. | L |  | nessuna rilevata | nessuna rilevata | parziale: un solo bersaglio per carta, ricontrollato alla risoluzione | P01 |
| F1-58 | Possibilità di bersagliare sé stessi, salvo restrizioni della carta. | L |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
| F1-59 | Armor X: riduce di X il danno subito. | T |  | non analizzato | non analizzato | non implementato | — |
| F1-60 | Flying: permette di muovere di qualsiasi numero di caselle. | T |  | non analizzato | non analizzato | non implementato | — |
| F1-61 | Reposition: mettere un segnalino attacco sull’unità la stappa. | X |  | non analizzato | non analizzato | non implementato | — |
| F1-62 | Swift: l’unità può agire nel turno d’ingresso. | L | **Interpretazione:** Attiva solo nei preset con F1-30 (C10). | F1-30 (T), alleati sulla board | nessuna rilevata | non implementato | — |
| F1-63 | Taunt: protegge dagli attacchi le unità senza Taunt adiacenti. | L | **Interpretazione:** Da riformulare con F2-03: 'attacco' non è più un'azione dichiarata. Non analizzato oltre. | alleati sulla board | nessuna rilevata | non implementato | — |
| F1-64 | Unique: limita la presenza contemporanea degli alleati o oggetti con questo sottotipo, secondo la formulazione originale. | L |  | non analizzato | non analizzato | non implementato | — |
| F1-65 | Venomous: applica veleno; un’unità avvelenata subisce 1 danno all’inizio del proprio turno, indipendentemente dal numero di segnalini. | L |  | nessuna rilevata | nessuna rilevata | implementato (Dalia); non definito come si toglie il veleno (D31) | P01 |
| F1-66 | Alleati token: unità create dagli effetti, che cessano di esistere quando lasciano la zona di gioco. | L |  | non analizzato | non analizzato | non implementato | — |
| F1-67 | Cure senza superare la salute massima. | L |  | F1-34 | nessuna rilevata | implementato | P01 |
| F1-68 | Modificatori di attacco, salute e movimento sulle unità. | L |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
## Fase 2 - Riduzione segnalini e interazione

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| F2-01 | Massimo cinque miniature con nomi diversi, Guildmaster incluso. Ambito per giocatore o globale non esplicitato. | T | Ambito per giocatore. Potrebbe essere anche con nomi diversi in realtà se si trova il modo di segnalare chiaramente quale è quale (o se non ce n’è bisogno) — **Interpretazione:** Ambito: per giocatore. La seconda frase della nota è ambigua (probabilmente intende miniature con lo stesso nome): da chiarire. | alleati sulla board | F3-01, F4B-01, BL-47 | non implementato | — |
| F2-02 | Eliminazione del segnalino attacco. | L |  | nessuna rilevata | F1-26 (X), F1-28 (X), F1-61 (X) | implementato | P01 |
| F2-03 | Combattimento automatico dopo il movimento, per adiacenza o linea di vista nel caso ranged. | L | Ranged è da pensare meglio un attimo, ma in linea di massima deve andare così — **Interpretazione:** Deciso il 14 nov 2026: un'unità ranged attacca solo nel turno del proprio controllore. Un'unità che finisce nella linea di vista di un ranged fuori dal turno di quest'ultimo non subisce nulla, salvo effetti di carte attive sulla board. | modello di movimento, modello di linea di vista | F4A-12 (attacco come azione a PA) | implementato | P01 |
| F2-04 | Risposte dell’avversario al movimento prima del combattimento, con priorità e pila. | L |  | F2-06 | nessuna rilevata | implementato | P01 |
| F2-05 | Carte come comandi: attacchi, spostamenti o altre azioni delle pedine eseguiti tramite carte. | X |  | nessuna rilevata | F1-24, F4B-05 | non implementato | — |
| F2-06 | Attacchi e magie istantanei, giocabili in risposta. | L |  | F2-04 | F1-14 (C5) | implementato | P01 |
| F2-07 | Magie permanenti o continuative. | L |  | nessuna rilevata | nessuna rilevata | non implementato |  (rinviata) |
| F2-08 | Edifici come supporto per giocare altre carte. | T |  | non analizzato | non analizzato | non implementato | — |
| F2-09 | Risorse attraverso carte nel mazzo, come alternativa da valutare. | T | **Interpretazione:** Scelta da Peppe il 14 nov 2026 come modello di risorse del primo preset. Quante se ne giocano a turno, se restano in gioco e cosa producono non è scritto in nessuna fonte: ipotesi H1. | regole delle carte risorsa (non definite: H1) | F2-10, F1-41 | implementato | P01 |
| F2-10 | Risorse esterne al mazzo in quantità prefissata, come alternativa da valutare. | T | **Interpretazione:** La fonte (fase 2) cita come esempi Hearthstone (crescente) e Slay the Spire (fissa): la quantità non è definita. | nessuna rilevata | F2-09, F1-41, F4A-08 | non implementato |  (sostituita da F2-09 in P01 per decisione del 14 nov 2026; resta variante per M4) |
| F2-11 | Board composta da due metà personali, unite all’inizio della partita. | X |  | non analizzato | non analizzato | non implementato | — |
| F2-12 | Metà della board personalizzabili. | X |  | non analizzato | non analizzato | non implementato | — |
| F2-13 | Movimenti fissi ispirati agli scacchi. | T |  | F2-14 o F4C-01 (per sapere quale pezzo) | F1-22, F3-04 | non implementato | — |
| F2-14 | Ruoli delle pedine ispirati agli scacchi, con possibile limite per ruolo o sostituzione della pedina precedente. | T |  | alleati sulla board | nessuna rilevata | non implementato | — |
| F2-15 | Basette per tracciare i punti vita. | T | Sicuramente sì per il GM, sto pensando anche a una variante dove le carte hanno la costituzione fissa come le carte di Magic. In quel caso non ci sarebbe bisogno di basette per la vita, anche se comunque si possono fare basette per segnare forza e costituzione — **Interpretazione:** Sul tavolo; nel prototipo i PV sono mostrati sul token. Variante 'costituzione fissa' citata nella nota: non in inventario. | nessuna rilevata | nessuna rilevata | non implementato | — |
| F2-16 | Moneta dell’azione, citata ma senza funzionamento completo. | T | La moneta dava qualcosa come una risorsa o una azione extra, ma è da testare sia con che senza. — **Interpretazione:** Funzionamento non definito: non implementabile senza una regola. | nessuna rilevata | nessuna rilevata | non implementato | — |
| F2-17 | Zona di esilio distinta dal cimitero. | L |  | nessuna rilevata | nessuna rilevata | inattiva in P01: nessuna carta usa l'esilio | P01 (zona presente; nessuna carta di P01 la usa) |
| F2-18 | Dadi per mira o probabilità. | T |  | non analizzato | non analizzato | non implementato | — |
| F2-19 | Schivare o evitare un attacco muovendosi in risposta. | L |  | F2-04, un modo di muoversi in risposta: F4B-06 o carte F2-06 | nessuna rilevata | implementato | P01 |
| F2-20 | Teletrasporto, sparizione e ricomparsa delle unità. | L |  | nessuna rilevata | nessuna rilevata | non implementato |  (come effetto di carta, dopo P01) |
| F2-21 | Scambio di posizione tra due pedine. | L |  | nessuna rilevata | nessuna rilevata | non implementato |  (richiede due pedine: non in P01) |
| F2-22 | Movimento sui muri. | L |  | nessuna rilevata | F1-25 (C16) | non implementato |  (rinviata) |
| F2-23 | Attacchi alle spalle. | X |  | non analizzato | non analizzato | non implementato | — |
| F2-24 | Trappole. | X |  | non analizzato | non analizzato | non implementato | — |
| F2-25 | Attacchi multipli o unità che si stappano giocando carte attacco. | T |  | non analizzato | non analizzato | non implementato | — |
| F2-26 | Recuperare o rianimare creature e riutilizzare carte dal cimitero. | L |  | non analizzato | non analizzato | non implementato | — |
| F2-27 | Interferire con la mano e il mazzo avversari: scarto, mill, furto o uso delle carte avversarie. | L |  | non analizzato | non analizzato | non implementato | — |
| F2-28 | Neutralizzare azioni o effetti avversari. | L |  | F2-04 | nessuna rilevata | non implementato |  (candidata per P01 (carta 'neutralizza'), da confermare in M3) |
| F2-29 | Danni diretti tramite carte o abilità. | L |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
## Fase 3 - Zero permanents

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| F3-01 | Una sola pedina eroe per giocatore. | T |  | nessuna rilevata | F1-10 (alleati), F2-01, F4C-01, F4C-02 | implementato | P01 |
| F3-02 | Mazzo dedicato all’eroe. | T | **Interpretazione:** Letta come: mazzo costruito attorno al proprio Guildmaster. Regole di costruzione specifiche non definite. | F3-01 | nessuna rilevata | implementato | P01 |
| F3-03 | Movimento obbligatorio ogni turno. | L | **Interpretazione:** Deciso il 14 nov 2026: se un'unità è bloccata ed è l'unica in grado di muoversi, l'obbligo decade. Letto come: l'obbligo si soddisfa muovendo almeno un'unità (OBB-A); se un'altra unità può muovere, l'obbligo resta. Lettura da confermare (D4). | modello di movimento | CS-21 (C7) | implementato (OBB-A; OBB-B non distinguibile con un solo eroe) | P01 (OBB-A) |
| F3-04 | Movimento da un minimo di 1 a un massimo di 3 caselle. | T |  | nessuna rilevata | F2-13, F4B-04, F1-60 (Flying) | implementato | P01 |
| F3-05 | Eroe che diventa più potente durante la partita, con sistema ancora da definire. | X |  | non analizzato | non analizzato | non implementato | — |
| F3-06 | Mostri neutrali nella Jungle. | T | Mi piacerebbe molto se ci fosse | mappa, regole dei mostri | nessuna rilevata | non implementato | — |
| F3-07 | Benefici ottenuti sconfiggendo i mostri. | T |  | F3-06 | nessuna rilevata | non implementato | — |
| F3-08 | Vittoria sconfiggendo un numero di mostri, quantità ancora da stabilire. | L | **Interpretazione:** Attiva solo nei preset con mostri (C8). | F3-06 (T) o F4b-07 (L), quantità da stabilire | nessuna rilevata | non implementato | — |
| F3-09 | Recupero di 1 PV entrando nella casella iniziale. | T |  | non analizzato | non analizzato | non implementato | — |
| F3-10 | Ogni eroe dispone di un attacco melee e uno ranged. | T | **Interpretazione:** Caso particolare di N-05 limitato agli eroi. | nessuna rilevata | nessuna rilevata | non implementato |  (non in P01) |
| F3-11 | Cambiare casella evita il danno dell’attacco subito. | L |  | F2-19 | nessuna rilevata | implementato (letterale per gli effetti con bersaglio; RIC-GEO o RIC-ANNULLA per il combattimento automatico) | P01 (variante RIC-GEO per il combattimento automatico; letterale per effetti con bersaglio) |
| F3-12 | Costruzione e distruzione di edifici. | T |  | non analizzato | non analizzato | non implementato | — |
| F3-13 | Edificio Gilda che fornisce risorse, con annotazione di una Gilda gratuita per turno. | T |  | non analizzato | non analizzato | non implementato | — |
| F3-14 | Torre che danneggia unità entro un raggio. | T |  | non analizzato | non analizzato | non implementato | — |
| F3-15 | Torre come elemento della board anziché carta, alternativa esplicitamente valutata. | T |  | non analizzato | non analizzato | non implementato | — |
| F3-16 | Dadi con modificatori dei personaggi. | X |  | non analizzato | non analizzato | non implementato | — |
## Fase 4A - Nuovo Guilds

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| F4A-01 | Mazzo principale da 40 carte, come riportato nell’archivio. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-02 | Mazzo principale da 45 carte, come riportato nella trascrizione. È un’alternativa documentale, non una revisione accertata. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-03 | Mazzo strutture separato da 15 carte. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-04 | Moneta della gilda come componente di gioco. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-05 | 3 punti azione per turno. | T |  | nessuna rilevata | F1-46/F1-48 (turno libero) | non implementato | — |
| F4A-06 | Azioni ripetibili nello stesso turno, tranne costruire strutture. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-07 | Girare la moneta e produrre risorse costa 0 PA. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-08 | La moneta produce una risorsa di qualsiasi tipo. | T |  | F4A-04 | F2-10, F2-09 | non implementato | — |
| F4A-09 | Pescare una carta costa 1 PA. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-10 | Costruire una struttura costa 1 PA, massimo una per turno. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-11 | Giocare una carta costa 1 PA oltre alle risorse richieste. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-12 | Attaccare l’avversario costa 1 PA. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-13 | Attaccare un mostro costa 2 PA. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-14 | Mulligan durante il turno per 3 PA: «scarti tutto, peschi tutto». Quantità e procedura non ulteriormente precisate. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-15 | Conversione di risorse in danni: 1 risorsa = 1 danno. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-16 | Reazione dei mostri determinata dai dadi. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-17 | Requisito di forza del party per affrontare il mostro. Il confronto esatto è incompleto negli appunti. | X |  | non analizzato | non analizzato | non implementato | — |
| F4A-18 | Vittoria controllando 3 mostri. | T |  | non analizzato | non analizzato | non implementato | — |
| F4A-19 | Esaurimento del mazzo come condizione di fine partita, poi esplicitato nella versione unificata come sconfitta dell’avversario che lo termina. | L |  | nessuna rilevata | F1-55 | implementato | P01 |
## Fase 4B - Another Version / Chess

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| F4B-01 | Una sola pedina eroe sul campo. La formulazione non esplicita “per giocatore”. | T | Per giocatore — **Interpretazione:** Per giocatore (nota). | nessuna rilevata | F1-10 (alleati), F2-01 | implementato | P01 |
| F4B-02 | Movimenti eseguiti tramite carte movimento. | X |  | non analizzato | non analizzato | non implementato | — |
| F4B-03 | Carte movimento con pattern dei pezzi degli scacchi. | X |  | non analizzato | non analizzato | non implementato | — |
| F4B-04 | Movimento base dell’eroe come il Re. | T | Oppure inizialmente come il re e successivamente – o in alternativa - come regina + cavallo — **Interpretazione:** Nota: in alternativa o in seguito regina + cavallo. Variante di movimento per M4. | nessuna rilevata | F1-22, F3-04 | non implementato | — |
| F4B-05 | Tappare la carta dell’eroe per il movimento base. | L |  | nessuna rilevata | F2-05 (X) | implementato | P01 |
| F4B-06 | Movimenti in risposta, esplicitamente previsti come elemento da mettere a punto. | T |  | F2-04 | nessuna rilevata | implementato | P01 (variante SCH-TAP) |
| F4B-07 | Mostri negli highground agli angoli. | T |  | non analizzato | non analizzato | non implementato | — |
| F4B-08 | Minimazzo strutture precostruito da 5–7 carte, al posto delle 15. | T |  | non analizzato | non analizzato | non implementato | — |
| F4B-09 | Conservare una risorsa non utilizzata tappando un edificio. Il significato operativo di «tenere in mano» resta indefinito. | X |  | non analizzato | non analizzato | non implementato | — |
| F4B-10 | Scarto a fine turno oltre il limite di mano, con limite numerico non specificato in questa versione. | L |  | un limite numerico (F1-52) | nessuna rilevata | implementato | P01 |
## Fase 4C - Semi Chess

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| F4C-01 | Ogni pedina ha un tipo corrispondente a un pezzo degli scacchi. | T | Proprio in questi giorni sto pensando che si potrebbe fare che ogni giocatore ha disposizione un tipo per pezzo e può assegnare una unità a quel pezzo. Per assegnarla allo stesso pezzo deve sacrificare l’unità assegnata e ripartire da un pezzo legale. Questa feature apre possibilità a scenari del tipo “se giochi questa corta come TORRE allora (EFFETTO)” — **Interpretazione:** Nota: un pezzo per tipo a disposizione, assegnazione di un'unità al pezzo, sacrificio per riassegnare; apre a effetti 'se giocata come TORRE'. | alleati sulla board | F3-01 | non implementato | — |
| F4C-02 | Massimo una pedina per tipo contemporaneamente. Ambito per giocatore o globale non specificato. | T | PER GIOCATORE — **Interpretazione:** Per giocatore (nota). | F4C-01 | F3-01 | non implementato | — |
| F4C-03 | Linea di vista tra zone adiacenti. | L |  | zone e adiacenze dalla mappa | F1-36, F1-37, F1-38 | non implementato |  (variante LOS-Z, non in P01) |
| F4C-04 | Jungle mantenuta sulla board. | L |  | mappa | nessuna rilevata | non implementato |  (sulla mappa, se la mappa la riporta; senza regole proprie in P01) |
| F4C-05 | Torre all’angolo da conquistare. | T |  | F4b-10 | F4b-15 (distruggere contro conquistare: da chiarire) | non implementato | — |
| F4C-06 | La Torre conquistabile infligge danni, secondo l’idea annotata. | T |  | non analizzato | non analizzato | non implementato | — |
## Fase 4 - Versione unificata

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| U-01 | Uso di una scacchiera standard reperibile commercialmente. | T |  | nessuna rilevata | F1-16 | implementato | P01 |
| U-02 | Board divisa diagonalmente da muri e highground. | L |  | mappa | nessuna rilevata | implementato dalla mappa fornita | P01 (richiede la mappa) |
| U-03 | Una metà della board per ciascun giocatore, con highground condiviso. | L |  | mappa | nessuna rilevata | parziale: la mappa non assegna le metà ai giocatori (zone «da definire») | P01 (richiede la mappa) |
| U-04 | Highground attraversabile dalle unità. | L |  | mappa | nessuna rilevata | implementato | P01 (richiede la mappa) |
| U-05 | Linea di vista che può entrare, uscire o restare nell’highground, ma non attraversarlo completamente tra due zone esterne. | L |  | mappa | nessuna rilevata | implementato | P01 (richiede la mappa) |
| U-06 | Partenza degli eroi negli angoli liberi opposti. | L |  | mappa (quali angoli sono 'liberi') | F4b-16, F4b-17 | implementato (S-A), ma non avviabile sulle mappe attuali: angoli non liberi | P01b (variante S-A) |
| U-07 | Vittoria quando l’avversario termina il mazzo. | L |  | nessuna rilevata | F1-55 | implementato | P01 |
## Fase 4 - Ramo boardless

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| BL-01 | Campo senza scacchiera fisica, organizzato attraverso le posizioni delle carte. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-02 | Tre fasce Close / Mid / Far. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-03 | Alternativa a zone e colonne: Frontline propria, Midline, Frontline nemica, ciascuna con slot A/B/C. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-04 | Unità che entrano nella propria Frontline. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-05 | Più unità nello stesso slot, anche nemiche. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-06 | City Ring: edifici attorno all’avatar, massimo 6 posti. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-07 | Stronghold Row al posto del City Ring: avatar ed edifici disposti in fila. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-08 | Stronghold Row senza limite di lunghezza, con eventuale seconda fila. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-09 | Adiacenza degli edifici a destra e sinistra, limitata alla stessa fila. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-10 | Mazzo da 30 carte. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-11 | Mazzo mono-gilda. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-12 | Massimo 3 copie delle carte non leggendarie. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-13 | Guildmaster a 15 HP. | (vuoto) | **Interpretazione:** Vuota: non usata come base. Citata solo come alternativa numerica a P-01. | nessuna rilevata | P-01 | non implementato | — |
| BL-14 | Avatar inizialmente in Mid, nella prima formulazione. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-15 | Avatar inizialmente nella Stronghold Row, nella revisione. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-16 | Un edificio gratuito al setup, nella prima formulazione. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-17 | Nessun edificio al setup, nella revisione. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-18 | Un edificio gratuito dalla mano per turno. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-19 | Edifici con 2 punti Structure, distruttibili e con danni persistenti. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-20 | Risorse automatiche: 1 per edificio a inizio turno. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-21 | Una risorsa base aggiuntiva prodotta dal Guildmaster. | T | **Interpretazione:** Unica voce BL con una scelta (T). Modello di risorse alternativo/aggiuntivo: non in P01. | nessuna rilevata | nessuna rilevata | non implementato | — |
| BL-22 | Risorse degli edifici del colore della loro gilda, spendibili anche per costi generici. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-23 | Fase specifica per costruire, City Step poi Stronghold Step. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-24 | Un solo movimento base complessivo per turno, assegnato a una unità. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-25 | Movimento base di una zona avanti o indietro nella stessa colonna. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-26 | Movimenti laterali attraverso effetti delle carte. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-27 | Commander Step: usare il movimento disponibile per schierare o spostare l’avatar. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-28 | Avatar schierato trattato come una normale unità. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-29 | Avatar nella Stronghold Row non bersagliabile da Melee/Reach, ma da Ranged, Flying e magie. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-30 | Un’azione per unità pronta: attacco, abilità attivata oppure Guard. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-31 | Abilità del Guildmaster una volta per turno. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-32 | Finestra di reazione quando si dichiara un’azione o si scelgono i bersagli, con prima opportunità al non attivo. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-33 | Ricontrollo dei bersagli dopo le reazioni: l’effetto fallisce per quelli diventati illegali. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-34 | Dodge scartando una carta: spostamento in risposta. Le formulazioni a fasce e a slot differiscono. | (vuoto) | **Interpretazione:** Vuota: il costo 'scarta una carta' non è selezionabile. Vedi C6. | nessuna rilevata | nessuna rilevata | non implementato | — |
| BL-35 | Guard: intercettare attacchi di unità e riceverne i danni. | (vuoto) | **Interpretazione:** Vuota: blocca CS-25 (C11). | nessuna rilevata | nessuna rilevata | non implementato | — |
| BL-36 | Guard posizionato in Frontline e protezione nella stessa colonna, nella revisione a slot. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-37 | Guard protegge anche avatar ed edifici, secondo le condizioni di posizione. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-38 | Guard non intercetta magie, tattiche o veleno. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-39 | Melee nello stesso slot, con danni simultanei quando entrambi sono melee. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-40 | Melee nella Frontline nemica può attaccare lo Stronghold adiacente, nella regola generale; resta un conflitto con la protezione dell’avatar descritta sopra. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-41 | Reach: attacchi alle zone adiacenti nella stessa colonna. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-42 | Ranged: attacchi a qualsiasi slot, anche Stronghold; danno base 1 salvo diversa indicazione e nessuno scambio di danni. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-43 | Flying: attacchi a qualsiasi slot e superamento dei Guard non volanti. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-44 | Un solo equipaggiamento per portatore, salvo eccezioni. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-45 | Ongoing Tactics: tattiche persistenti collocate in uno slot o nella Stronghold Row. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-46 | Ongoing non attaccabili normalmente dalle unità, ma rimovibili da effetti o danni appropriati. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-47 | Limite opzionale di 3 unità per giocatore. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-48 | Mulligan opzionale iniziale: mettere carte in fondo al mazzo e pescarne altrettante meno una, una sola volta. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-49 | Statistiche separate RNG / ATK / HP, usate nelle revisioni delle carte. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-50 | Poison: a fine turno infligge danni pari ai segnalini, poi ne viene rimosso uno. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-51 | Shield: previene danni fino al numero dei segnalini scudo, poi li rimuove tutti. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-52 | Quick: carte giocabili nelle finestre di reazione. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-53 | Counter per tipo: annulla una tattica o abilità della categoria indicata. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-54 | Sabotage: gli edifici danneggiati non producono risorse nel turno successivo. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-55 | Overclock: bonus se per la carta è stata spesa almeno una risorsa rossa. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-56 | Companion: sottotipo di unità, con sinergie dedicate. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-57 | Channel: pagare una risorsa in meno e rinviare ingresso o risoluzione al proprio inizio turno successivo. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-58 | Sneak: evita l’intercettazione dei Guard quando attacca edifici. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
| BL-59 | Haste: agire nel turno d’ingresso. | (vuoto) |  | non analizzato | non analizzato | non implementato | — |
## Fase 4 - Core Set e meccaniche delle carte

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| CS-01 | Abilità rossa del Guildmaster: pagare 1 rosso per fare 2 danni a un edificio e 1 agli edifici adiacenti. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-02 | Abilità gialla del Guildmaster: pagare 1 giallo per fare 1 danno e applicare 1 veleno. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-03 | Abilità verde del Guildmaster: fare 1 danno a un’unità e curare 1 a una propria unità o edificio. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-04 | Abilità blu del Guildmaster: aggiungere un segnalino scudo a un’unità o avatar. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-05 | Edifici con abilità aggiuntive alla produzione di risorse. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-06 | Danneggiare propri edifici o unità per ottenere vantaggi: risorse, attacco o altri effetti. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-07 | Bonus proporzionali ai danni presenti sui propri edifici. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-08 | Danni ad area su fasce, slot o edifici adiacenti. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-09 | Tappare più edifici per alimentare un effetto. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-10 | Schivate gratuite o ricompensate con attacco e altri effetti. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-11 | Danni aggiuntivi contro bersagli avvelenati. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-12 | Trappole o reazioni che puniscono movimento, Dodge o ingresso in una zona. | X | In realtà sarebbe da testare, ma è una cosa molto difficile, meglio farla quando c’è un prototipo più avanzato — **Interpretazione:** X con rinvio esplicito a un prototipo più avanzato: esclusa ora, non scartata. | nessuna rilevata | nessuna rilevata | non implementato | — |
| CS-13 | Creazione di compagni e copie di unità. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-14 | Bonus per la presenza di compagni o altre bestie. | L |  | CS-13 (T) o BL-56 (vuota), alleati sulla board | nessuna rilevata | non implementato | — |
| CS-15 | Edifici che assorbono parte dei danni destinati alle unità. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-16 | Cure distribuite fra più unità o edifici. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-17 | Ricerca di carte specifiche nel mazzo. | L |  | non analizzato | non analizzato | non implementato | — |
| CS-18 | Conservazione di risorse per le reazioni, attraverso un’abilità specifica. | X |  | non analizzato | non analizzato | non implementato | — |
| CS-19 | Effetti che fanno saltare la pesca avversaria. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-20 | Pesca aggiuntiva e selezione delle carte, anche tramite pesca/scarto o controllo della cima. | L |  | non analizzato | non analizzato | non implementato | — |
| CS-21 | Root: impedire il movimento per un turno. | L |  | nessuna rilevata | F3-03 (C7) | non implementato |  (ora utilizzabile in P01: l'obbligo decade se il GM è bloccato) |
| CS-22 | Ritorno di unità in mano. | L |  | non analizzato | non analizzato | non implementato | — |
| CS-23 | Bonus o effetti legati alla posizione: Frontline nemica, Midline, Far o stesso slot. | X |  | non analizzato | non analizzato | non implementato | — |
| CS-24 | Effetti alla morte delle unità. | L |  | non analizzato | non analizzato | non implementato | — |
| CS-25 | Attacchi all’avatar che ignorano Guard. | L | **Interpretazione:** Non attivabile finché Guard non è decisa. | BL-35 (vuota) | nessuna rilevata | non implementato | — |
| CS-26 | Effetti persistenti sugli slot: bonus ai danni, vulnerabilità, veleno all’ingresso o prevenzione del danno. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-27 | Sinergie con gli scudi: trasferimento, maggiore prevenzione o potenziamento di altri effetti. | X |  | non analizzato | non analizzato | non implementato | — |
| CS-28 | Potenziamenti al raggiungimento di una quantità di edifici della propria gilda. | T |  | non analizzato | non analizzato | non implementato | — |
| CS-29 | Unità che non possono effettuare Dodge. | L |  | F2-19 | nessuna rilevata | non implementato | — |
## Fase 4b - Scacchiera, zone e Torri

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| F4b-01 | Scacchiera fisica 8×8. | T | Attraverso le varie iterazioni mi sono reso conto che in realtà se il sistema di movimento è valido, anche la vecchia board potrebbe funzionare benissimo, si potrebbe addirittura pensare a board ad hoc per partite con più di due giocatori o espansioni specifiche, e tenere la board 8x8 come board principale e di facile accesso a giocatori di tutti i livelli — **Interpretazione:** Nota: 8x8 come board principale; la vecchia board e board ad hoc restano possibili. | mappa | F1-16 | implementato | P01 |
| F4b-02 | Sistema ibrido caselle e zone: movimento sulle caselle, organizzazione delle regole per zone. | X | Detta così non è chiara questa regola, ma in sostanza è esattamente quello che c’è già — **Interpretazione:** X sulla formulazione, ma la nota dice che il contenuto 'è esattamente quello che c'è già'. Letta così: la voce è esclusa come regola separata perché ridondante; il comportamento (movimento sulle caselle, regole per zone) resta tramite F4b-03/04/05/06/07. Da confermare. | nessuna rilevata | nessuna rilevata | non implementato | — |
| F4b-03 | Coordinate usate per setup e linea di vista. | L |  | nessuna rilevata | F4C-03 (per la linea di vista) | implementato | P01 |
| F4b-04 | Overlay stampato per mostrare le zone. | L |  | mappa | nessuna rilevata | parziale: tipi di casella colorati; territori dei giocatori non definiti nella mappa | P01 (zone colorate sulla board; richiede la mappa) |
| F4b-05 | Una zona personale per giocatore. | L |  | mappa | nessuna rilevata | parziale: zone personali non definite nella mappa | P01 (richiede la mappa) |
| F4b-06 | Highground conteso lungo la diagonale. | L |  | mappa | nessuna rilevata | implementato dalla mappa fornita | P01 (richiede la mappa) |
| F4b-07 | Jungle laterali accessibili a entrambi, con mostri che conferiscono vantaggi ancora da definire. | L |  | mappa, regole dei mostri (non definite) | nessuna rilevata | non implementato |  (zone Jungle mostrate; mostri non in P01) |
| F4b-08 | Caselle normali senza effetti speciali. | L |  | nessuna rilevata | nessuna rilevata | implementato | P01 |
| F4b-09 | Mappa con posizioni prestabilite di muri, highground e mostri, riportata nel documento. | L |  | documento di aggiornamento fase 4b (non disponibile a M0) | nessuna rilevata | implementato: mappe 8x8 e 9x7 convertite dall'Excel; mostri come ostacoli inerti | P01 (bloccata finché non arriva la mappa) |
| F4b-10 | Una Torre fissa per giocatore, presente dall’inizio e non costruibile. | L |  | una posizione (F4b-11) | nessuna rilevata | implementato come pezzo fisso inerte (H12) | P01 (Torre come pezzo fisso inerte: senza PV, danno o vittoria (F4b-12/14/15 sono T, spente)) |
| F4b-11 | Torri negli angoli H1 e A8. | T |  | F4b-10 | nessuna rilevata | divergente: nella mappa fornita le Torri sono in A1 e H8, non in H1 e A8 | P01 |
| F4b-12 | Torri con punti vita e distruttibili. | T |  | F4b-10 | nessuna rilevata | non implementato |  (non in P01) |
| F4b-13 | Possibilità di valori di HP delle Torri diversi fra gilde, lasciata aperta al testing. | X |  | non analizzato | non analizzato | non implementato | — |
| F4b-14 | Torre che danneggia nemici in ingresso nelle caselle adiacenti. Valore del danno e attivazione ancora aperti. | T |  | F4b-10 | nessuna rilevata | non implementato |  (non in P01; valore e attivazione non definiti) |
| F4b-15 | Vittoria distruggendo la Torre avversaria. | T |  | F4b-12 | nessuna rilevata | non implementato |  (non in P01) |
| F4b-16 | Guildmaster inizialmente adiacente alla propria Torre, non sulla Torre. | L |  | F4b-10, F4b-11 | U-06 | implementato | P01 (variante S-B) |
| F4b-17 | Scelta della casella iniziale fra le tre adiacenti alla Torre. | L |  | F4b-16 | U-06, F1-18 | implementato | P01 (variante S-B) |
| F4b-18 | Edifici fuori dalla scacchiera, nella zona del giocatore. | T |  | non analizzato | non analizzato | non implementato | — |
| F4b-19 | HP dei Guildmaster da determinare tramite playtest, senza obbligo che siano uguali per tutti. | L |  | nessuna rilevata | nessuna rilevata | implementato: Salute sulla carta di ogni Guildmaster | P01 (PV per Guildmaster come parametro) |
## Prototipi fotografati

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| P-01 | Fiaran e Senya a 20 HP, alternativa numerica documentata rispetto ai 15 HP del ramo boardless. | T |  | nessuna rilevata | BL-13 | implementato | P01 (20 PV per tutti e quattro come ipotesi H3 (la fonte lo documenta solo per Fiaran e Senya)) |
## Aggiunte successive all'inventario (14 nov 2026)

| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |
|---|---|---|---|---|---|---|---|
| N-01 | Vita che si ripristina: le unità NON Guildmaster non accumulano danni; la loro vita torna al valore pieno all'inizio del turno di ogni giocatore. | T | Richiesta in chat il 14 nov 2026. — **Interpretazione:** I Guildmaster mantengono danni persistenti. Vicina alla 'costituzione fissa come Magic' della nota di F2-15, che però ripristinava a fine turno. | alleati sulla board (F1-10) | F1-34 (per le unità non GM), F1-65 Venomous e cure F1-67 (perdono quasi tutto il senso sulle unità non GM) | non implementato |  (variante 'Vita delle unità' del preset con alleati (P02)) |
| N-02 | Scambio automatico in mischia: quando due unità melee finiscono in caselle adiacenti si infliggono entrambe, automaticamente, danno pari alla propria forza. | T | Richiesta in chat il 14 nov 2026. Parte della modalità N-01. — **Interpretazione:** Coincide con F2-03 + F1-33 per il caso melee contro melee; registrata a parte perché fa parte della modalità richiesta. | F2-03, F1-31 | nessuna rilevata | non implementato |  (P02) |
| N-03 | Assalto (nome proposto): se l'unità che muove vuole finire il movimento sulla casella dell'unità nemica, gli scambi di danno si ripetono finché una delle due muore. | T | Richiesta in chat il 14 nov 2026. Nome proposto da Claude, da confermare. — **Interpretazione:** Dentro un Assalto i danni si accumulano da uno scambio all'altro anche con N-01, altrimenti non finirebbe mai. Non definiti: esito se muoiono entrambe, Assalto contro un Guildmaster, Assalto dichiarato da un ranged, caso in cui nessuna delle due può fare danno. | N-02, un'unità nemica sulla casella di arrivo | F1-21 (durante l'Assalto), F1-27 (più scambi nello stesso movimento), F1-25 (la casella nemica diventa destinazione legale) | non implementato |  (P02) |
| N-04 | Nell'Assalto, un'unità ranged di cui si vuole prendere la casella infligge comunque 1 danno a ogni scambio, anche se normalmente non restituirebbe danni. | T | Richiesta in chat il 14 nov 2026. | N-03 | F1-33 (il difensore ranged non restituisce danni) | non implementato |  (P02) |
| N-05 | Carte con doppio attacco: la stessa unità può essere sia melee sia ranged, con valori di danno diversi. | T | Richiesta in chat il 14 nov 2026. — **Interpretazione:** Generalizza F3-10. Ipotesi H10: adiacente si usa il valore melee, a distanza quello ranged. | nessuna rilevata | nessuna rilevata | non implementato |  (variante 'Profilo d'attacco' (P02; applicabile anche ai GM)) |
| N-06 | Mulligan iniziale facoltativo, una sola volta: si mettono in fondo al mazzo le carte scelte e se ne pescano altrettante meno una. | T | Richiesto da Peppe dopo la prima prova (14 nov 2026). Procedura ripresa da BL-48, che nell'inventario è non decisa: da confermare. F4A-14 (Mulligan a 3 PA durante il turno) resta rinviata coi punti azione. — **Interpretazione:** BL-48 usata come definizione operativa. | nessuna rilevata | F4A-14 | implementato | P01 |
| N-07 | Abilità proprie dei Guildmaster (quelle stampate sulle carte spoiler), giocabili al massimo una volta per turno. | L | Richiesta da Peppe il 14 nov 2026 dopo la seconda prova. Corrisponde a BL-31, che nell'inventario è non decisa. | F1-56 | nessuna rilevata | implementato | P01 |
| N-08 | Lo spostamento in risposta avviene solo scartando una carta dalla mano, ed è possibile solo in risposta a un attacco. | L | Deciso da Peppe il 14 nov 2026. Riprende BL-34 (schivata scartando una carta), che nell'inventario era non decisa. | F2-04, F2-19 | F4B-06 | implementato | P01 |
