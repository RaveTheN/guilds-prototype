"""Genera data/cards, data/decks, data/presets e il tema 'semplice'. Uso: python tools/gen_content.py
Le carte sono CARTE DI TEST PROPOSTE (non canon). Modifica qui valori e testi, poi rilancia."""
import json, pathlib

root = pathlib.Path(__file__).resolve().parent.parent / "data"
for d in ("cards", "decks", "presets", "themes/semplice/models", "art/cards", "art/symbols"):
    (root / d).mkdir(parents=True, exist_ok=True)


def card(id, name, guild, type, cc=0, cg=0, target="none", effects=(), origin=("nuova proposta", ""), tests="", why="", **kw):
    d = dict(id=id, version=1, name=name, guild=guild, type=type, costColored=cc, costGeneric=cg, target=target,
             effects=[dict(zip(("kind", "amount", "stat", "duration"), e)) for e in effects],
             originKind=origin[0], originSource=origin[1], tests=tests, rationale=why)
    d.update(kw)
    return d


def gm(id, name, guild, rng, folder, ability, model, keywords=()):
    target, effects, aname = ability
    return card(id, name, guild, "Guildmaster", origin=("adattamento", f"reference/Guild References/{folder}/{name}"),
                tests="Ciclo base e abilità propria del Guildmaster, una volta per turno",
                why="Attacco 2, Salute 20, portata e abilità presi dalla carta spoiler. Costo dell'abilità: 1 risorsa della gilda (il simbolo prima dei due punti).",
                attack=2, health=20, range=rng, altAttack=1, tokenModel=f"models/{model}.obj", keywords=list(keywords),
                ability=dict(name=aname, costColored=1, target=target, effects=[dict(zip(("kind", "amount"), e)) for e in effects]))


def ally(id, name, guild, cc, cg, attack, health, rng, role, tests, why, **kw):
    return card(id, name, guild, "Ally", cc, cg, origin=NEW_ALLY, tests=tests, why=why, attack=attack, health=health, range=rng, role=role, **kw)


NEW_ALLY = ("nuova proposta", "alleato di test proposto (nessuna fonte ne fissa i valori)")


def ADAPT(n): return ("adattamento", f"Core set - fase 4: {n} (testo originale basato su regole boardless)")


NEW = ("nuova proposta", "carta di test proposta")
RES = dict(origin=NEW, tests="Modello di risorse F2-09 (ipotesi H1)", why="1 risorsa a turno: il minimo che fa funzionare F2-09")
G = {
    "Red": [
        gm("gm_fiaran", "Fiaran Rushblood", "Red", "melee", "Spoilers - red", ("none", [("damageCross", 2)], "Bomba di Fiaran"), "fiaran"),
        card("res_red", "Essenza rossa", "Red", "Resource", **RES),
        card("r_thermal_surge", "Thermal Surge (test)", "Red", "Action", 1, 1, "any", [("damage", 3)], ADAPT("Thermal Surge, 3 danni a un'unità"), "Danni diretti (F2-29) e bersaglio in linea di vista", "3 danni per 2 risorse: più di un attacco base (2), ma solo a velocità normale"),
        card("r_fury", "Furia della Fucina (test)", "Red", "Instant", 1, 0, "self", [("mod", 2, "attack", "turn")], NEW, "Modificatore di attacco (F1-68) giocato prima o in risposta al proprio movimento", "+2 raddoppia il danno del combattimento automatico per un turno"),
        card("r_rush", "Rincorsa (test)", "Red", "Action", 1, 0, "self", [("mod", 2, "move", "turn")], NEW, "Modificatore di movimento (F1-22): raggiungere il contatto in mischia", "Da 3 a 5 caselle per un turno: velocità, identità rossa"),
        card("r_flame_gauntlet", "Flame Gauntlet (test)", "Red", "Item", 1, 1, "none", [("mod", 1, "attack", "permanent")], ADAPT("Flame Gauntlet, oggetto"), "Oggetti assegnati al Guildmaster (F1-11/12)", "+1 permanente per 2 risorse; cumulabile (F1-12)"),
        ally("r_forge_recruit", "Recluta della Fucina (test)", "Red", 1, 1, 2, 3, "melee", "rook", "Alleato in mischia robusto: evocazione, obbligo di movimento con più unità, Assalto", "2/3 per 2 risorse: regge un attacco base"),
        ally("r_bomber", "Bombarolo (test)", "Red", 1, 0, 1, 2, "ranged", "bishop", "Alleato a distanza con Swift (F1-62)", "1/2 per 1 risorsa: fragile ma agisce subito", keywords=["swift"]),
        ally("r_shieldbearer", "Portascudo (test)", "Red", 1, 1, 1, 5, "melee", "pawn", "Alleato difensivo: molta Salute, poco Attacco; blocca passaggi e linee di vista", "1/5 per 2 risorse: regge due attacchi base"),
        ally("r_fire_lancer", "Lanciere di fiamma (test)", "Red", 1, 2, 3, 3, "melee", "queen", "Alleato costoso con Attacco superiore a quello di un Guildmaster", "3/3 per 3 risorse"),
    ],
    "Blue": [
        gm("gm_nebros", "Nebros Clearmind", "Blue", "ranged", "Spoilers - blue", ("any", [("reposition", 2)], "Fiamma di Nebros"), "nebros"),
        card("res_blue", "Essenza blu", "Blue", "Resource", **RES),
        card("b_blink_step", "Blink Step (test)", "Blue", "Instant", 1, 1, "any", [("reposition", 2)], ADAPT("Blink Step, sposta un'unità"), "Spostamento in risposta con carte, anche del nemico: rompe contatto o linea di vista (F2-19)", "2 caselle come l'abilità stampata di Nebros; 2 risorse perché può annullare un combattimento"),
        card("b_arcane_dart", "Dardo arcano (test)", "Blue", "Instant", 1, 1, "enemy", [("damage", 2)], NEW, "Danno diretto istantaneo e ricontrollo del bersaglio (F3-11)", "2 danni a velocità istantanea per 2 risorse"),
        card("b_frost", "Gelo (test)", "Blue", "Action", 1, 0, "enemy", [("mod", -2, "move", "targetNextTurn")], NEW, "Controllo del movimento avversario senza bloccarlo (confronto con Root)", "Il nemico muove di 1 nel suo prossimo turno"),
        card("b_runic_staff", "Runic Staff (test)", "Blue", "Item", 1, 1, "none", [("mod", 1, "move", "permanent")], ADAPT("Runic Staff, oggetto"), "Oggetto di posizionamento per un Guildmaster a distanza", "+1 movimento permanente"),
        ally("b_rune_golem", "Golem runico (test)", "Blue", 1, 1, 2, 4, "melee", "rook", "Alleato in mischia che fa da scudo a un Guildmaster a distanza (le unità nemiche bloccano la linea di vista, F1-37)", "2/4 per 2 risorse"),
        ally("b_apprentice", "Apprendista arcano (test)", "Blue", 1, 0, 1, 2, "ranged", "bishop", "Secondo tiratore: scelta del bersaglio con più nemici a portata", "1/2 per 1 risorsa"),
        ally("b_frost_sentinel", "Sentinella di brina (test)", "Blue", 1, 2, 2, 5, "melee", "pawn", "Alleato difensivo costoso", "2/5 per 3 risorse"),
        ally("b_spark_adept", "Adepta della scintilla (test)", "Blue", 1, 1, 2, 2, "ranged", "queen", "Alleato a distanza con attacco pieno", "2/2 per 2 risorse"),
    ],
    "Green": [
        gm("gm_senya", "Senya Windseeker", "Green", "ranged", "Spoiler - green", ("enemy", [("damage", 1), ("healOwn", 1)], "Freccia di Senya"), "senya"),
        card("res_green", "Essenza verde", "Green", "Resource", **RES),
        card("g_field_dressing", "Field Dressing (test)", "Green", "Action", 1, 1, "own", [("heal", 4)], ADAPT("Field Dressing, cura"), "Cure fino al massimo (F1-67) contro danni persistenti (F1-34)", "4 = due attacchi base annullati per 2 risorse"),
        card("g_regrowth", "Regrowth (test)", "Green", "Instant", 1, 0, "own", [("heal", 2)], ADAPT("Regrowth, cura"), "Cura a velocità istantanea", "2 = un attacco base"),
        card("g_hunters_net", "Hunter's Net (test)", "Green", "Action", 1, 1, "enemy", [("root", 0)], ADAPT("Hunter's Net, Root (CS-21)"), "Root contro movimento obbligatorio: l'obbligo decade e il nemico non attacca", "2 risorse: nega un intero turno di movimento e quindi di combattimento"),
        card("g_hunters_bow", "Hunter's Bow (test)", "Green", "Item", 1, 1, "none", [("mod", 1, "attack", "permanent")], ADAPT("Hunter's Bow, oggetto"), "Oggetto d'attacco su un Guildmaster a distanza", "+1 permanente"),
        ally("g_hound", "Segugio (test)", "Green", 1, 0, 2, 2, "melee", "knight", "Alleato veloce con Swift; col ruolo Cavallo salta gli ostacoli", "2/2 per 1 risorsa: colpisce subito ma muore a un attacco base", keywords=["swift"]),
        ally("g_wood_archer", "Arciere dei boschi (test)", "Green", 1, 1, 2, 2, "ranged", "bishop", "Alleato a distanza con attacco pieno", "2/2 per 2 risorse"),
        ally("g_bear", "Orso dei boschi (test)", "Green", 1, 2, 3, 4, "melee", "pawn", "Alleato grosso in mischia: Assalto e scambi ripetuti", "3/4 per 3 risorse"),
        ally("g_scout", "Esploratrice (test)", "Green", 1, 0, 1, 1, "ranged", "queen", "Alleato minimo a distanza con Swift: muore a qualsiasi danno", "1/1 per 1 risorsa", keywords=["swift"]),
    ],
    "Yellow": [
        gm("gm_dalia", "Dalia Shadowstep", "Yellow", "melee", "Spoilers - Yellow", ("any", [("damage", 1)], "Lama di Dalia"), "daalia", keywords=("venomous",)),
        card("res_yellow", "Essenza gialla", "Yellow", "Resource", **RES),
        card("y_smoke_step", "Smoke Step (test)", "Yellow", "Instant", 1, 0, "self", [("reposition", 2)], ADAPT("Smoke Step, schivata gratuita"), "Spostamento in risposta con una carta nel turno avversario, quando l'unità è tappata", "Costa 1 invece di 0: quattro schivate gratuite toglierebbero senso al test"),
        card("y_toxic_shiv", "Toxic Shiv (test)", "Yellow", "Instant", 1, 0, "enemy", [("damage", 1)], ADAPT("Toxic Shiv, 1 danno + veleno"), "Danno istantaneo economico", "Il veleno è rinviato: resta solo il danno"),
        card("y_blinding_dust", "Polvere accecante (test)", "Yellow", "Instant", 1, 0, "enemy", [("mod", -2, "attack", "turn")], NEW, "Risposta al movimento che riduce il danno del combattimento automatico", "-2 azzera un attacco base per un turno"),
        card("y_venom_blades", "Venom Blades (test)", "Yellow", "Item", 1, 1, "none", [("mod", 1, "attack", "permanent")], ADAPT("Venom Blades, oggetto"), "Oggetto d'attacco su un Guildmaster in mischia", "+1 permanente; il veleno è rinviato"),
        ally("y_cutpurse", "Tagliaborse (test)", "Yellow", 1, 0, 1, 2, "melee", "knight", "Alleato economico con Swift", "1/2 per 1 risorsa", keywords=["swift"]),
        ally("y_knife_thrower", "Lanciacoltelli (test)", "Yellow", 1, 1, 1, 3, "ranged", "bishop", "Alleato con doppio profilo (N-05): a distanza 1, in mischia 2", "1/3 per 2 risorse; in mischia vale 2 solo con la variante «doppio profilo»", altAttack=2),
        ally("y_bodyguard", "Guardaspalle (test)", "Yellow", 1, 1, 1, 4, "melee", "pawn", "Alleato difensivo economico", "1/4 per 2 risorse"),
        ally("y_assassin", "Sicario (test)", "Yellow", 1, 2, 3, 2, "melee", "queen", "Alleato fragile con Swift e Attacco alto: colpisce subito", "3/2 per 3 risorse", keywords=["swift"]),
    ],
}
names = {"Red": ("rosso", "Artificers"), "Blue": ("blu", "Wizards"), "Green": ("verde", "Hunters"), "Yellow": ("giallo", "Rogues")}


def dump(path, obj): path.write_text(json.dumps(obj, ensure_ascii=False, indent=1), encoding="utf-8")


for g, cards in G.items():
    dump(root / "cards" / f"{g.lower()}.json", cards)
    dump(root / "decks" / f"deck_{g.lower()}.json", dict(
        id=f"deck_{g.lower()}", name=f"Mazzo {names[g][0]} di prova ({names[g][1]})", version=1, guildmaster=cards[0]["id"],
        cards=[dict(card=cards[1]["id"], count=9)] + [dict(card=c["id"], count=4) for c in cards[2:6]]))
    dump(root / "decks" / f"deck_{g.lower()}_alleati.json", dict(
        id=f"deck_{g.lower()}_alleati", name=f"Mazzo {names[g][0]} con alleati ({names[g][1]})", version=1, guildmaster=cards[0]["id"],
        cards=[dict(card=cards[1]["id"], count=9)] + [dict(card=c["id"], count=2) for c in cards[2:6]] + [dict(card=c["id"], count=2) for c in cards[6:]]))

HYP = [
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
    "ND Entrambi i Guildmaster sconfitti nello stesso scambio: esito non definito (D21); la partita finisce senza vincitore.",
]
LIM = [
    "Test parziale: sottoinsieme delle feature dell'inventario. Non è il regolamento definitivo.",
    "Alleati, Assalto, scacchi, mostri, edifici, risorse fuori dal mazzo e linea di vista a zone esistono solo come varianti, quasi tutte basate su ipotesi (H27–H32). Jungle senza regole: le mappe non hanno caselle Jungle.",
    "Zone personali non definite nella mappa (foglio Zone: «da definire»): i territori sono ricavati per ipotesi (H27). Il divieto di F1-38 fra le zone di evocazione non è applicato.",
    "Il foglio Note dell'Excel descrive una disposizione diversa dalle celle dei fogli Mappa: usate le celle (Torri in A1 e H8, non H1 e A8 come in F4b-11).",
]
DEF = [
    "Token per ruolo o per gilda; assegnazione libera dei pezzi degli scacchi (nota di F4C-01)", "Jungle con regole proprie; reazione dei mostri coi dadi (F4A-16)",
    "Torre da conquistare (F4C-05/06), danno della Torre a raggio (F3-14)", "Edifici come supporto per altre carte, mazzo strutture (F2-08, F4A-03, CS-05..09)",
    "Punti azione e moneta (F4A-04..15, F2-16)", "Parole chiave: Armor, Flying, Taunt, Unique", "Aree d'effetto diverse dalla Croce (F1-39/40)",
    "Magie continuative, esilio, ricerca nel mazzo, dadi", "Scelta manuale del percorso (H18): nessuna regola dipende dal percorso",
]


def preset(id, name, desc, based=None, map="mappa_8x8", numbers=None, **choices):
    return dict(id=id, name=name, basedOn=based, description=desc, featureSet="P01", map=map, theme="semplice", partial=True, builtIn=True,
                choices=choices, numbers=numbers or {}, hypotheses=HYP, limits=LIM, deferred=DEF)


ALLEATI = " Usa i mazzi «con alleati»."


for p in [
    preset("P01", "Duello dei Guildmaster", "Ciclo base: movimento obbligatorio → risposta → combattimento automatico → danni. Un eroe per giocatore."),
    preset("P01-9x7", "Duello dei Guildmaster — mappa 9x7", "Come P01, sulla mappa 9x7 + 2.", "P01", "mappa_9x7"),
    preset("P01b", "Duello — partenza negli angoli (S-A)", "Come P01 con la posizione iniziale U-06. Non avviabile sulle mappe attuali: gli angoli non sono liberi.", "P01", start="S-A"),
    preset("P01m", "Duello — spostamento in risposta col solo tap", "Come P01, ma per spostarsi in risposta basta tappare l'unità, senza scartare (SCH-TAP).", "P01", dodge="SCH-TAP"),
    preset("P01d", "Duello — si combatte se c'è ancora contatto", "Come P01, ma dopo uno spostamento in risposta si ricontrolla contatto / linea di vista: se c'è ancora, si combatte (RIC-GEO).", "P01", recheck="RIC-GEO"),
    preset("P01h", "Duello — restare fermi vale come mossa", "Come P01, ma muoversi nella propria casella è una mossa legale: tappa, soddisfa l'obbligo e fa partire il combattimento.", "P01", stayMove="si"),
    preset("P01i", "Duello — attacco senza movimento", "Come P01, ma un'unità stappata con un bersaglio valido può tapparsi e attaccare senza cambiare casella: il tap vale come movimento.", "P01", attackNoMove="si"),
    preset("P01j", "Duello — linea di tiro che non tocca gli angoli", "Come P01, ma una retta che tocca l'angolo di un muro o di un'unità non amica non vale.", "P01", losLine="angoli-netta"),
    preset("P01k", "Duello — linea di tiro dal centro", "Come P01, ma la linea di tiro è una sola retta fra i centri delle due caselle.", "P01", losLine="centro"),
    preset("P01l", "Duello — il veleno cala", "Come P01, ma i segnalini veleno calano di uno ogni volta che fanno danno, cioè all'inizio del turno dell'unità avvelenata.", "P01", venom="cala"),
    preset("P01n", "Duello — Torri attive", "Come P01, con tutte le regole delle Torri accese: cura e danno all'ingresso e a inizio turno, Torre con punti vita. I valori X si cambiano nei parametri.", "P01",
           towerHealEnter="si", towerHealStart="si", towerDmgEnter="si", towerDmgStart="si", towerHp="si"),
    preset("P01g", "Duello — abilità solo nel proprio turno", "Come P01, ma l'abilità del Guildmaster si usa solo nel proprio turno, non in risposta.", "P01", gmAbilities="turno"),
    preset("P01e", "Duello — movimento facoltativo", "Come P01, ma muovere non è obbligatorio.", "P01", mandatoryMove="no"),
    preset("P01o", "Duello — vince chi distrugge la Torre", "Come P01, con la Torre a punti vita: chi porta a 0 la Torre avversaria vince.", "P01", towerHp="si", towerWin="si"),
    preset("P02", "Alleati", "Come P01, con gli alleati: si evocano nel proprio territorio, entrano tappati, al massimo 5 miniature per giocatore." + ALLEATI, "P01", units="alleati"),
    preset("P02a", "Alleati — vita ripristinata e Assalto", "Come P02: la vita degli alleati torna piena a ogni inizio turno e si può muovere sulla casella di un alleato nemico per l'Assalto (N-01..N-04)." + ALLEATI, "P02", units="alleati", allyLife="ripristinata", assault="alleati"),
    preset("P02b", "Alleati — doppio profilo d'attacco", "Come P02: le unità col secondo profilo attaccano sia in mischia sia a distanza (N-05)." + ALLEATI, "P02", units="alleati", attackProfile="doppio"),
    preset("P02c", "Alleati — ogni unità deve muovere", "Come P02, ma l'obbligo di movimento vale per ogni unità che può muovere (OBB-B)." + ALLEATI, "P02", units="alleati", obligation="OBB-B"),
    preset("P03", "Scacchi — ogni unità col suo pezzo", "Come P02, ma ogni unità muove come il pezzo degli scacchi scritto sulla carta; il Guildmaster come il Re; un solo alleato per tipo." + ALLEATI, "P02", units="alleati", moveStyle="ruoli"),
    preset("P03b", "Scacchi — pezzo scelto all'ingresso", "Come P03, ma ogni alleato sceglie il pezzo quando entra, fra quelli a disposizione del giocatore: 1 Cavallo, 1 Alfiere, 1 Regina, 1 Torre, 2 Pedoni. Il Re è del Guildmaster, che qui muove come il Re." + ALLEATI, "P03", units="alleati", moveStyle="scelta"),
    preset("P03a", "Scacchi — Guildmaster come il Re", "Come P01, ma il Guildmaster muove di una sola casella in qualsiasi direzione (F4B-04).", "P01", moveStyle="re"),
    preset("P04", "Mostri attivi", "Come P01, ma i mostri hanno punti vita e si attaccano: sono di tre tipi (cura, attacco, pesca), pescati da un mazzo di 10 carte; chi ne sconfigge 2 vince.", "P01", numbers=dict(monsterWin=2), monsters="attivi"),
    preset("P05", "Risorse fisse fuori dal mazzo", "Come P01, ma ogni giocatore ha 3 risorse fisse dal primo turno e le carte risorsa escono dai mazzi. Con i mazzi più corti il mazzo esaurito non fa perdere.", "P01", resourceModel="fisse", deckOut="salta"),
    preset("P05a", "Risorse crescenti fuori dal mazzo", "Come P05, ma si riceve una risorsa in più a ogni proprio turno, fino a 3.", "P05", resourceModel="crescente", deckOut="salta"),
    preset("P06", "Edifici", "Come P01, ma le carte risorsa in gioco sono edifici con 2 punti Struttura: si distruggono con i danni di carte e abilità.", "P01", buildings="si"),
    preset("P07", "Linea di vista a zone", "Come P01, ma si vede solo chi sta nella propria zona o in una zona confinante (LOS-Z).", "P01", los="LOS-Z"),
]:
    dump(root / "presets" / f"{p['id']}.json", p)

dump(root / "themes" / "semplice" / "theme.json", dict(
    name="semplice", description="Geometrie semplici generate da codice.",
    colors={".": "#D8D2C4", "alt": "#C4BEB0", "W": "#3A3A3A", "H": "#A9794F", "J": "#4F8A4F", "M": "#7A4FA9", "T1": "#EEEEEE", "T2": "#EEEEEE"},
    heights={".": 0.1, "W": 0.9, "H": 0.3, "J": 0.1, "M": 0.3, "T1": 0.1, "T2": 0.1},
    guildColors={"Red": "#D0262B", "Blue": "#2F6FD6", "Green": "#2E9A45", "Yellow": "#E0B020"},
    models={}))
print("contenuti generati")
