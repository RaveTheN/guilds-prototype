#!/usr/bin/env python3
"""Genera data/features/features.json e docs/FEATURES.md.
Input: testo dell'inventario compilato (pandoc -t plain --wrap=none) + data/features/analysis.json.
Uso: python3 tools/build_feature_registry.py <inventario.txt>
Lo stesso features.json verra' letto dal codice C# (registro unico: stato di implementazione e preset).
"""
import json, re, sys, pathlib
sys.stdout.reconfigure(encoding='utf-8')
root = pathlib.Path(__file__).resolve().parent.parent
lines = pathlib.Path(sys.argv[1] if len(sys.argv) > 1 else root/"data/features/inventario_compilato.txt").read_text(encoding="utf-8").splitlines()
analysis = json.loads((root/"data/features/analysis.json").read_text(encoding="utf-8"))
status = json.loads((root/"data/features/impl_status.json").read_text(encoding="utf-8"))
def impl(code, a): return status.get(code) or ("implementato" if any(p.startswith("P01") for p in a.get("presets", [])) else "non implementato")
SECT = {"F1":"Fase 1 - Regolamento originale","F2":"Fase 2 - Riduzione segnalini e interazione","F3":"Fase 3 - Zero permanents",
"F4A":"Fase 4A - Nuovo Guilds","F4B":"Fase 4B - Another Version / Chess","F4C":"Fase 4C - Semi Chess","U":"Fase 4 - Versione unificata",
"BL":"Fase 4 - Ramo boardless","CS":"Fase 4 - Core Set e meccaniche delle carte","F4b":"Fase 4b - Scacchiera, zone e Torri","P":"Prototipi fotografati"}
head = re.compile(r"^(F1|F2|F3|F4A|F4B|F4C|F4b|U|BL|CS|P)-(\d+) Scelta:\s*(.*)$")
feats, i = [], 0
while i < len(lines):
    m = head.match(lines[i])
    if not m: i += 1; continue
    pref, num, raw = m.group(1), m.group(2), m.group(3).strip()
    choice = raw.strip("_ ").upper()            # x == X; vuoto = non deciso
    j = i+1; body=[]
    while j < len(lines) and not lines[j].startswith("Note:"):
        if lines[j].strip(): body.append(lines[j].strip())
        j += 1
    note = lines[j][5:].strip() if j < len(lines) else ""
    if set(note) <= set("_ "): note = ""
    code = f"{pref}-{num}"
    a = analysis["features"].get(code, {})
    feats.append({"code":code,"section":SECT[pref],"description":" ".join(body),
        "choice": choice if choice in ("L","T","X") else "", "choiceRaw": raw.strip("_ "),
        "note":note,"interpretation":a.get("interp",""),
        "dependsOn":a.get("deps"),"incompatibleWith":a.get("incompat"),
        "analyzed": code in analysis["features"],
        "implementation":impl(code, a),
        "presets":a.get("presets",[]),"presetNote":a.get("presetNote","")})
    i = j+1
add = json.loads((root/"data/features/additions.json").read_text(encoding="utf-8"))
for x in add["features"]:
    a = analysis["features"].get(x["code"], {})
    feats.append({"code":x["code"],"section":add["section"],"description":x["description"],"choice":x["choice"],"choiceRaw":x["choice"],
        "note":x["note"],"interpretation":a.get("interp",""),"dependsOn":a.get("deps"),"incompatibleWith":a.get("incompat"),
        "analyzed": x["code"] in analysis["features"],"implementation":impl(x["code"], a),
        "presets":a.get("presets",[]),"presetNote":a.get("presetNote","")})
out = {"source":"Guilds-Feature-da-compilare1.docx (inventario 26 settembre 2026, compilato)","count":len(feats),"inventoryCount":len(feats)-len(add["features"]),
       "conflicts":analysis["conflicts"],"features":feats}
(root/"data/features/features.json").write_text(json.dumps(out,ensure_ascii=False,indent=1),encoding="utf-8")

def cell(s): return (s or "").replace("|","\\|").replace("\n"," ")
def lst(v): return "non analizzato" if v is None else (", ".join(v) if v else "nessuna rilevata")
md = ["# Tabella delle feature — Guilds prototipo","",
 "Generata da `tools/build_feature_registry.py` a partire da `data/features/features.json`. Non modificare a mano.","",
 f"Voci: **{len(feats)}** ({len(feats)-len(add['features'])} dell'inventario + {len(add['features'])} aggiunte N-xx). Scelta: L = mantenere, T = testare, X = esclusa, (vuoto) = non deciso.",
 "Dipendenze e incompatibilità sono analizzate solo per il primo preset e per le aree critiche; altrove: \"non analizzato\".",
 "Stato di implementazione: da `data/features/impl_status.json` (aggiornato a M4, famiglia di preset P01).",""]
from collections import Counter
c = Counter(f["choice"] or "vuoto" for f in feats)
md += ["## Riepilogo scelte","","| Scelta | Voci |","|---|---|"]+[f"| {k} | {c[k]} |" for k in ("L","T","X","vuoto")]+[""]
md += ["## Conflitti fra L e nodi critici","","| ID | Tipo | Codici | Descrizione | Trattamento |","|---|---|---|---|---|"]
for k in analysis["conflicts"]:
    md.append(f"| {k['id']} | {k['kind']} | {', '.join(k['codes'])} | {cell(k['text'])} | {cell(k['handling'])} |")
md.append("")
cur=None
for f in feats:
    if f["section"]!=cur:
        cur=f["section"]; md += [f"## {cur}","","| Codice | Descrizione | Scelta | Note / interpretazione | Dipendenze | Incompatibilità | Stato impl. | Preset |","|---|---|---|---|---|---|---|---|"]
    note = f["note"]
    if f["interpretation"]: note = (note+" — " if note else "")+"**Interpretazione:** "+f["interpretation"]
    pres = ", ".join(f["presets"]) + (f" ({f['presetNote']})" if f["presetNote"] else "")
    md.append(f"| {f['code']} | {cell(f['description'])} | {f['choice'] or '(vuoto)'} | {cell(note)} | {lst(f['dependsOn'])} | {lst(f['incompatibleWith'])} | {f['implementation']} | {pres or '—'} |")
(root/"docs/FEATURES.md").write_text("\n".join(md)+"\n",encoding="utf-8")
print(len(feats), dict(c))
