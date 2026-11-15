# Catalogo delle carte — Guilds prototipo

Generato da `dotnet test` a partire da `data/cards/`. Non modificare a mano: cambia `tools/gen_content.py` e rilancia.

Tutte le carte sono **carte di test proposte**, non canon. Il testo è generato dagli effetti eseguibili, quindi coincide con la logica.

## Gilda blu

### Nebros Clearmind
- ID e versione: `gm_nebros@v1` — Guildmaster — costo — — Attacco 2, Salute 20, ranged
- Testo effettivo: A distanza. Attacco 2, Salute 20.
Abilità, una volta per turno, anche in risposta (costo: 1 risorsa blu): Sposta un'unità in linea di vista fino a 2 caselle.
- Origine: adattamento — reference/Guild References/Spoilers - blue/Nebros Clearmind
- Serve a testare: Ciclo base e abilità propria del Guildmaster, una volta per turno
- Motivazione dei valori: Attacco 2, Salute 20, portata e abilità presi dalla carta spoiler. Costo dell'abilità: 1 risorsa della gilda (il simbolo prima dei due punti).
- Regole richieste: type.Guildmaster, target.none, range.ranged, ability.activated, effect.reposition
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1
- Modello del token: models/nebros.obj

### Essenza blu
- ID e versione: `res_blue@v1` — Risorsa — costo —
- Testo effettivo: Resta in gioco. All'inizio del tuo turno produce 1 risorsa blu.
- Origine: nuova proposta — carta di test proposta
- Serve a testare: Modello di risorse F2-09 (ipotesi H1)
- Motivazione dei valori: 1 risorsa a turno: il minimo che fa funzionare F2-09
- Regole richieste: type.Resource, target.none
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Blink Step (test)
- ID e versione: `b_blink_step@v1` — Azione istantanea — costo 1[Fiamma]
- Testo effettivo: Istantanea. Sposta un'unità in linea di vista fino a 2 caselle.
- Origine: adattamento — Core set - fase 4: Blink Step, sposta un'unità (testo originale basato su regole boardless)
- Serve a testare: Spostamento in risposta con carte, anche del nemico: rompe contatto o linea di vista (F2-19)
- Motivazione dei valori: 2 caselle come l'abilità stampata di Nebros; 2 risorse perché può annullare un combattimento
- Regole richieste: type.Instant, target.any, cost.resources, effect.reposition
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Dardo arcano (test)
- ID e versione: `b_arcane_dart@v1` — Azione istantanea — costo 1[Fiamma]
- Testo effettivo: Istantanea. Infligge 2 danni a un'unità nemica in linea di vista.
- Origine: nuova proposta — carta di test proposta
- Serve a testare: Danno diretto istantaneo e ricontrollo del bersaglio (F3-11)
- Motivazione dei valori: 2 danni a velocità istantanea per 2 risorse
- Regole richieste: type.Instant, target.enemy, cost.resources, effect.damage
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Gelo (test)
- ID e versione: `b_frost@v1` — Azione — costo [Fiamma]
- Testo effettivo: Un'unità nemica in linea di vista ottiene -2 Movimento fino alla fine del prossimo turno del suo proprietario.
- Origine: nuova proposta — carta di test proposta
- Serve a testare: Controllo del movimento avversario senza bloccarlo (confronto con Root)
- Motivazione dei valori: Il nemico muove di 1 nel suo prossimo turno
- Regole richieste: type.Action, target.enemy, cost.resources, effect.mod, stat.move, duration.targetNextTurn
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Runic Staff (test)
- ID e versione: `b_runic_staff@v1` — Oggetto — costo 1[Fiamma]
- Testo effettivo: Oggetto: assegnalo al tuo Guildmaster. Il Guildmaster ottiene +1 Movimento.
- Origine: adattamento — Core set - fase 4: Runic Staff, oggetto (testo originale basato su regole boardless)
- Serve a testare: Oggetto di posizionamento per un Guildmaster a distanza
- Motivazione dei valori: +1 movimento permanente
- Regole richieste: type.Item, target.none, cost.resources, effect.mod, stat.move, duration.permanent
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

## Gilda verde

### Senya Windseeker
- ID e versione: `gm_senya@v1` — Guildmaster — costo — — Attacco 2, Salute 20, ranged
- Testo effettivo: A distanza. Attacco 2, Salute 20.
Abilità, una volta per turno, anche in risposta (costo: 1 risorsa verde): Infligge 1 danni a un'unità nemica in linea di vista. Cura 1 a una tua unità in linea di vista (qui: il tuo Guildmaster).
- Origine: adattamento — reference/Guild References/Spoiler - green/Senya Windseeker
- Serve a testare: Ciclo base e abilità propria del Guildmaster, una volta per turno
- Motivazione dei valori: Attacco 2, Salute 20, portata e abilità presi dalla carta spoiler. Costo dell'abilità: 1 risorsa della gilda (il simbolo prima dei due punti).
- Regole richieste: type.Guildmaster, target.none, range.ranged, ability.activated, effect.damage, effect.healOwn
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1
- Modello del token: models/senya.obj

### Essenza verde
- ID e versione: `res_green@v1` — Risorsa — costo —
- Testo effettivo: Resta in gioco. All'inizio del tuo turno produce 1 risorsa verde.
- Origine: nuova proposta — carta di test proposta
- Serve a testare: Modello di risorse F2-09 (ipotesi H1)
- Motivazione dei valori: 1 risorsa a turno: il minimo che fa funzionare F2-09
- Regole richieste: type.Resource, target.none
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Field Dressing (test)
- ID e versione: `g_field_dressing@v1` — Azione — costo 1[Arco]
- Testo effettivo: Cura 4 a una tua unità in linea di vista (mai oltre la Salute massima).
- Origine: adattamento — Core set - fase 4: Field Dressing, cura (testo originale basato su regole boardless)
- Serve a testare: Cure fino al massimo (F1-67) contro danni persistenti (F1-34)
- Motivazione dei valori: 4 = due attacchi base annullati per 2 risorse
- Regole richieste: type.Action, target.own, cost.resources, effect.heal
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Regrowth (test)
- ID e versione: `g_regrowth@v1` — Azione istantanea — costo [Arco]
- Testo effettivo: Istantanea. Cura 2 a una tua unità in linea di vista (mai oltre la Salute massima).
- Origine: adattamento — Core set - fase 4: Regrowth, cura (testo originale basato su regole boardless)
- Serve a testare: Cura a velocità istantanea
- Motivazione dei valori: 2 = un attacco base
- Regole richieste: type.Instant, target.own, cost.resources, effect.heal
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Hunter's Net (test)
- ID e versione: `g_hunters_net@v1` — Azione — costo 1[Arco]
- Testo effettivo: Un'unità nemica in linea di vista non può muoversi fino alla fine del prossimo turno del suo proprietario.
- Origine: adattamento — Core set - fase 4: Hunter's Net, Root (CS-21) (testo originale basato su regole boardless)
- Serve a testare: Root contro movimento obbligatorio: l'obbligo decade e il nemico non attacca
- Motivazione dei valori: 2 risorse: nega un intero turno di movimento e quindi di combattimento
- Regole richieste: type.Action, target.enemy, cost.resources, effect.root
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Hunter's Bow (test)
- ID e versione: `g_hunters_bow@v1` — Oggetto — costo 1[Arco]
- Testo effettivo: Oggetto: assegnalo al tuo Guildmaster. Il Guildmaster ottiene +1 Attacco.
- Origine: adattamento — Core set - fase 4: Hunter's Bow, oggetto (testo originale basato su regole boardless)
- Serve a testare: Oggetto d'attacco su un Guildmaster a distanza
- Motivazione dei valori: +1 permanente
- Regole richieste: type.Item, target.none, cost.resources, effect.mod, stat.attack, duration.permanent
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

## Gilda rossa

### Fiaran Rushblood
- ID e versione: `gm_fiaran@v1` — Guildmaster — costo — — Attacco 2, Salute 20, melee
- Testo effettivo: Corpo a corpo. Attacco 2, Salute 20.
Abilità, una volta per turno, anche in risposta (costo: 1 risorsa rossa): Scegli una casella in linea di vista: infligge 2 danni a tutte le unità in una Croce (quella casella e le 4 adiacenti in orizzontale e verticale).
- Origine: adattamento — reference/Guild References/Spoilers - red/Fiaran Rushblood
- Serve a testare: Ciclo base e abilità propria del Guildmaster, una volta per turno
- Motivazione dei valori: Attacco 2, Salute 20, portata e abilità presi dalla carta spoiler. Costo dell'abilità: 1 risorsa della gilda (il simbolo prima dei due punti).
- Regole richieste: type.Guildmaster, target.none, range.melee, ability.activated, effect.damageCross
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1
- Modello del token: models/fiaran.obj

### Essenza rossa
- ID e versione: `res_red@v1` — Risorsa — costo —
- Testo effettivo: Resta in gioco. All'inizio del tuo turno produce 1 risorsa rossa.
- Origine: nuova proposta — carta di test proposta
- Serve a testare: Modello di risorse F2-09 (ipotesi H1)
- Motivazione dei valori: 1 risorsa a turno: il minimo che fa funzionare F2-09
- Regole richieste: type.Resource, target.none
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Thermal Surge (test)
- ID e versione: `r_thermal_surge@v1` — Azione — costo 1[Bomba]
- Testo effettivo: Infligge 3 danni a un'unità in linea di vista.
- Origine: adattamento — Core set - fase 4: Thermal Surge, 3 danni a un'unità (testo originale basato su regole boardless)
- Serve a testare: Danni diretti (F2-29) e bersaglio in linea di vista
- Motivazione dei valori: 3 danni per 2 risorse: più di un attacco base (2), ma solo a velocità normale
- Regole richieste: type.Action, target.any, cost.resources, effect.damage
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Furia della Fucina (test)
- ID e versione: `r_fury@v1` — Azione istantanea — costo [Bomba]
- Testo effettivo: Istantanea. Il tuo Guildmaster ottiene +2 Attacco fino a fine turno.
- Origine: nuova proposta — carta di test proposta
- Serve a testare: Modificatore di attacco (F1-68) giocato prima o in risposta al proprio movimento
- Motivazione dei valori: +2 raddoppia il danno del combattimento automatico per un turno
- Regole richieste: type.Instant, target.self, cost.resources, effect.mod, stat.attack, duration.turn
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Rincorsa (test)
- ID e versione: `r_rush@v1` — Azione — costo [Bomba]
- Testo effettivo: Il tuo Guildmaster ottiene +2 Movimento fino a fine turno.
- Origine: nuova proposta — carta di test proposta
- Serve a testare: Modificatore di movimento (F1-22): raggiungere il contatto in mischia
- Motivazione dei valori: Da 3 a 5 caselle per un turno: velocità, identità rossa
- Regole richieste: type.Action, target.self, cost.resources, effect.mod, stat.move, duration.turn
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Flame Gauntlet (test)
- ID e versione: `r_flame_gauntlet@v1` — Oggetto — costo 1[Bomba]
- Testo effettivo: Oggetto: assegnalo al tuo Guildmaster. Il Guildmaster ottiene +1 Attacco.
- Origine: adattamento — Core set - fase 4: Flame Gauntlet, oggetto (testo originale basato su regole boardless)
- Serve a testare: Oggetti assegnati al Guildmaster (F1-11/12)
- Motivazione dei valori: +1 permanente per 2 risorse; cumulabile (F1-12)
- Regole richieste: type.Item, target.none, cost.resources, effect.mod, stat.attack, duration.permanent
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

## Gilda gialla

### Dalia Shadowstep
- ID e versione: `gm_dalia@v1` — Guildmaster — costo — — Attacco 2, Salute 20, melee
- Testo effettivo: Corpo a corpo. Attacco 2, Salute 20. Venomous: quando infligge danni a un'unità le mette un segnalino veleno; un'unità avvelenata subisce 1 danno all'inizio del turno del suo proprietario.
Abilità, una volta per turno, anche in risposta (costo: 1 risorsa gialla): Infligge 1 danni a un'unità in linea di vista.
- Origine: adattamento — reference/Guild References/Spoilers - Yellow/Dalia Shadowstep
- Serve a testare: Ciclo base e abilità propria del Guildmaster, una volta per turno
- Motivazione dei valori: Attacco 2, Salute 20, portata e abilità presi dalla carta spoiler. Costo dell'abilità: 1 risorsa della gilda (il simbolo prima dei due punti).
- Regole richieste: type.Guildmaster, target.none, range.melee, keyword.venomous, ability.activated, effect.damage
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1
- Modello del token: models/daalia.obj

### Essenza gialla
- ID e versione: `res_yellow@v1` — Risorsa — costo —
- Testo effettivo: Resta in gioco. All'inizio del tuo turno produce 1 risorsa gialla.
- Origine: nuova proposta — carta di test proposta
- Serve a testare: Modello di risorse F2-09 (ipotesi H1)
- Motivazione dei valori: 1 risorsa a turno: il minimo che fa funzionare F2-09
- Regole richieste: type.Resource, target.none
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Smoke Step (test)
- ID e versione: `y_smoke_step@v1` — Azione istantanea — costo [Stivale]
- Testo effettivo: Istantanea. Sposta il tuo Guildmaster fino a 2 caselle.
- Origine: adattamento — Core set - fase 4: Smoke Step, schivata gratuita (testo originale basato su regole boardless)
- Serve a testare: Spostamento in risposta con una carta nel turno avversario, quando l'unità è tappata
- Motivazione dei valori: Costa 1 invece di 0: quattro schivate gratuite toglierebbero senso al test
- Regole richieste: type.Instant, target.self, cost.resources, effect.reposition
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Toxic Shiv (test)
- ID e versione: `y_toxic_shiv@v1` — Azione istantanea — costo [Stivale]
- Testo effettivo: Istantanea. Infligge 1 danni a un'unità nemica in linea di vista.
- Origine: adattamento — Core set - fase 4: Toxic Shiv, 1 danno + veleno (testo originale basato su regole boardless)
- Serve a testare: Danno istantaneo economico
- Motivazione dei valori: Il veleno è rinviato: resta solo il danno
- Regole richieste: type.Instant, target.enemy, cost.resources, effect.damage
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Polvere accecante (test)
- ID e versione: `y_blinding_dust@v1` — Azione istantanea — costo [Stivale]
- Testo effettivo: Istantanea. Un'unità nemica in linea di vista ottiene -2 Attacco fino a fine turno.
- Origine: nuova proposta — carta di test proposta
- Serve a testare: Risposta al movimento che riduce il danno del combattimento automatico
- Motivazione dei valori: -2 azzera un attacco base per un turno
- Regole richieste: type.Instant, target.enemy, cost.resources, effect.mod, stat.attack, duration.turn
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

### Venom Blades (test)
- ID e versione: `y_venom_blades@v1` — Oggetto — costo 1[Stivale]
- Testo effettivo: Oggetto: assegnalo al tuo Guildmaster. Il Guildmaster ottiene +1 Attacco.
- Origine: adattamento — Core set - fase 4: Venom Blades, oggetto (testo originale basato su regole boardless)
- Serve a testare: Oggetto d'attacco su un Guildmaster in mischia
- Motivazione dei valori: +1 permanente; il veleno è rinviato
- Regole richieste: type.Item, target.none, cost.resources, effect.mod, stat.attack, duration.permanent
- Preset compatibili: P01-9x7, P01, P01b, P01d, P01e, P01g, P01h, P01i, P01j, P01k, P01l, P01m, P01n, P01l-copia1

## Mazzi

- **Mazzo blu di prova (Wizards)** (`deck_blue` v1), Guildmaster Nebros Clearmind: 9× Essenza blu, 4× Blink Step (test), 4× Dardo arcano (test), 4× Gelo (test), 4× Runic Staff (test)
- **Mazzo verde di prova (Hunters)** (`deck_green` v1), Guildmaster Senya Windseeker: 9× Essenza verde, 4× Field Dressing (test), 4× Regrowth (test), 4× Hunter's Net (test), 4× Hunter's Bow (test)
- **Mazzo rosso di prova (Artificers)** (`deck_red` v1), Guildmaster Fiaran Rushblood: 9× Essenza rossa, 4× Thermal Surge (test), 4× Furia della Fucina (test), 4× Rincorsa (test), 4× Flame Gauntlet (test)
- **Mazzo giallo di prova (Rogues)** (`deck_yellow` v1), Guildmaster Dalia Shadowstep: 9× Essenza gialla, 4× Smoke Step (test), 4× Toxic Shiv (test), 4× Polvere accecante (test), 4× Venom Blades (test)
