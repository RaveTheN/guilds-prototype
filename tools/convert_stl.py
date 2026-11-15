"""Converte i quattro STL dei Guildmaster in OBJ leggeri per il tema 'semplice' (richiede numpy).
Gli originali in reference/ non vengono toccati. Uso: python tools/convert_stl.py [celle_griglia=56]
- riduzione poligoni: vertex clustering su griglia;
- assi: STL Z-up -> Unity Y-up; base a y=0, centrato; scala: sta in una casella (ingombro <= 0.7, altezza <= 1.3).
Scrive anche docs/logs/stl_conversion.md e copia i simboli di gilda in data/art/symbols."""
import pathlib, shutil, sys
import numpy as np

root = pathlib.Path(__file__).resolve().parent.parent
src = root / "reference" / "Guild References"
out = root / "data" / "themes" / "semplice" / "models"
out.mkdir(parents=True, exist_ok=True)
N = int(sys.argv[1]) if len(sys.argv) > 1 else 56
# Associazione modello -> Guildmaster -> gilda: dal nome del file e dalla cartella della carta spoiler.
MODELS = {"Fiaran": ("fiaran", "Fiaran Rushblood, rosso"), "Senya": ("senya", "Senya Windseeker, verde"),
          "Nebros": ("nebros", "Nebros Clearmind, blu"), "Daalia": ("daalia", "Dalia Shadowstep, giallo (grafia del file: Daalia)")}
log = ["# Conversione STL → OBJ", "", f"Script: `tools/convert_stl.py`, griglia di clustering {N}. Originali invariati in `reference/Guild References/`.", "",
       "| Modello | Guildmaster | Triangoli originali | Triangoli OBJ | Dimensioni originali (x,y,z) | Scala applicata |", "|---|---|---|---|---|---|"]
for name, (oid, who) in MODELS.items():
    raw = (src / f"{name}.stl").read_bytes()
    n = int(np.frombuffer(raw, dtype="<u4", count=1, offset=80)[0])
    assert len(raw) == 84 + n * 50, f"{name}.stl non è un STL binario"
    rec = np.frombuffer(raw, dtype=np.dtype([("n", "<f4", 3), ("v", "<f4", (3, 3)), ("a", "<u2")]), count=n, offset=84)
    v = rec["v"].reshape(-1, 3).astype(np.float64)
    lo, hi = v.min(0), v.max(0); size = hi - lo
    cell = size.max() / N
    key = np.floor((v - lo) / cell).astype(np.int64)
    uniq, inv = np.unique(key, axis=0, return_inverse=True)
    inv = inv.reshape(-1)
    pos = np.zeros((len(uniq), 3)); cnt = np.zeros(len(uniq))
    np.add.at(pos, inv, v); np.add.at(cnt, inv, 1); pos /= cnt[:, None]
    tri = inv.reshape(-1, 3)
    tri = tri[(tri[:, 0] != tri[:, 1]) & (tri[:, 1] != tri[:, 2]) & (tri[:, 0] != tri[:, 2])]
    scale = min(0.7 / max(size[0], size[1]), 1.3 / size[2])
    p = (pos - [(lo[0] + hi[0]) / 2, (lo[1] + hi[1]) / 2, lo[2]]) * scale
    with open(out / f"{oid}.obj", "w", encoding="ascii") as f:
        f.write(f"# {who} - convertito da {name}.stl\n")
        for x, y, z in p: f.write(f"v {x:.4f} {z:.4f} {y:.4f}\n")      # Z-up -> Y-up
        for a, b, c in tri: f.write(f"f {a + 1} {c + 1} {b + 1}\n")     # lo scambio d'assi inverte l'orientamento
    log.append(f"| {name}.stl → {oid}.obj | {who} | {n} | {len(tri)} | {size[0]:.1f} × {size[1]:.1f} × {size[2]:.1f} | {scale:.5f} |")
    print(name, n, "->", len(tri))
log += ["", "Orientamento: asse Z dello STL portato sull'asse Y di Unity; base del modello a y = 0; fronte non verificato sulle fonti (ruotabile dal tema)."]
(root / "docs" / "logs").mkdir(parents=True, exist_ok=True)
(root / "docs" / "logs" / "stl_conversion.md").write_text("\n".join(log) + "\n", encoding="utf-8")

sym = root / "data" / "art" / "symbols"; sym.mkdir(parents=True, exist_ok=True)
for a, b in {"Color symbols/Bomb.jpg": "red_bomb", "Color symbols/magic fire.jpg": "blue_flame", "Color symbols/arrow.jpg": "green_bow",
             "Color symbols/boot.jpg": "yellow_boot", "../dagger.jpg": "yellow_dagger"}.items():
    shutil.copyfile(src / a, sym / (b + ".jpg"))
print("simboli copiati")
