"""Converte reference/fase 4b - mappa.xlsx in data/maps/<id>.json (una per foglio "Mappa ...").
L'originale non viene modificato. Uso: python tools/convert_maps.py   (stampa anche Legenda e Note)"""
import json, pathlib, re, sys, zipfile
import xml.etree.ElementTree as ET

sys.stdout.reconfigure(encoding="utf-8")
M = "{http://schemas.openxmlformats.org/spreadsheetml/2006/main}"
root = pathlib.Path(__file__).resolve().parent.parent
src = root / "reference" / "fase 4b - mappa.xlsx"
z = zipfile.ZipFile(src)
names = re.findall(r'<sheet [^>]*name="([^"]*)"', z.read("xl/workbook.xml").decode("utf-8"))


def cells(i):
    out = {}
    for c in ET.fromstring(z.read(f"xl/worksheets/sheet{i + 1}.xml")).iter(M + "c"):
        txt = "".join(t.text or "" for t in c.iter(M + "t")) or (c.findtext(M + "v") or "")
        m = re.match(r"([A-Z]+)(\d+)", c.get("r"))
        if txt.strip():
            out[(m.group(1), int(m.group(2)))] = txt.strip()
    return out


zones = {}
for (col, row), v in sorted(cells(names.index("Zone")).items(), key=lambda k: k[0][1]):
    pass
zc = cells(names.index("Zone"))
for r in sorted({k[1] for k in zc}):
    if r == 1: continue
    zones.setdefault(zc.get(("A", r)), {})[zc.get(("B", r))] = zc.get(("D", r), "")

VALID = {".", "W", "H", "M", "T1", "T2"}
for i, name in enumerate(names):
    if not name.startswith("Mappa"): continue
    c = cells(i)
    # intestazioni: riga 1 = lettere colonna, colonna A = numeri riga
    cols = [(k[0], v) for k, v in c.items() if k[1] == 1]
    rows = [(k[1], int(v)) for k, v in c.items() if k[0] == "A" and v.isdigit()]
    cols.sort(key=lambda x: x[1]); rows.sort(key=lambda x: -x[1])
    grid = []
    for xr, rank in rows:
        line = []
        for xc, letter in cols:
            v = c.get((xc, xr), "")
            assert v == "" or v in VALID, f"{name} {letter}{rank}: codice sconosciuto {v!r}"
            line.append(v)
        grid.append(line)
    key = name.split()[-1]
    mid = "mappa_" + key
    out = {"id": mid, "name": name, "source": f"reference/{src.name} — foglio '{name}'", "baseMap": None, "changedCells": [],
           "width": len(cols), "height": len(rows),
           "legend": {".": "normale", "W": "muro", "H": "highground", "M": "mostro", "J": "jungle", "T1": "Torre G1", "T2": "Torre G2", "": "fuori mappa"},
           "cells": grid,  # prima riga = riga piu' alta; A1 in basso a sinistra
           "explicitZones": zones.get(key, {})}
    (root / "data" / "maps").mkdir(parents=True, exist_ok=True)
    (root / "data" / "maps" / f"{mid}.json").write_text(json.dumps(out, ensure_ascii=False, indent=1), encoding="utf-8")
    print("scritto", mid, f"{len(cols)}x{len(rows)}")
    for line in grid: print("  " + " ".join(f"{v or '_':2}" for v in line))

for sheet in ("Legenda", "Note"):
    print("=====", sheet)
    c = cells(names.index(sheet))
    for r in sorted({k[1] for k in c}):
        print(" | ".join(c[k] for k in sorted(c, key=lambda k: (len(k[0]), k[0])) if k[1] == r))
