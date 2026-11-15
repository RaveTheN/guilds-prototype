"""Stampa celle (valore + colore di riempimento) di ogni foglio di un .xlsx, senza dipendenze.
Uso: python tools/dump_xlsx.py <file.xlsx>"""
import re, sys, zipfile
import xml.etree.ElementTree as ET

NS = {"m": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}
z = zipfile.ZipFile(sys.argv[1])
shared = []
if "xl/sharedStrings.xml" in z.namelist():
    for si in ET.fromstring(z.read("xl/sharedStrings.xml")).findall("m:si", NS):
        shared.append("".join(t.text or "" for t in si.iter("{%s}t" % NS["m"])))
styles = ET.fromstring(z.read("xl/styles.xml"))
fills = []
for f in styles.find("m:fills", NS):
    fg = f.find("m:patternFill/m:fgColor", NS)
    fills.append(dict(fg.attrib) if fg is not None else {})
xf_fill = [int(x.get("fillId", 0)) for x in styles.find("m:cellXfs", NS)]
print("workbook:", re.findall(r'<sheet [^>]*name="([^"]*)"', z.read("xl/workbook.xml").decode("utf-8")))
for n in sorted(x for x in z.namelist() if x.startswith("xl/worksheets/sheet")):
    print("=====", n)
    root = ET.fromstring(z.read(n))
    for mc in root.iter("{%s}mergeCell" % NS["m"]):
        print("merge", mc.get("ref"))
    for c in root.iter("{%s}c" % NS["m"]):
        v = c.find("m:v", NS)
        val = "" if v is None else (shared[int(v.text)] if c.get("t") == "s" else v.text)
        if c.get("t") == "inlineStr":
            val = "".join(t.text or "" for t in c.iter("{%s}t" % NS["m"]))
        fill = fills[xf_fill[int(c.get("s", 0))]]
        if val or fill:
            print(c.get("r"), repr(val), fill)
