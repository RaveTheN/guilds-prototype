// Livello visivo sostituibile: tema (colori, altezze, modelli), illustrazioni esterne, board 3D e token.
// Niente regole qui: legge soltanto Guilds.Core.
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Guilds.Core;
using UnityEngine;

namespace Guilds.Unity
{
    public class ThemeData
    {
        public string Name, Description;
        public Dictionary<string, string> Colors = new Dictionary<string, string>(), GuildColors = new Dictionary<string, string>(), Models = new Dictionary<string, string>();
        public Dictionary<string, float> Heights = new Dictionary<string, float>();
    }

    /// <summary>File esterni collegati per ID. Se un file manca si usa un segnaposto generato.</summary>
    public class Assets
    {
        public ThemeData Theme; readonly string dataRoot, themeDir;
        readonly Dictionary<string, Texture2D> tex = new Dictionary<string, Texture2D>();
        readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

        public static List<string> ThemeNames(string dataRoot) =>
            Directory.GetDirectories(Path.Combine(dataRoot, "themes")).Where(d => File.Exists(Path.Combine(d, "theme.json"))).Select(Path.GetFileName).OrderBy(n => n).ToList();

        public Assets(string dataRoot, string theme)
        {
            this.dataRoot = dataRoot; themeDir = Path.Combine(dataRoot, "themes", theme);
            if (!File.Exists(Path.Combine(themeDir, "theme.json"))) themeDir = Path.Combine(dataRoot, "themes", "semplice");
            Theme = Json.Read<ThemeData>(Path.Combine(themeDir, "theme.json"));
        }

        static Color Hex(string s, Color fallback) => s != null && ColorUtility.TryParseHtmlString(s, out var c) ? c : fallback;
        public Color CellColor(string code) => Hex(Theme.Colors.TryGetValue(code, out var s) ? s : null, Color.magenta);
        public Color GuildColor(Guild g) => Hex(Theme.GuildColors.TryGetValue(g.ToString(), out var s) ? s : null, Color.gray);
        public float Height(string code) => Theme.Heights.TryGetValue(code, out var h) ? h : 0.1f;

        public Texture2D Tex(string relativeNoExt)
        {
            if (tex.TryGetValue(relativeNoExt, out var t)) return t;
            foreach (var ext in new[] { ".png", ".jpg" })
            {
                var f = Path.Combine(dataRoot, relativeNoExt + ext);
                if (!File.Exists(f)) continue;
                t = new Texture2D(2, 2); t.LoadImage(File.ReadAllBytes(f)); break;
            }
            return tex[relativeNoExt] = t;
        }
        public Texture2D Symbol(string id) => Tex("art/symbols/" + id);
        public Texture2D CardArt(string id) => Tex("art/cards/" + id);
        public Texture2D CardBack => Tex("art/cardback");

        /// <summary>Modello OBJ relativo alla cartella del tema (solo v/f). Null se manca: chi chiama usa la geometria semplice.</summary>
        public Mesh Model(string relative)
        {
            if (string.IsNullOrEmpty(relative)) return null;
            if (meshes.TryGetValue(relative, out var m)) return m;
            var f = Path.Combine(themeDir, relative);
            if (File.Exists(f))
            {
                var v = new List<Vector3>(); var tri = new List<int>();
                foreach (var line in File.ReadLines(f))
                {
                    var p = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length >= 4 && p[0] == "v") v.Add(new Vector3(F(p[1]), F(p[2]), F(p[3])));
                    else if (p.Length >= 4 && p[0] == "f")
                        for (int i = 2; i + 1 < p.Length; i++) { tri.Add(I(p[1])); tri.Add(I(p[i])); tri.Add(I(p[i + 1])); }
                }
                m = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32, name = relative };
                m.SetVertices(v); m.SetTriangles(tri, 0); m.RecalculateNormals(); m.RecalculateBounds();
            }
            return meshes[relative] = m;
        }
        static float F(string s) => float.Parse(s, CultureInfo.InvariantCulture);
        static int I(string s) => int.Parse(s.Split('/')[0]) - 1;

        public static Material Mat(Color c, bool overlay = false)
        {
            var m = new Material(Shader.Find(overlay ? "Sprites/Default" : "Standard")) { color = c };
            if (!overlay) m.SetFloat("_Glossiness", 0.1f);
            return m;
        }
    }

    public class BoardView
    {
        public readonly GameObject Root = new GameObject("Board");
        Board b; Assets a;
        readonly Dictionary<int, GameObject> tokens = new Dictionary<int, GameObject>();
        readonly List<GameObject> marks = new List<GameObject>();

        static void Kill(Object o) { if (Application.isPlaying) Object.Destroy(o); else Object.DestroyImmediate(o); }
        public Vector3 World(Cell c, float y) => new Vector3(c.X - (b.W - 1) / 2f, y, c.Y - (b.H - 1) / 2f);
        public float Top(Cell c) => a.Height(b.Code(c));
        public float Extent => Mathf.Max(b.W, b.H) + 3; // +3: accanto alla board stanno le carte in gioco

        GameObject Prim(PrimitiveType t, string name, Vector3 pos, Vector3 scale, Color col, bool keepCollider = false, bool overlay = false)
        {
            var go = GameObject.CreatePrimitive(t); go.name = name; go.transform.SetParent(Root.transform, false);
            go.transform.localPosition = pos; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Assets.Mat(col, overlay);
            if (!keepCollider) Kill(go.GetComponent<Collider>());
            return go;
        }

        public void Build(Board board, Assets assets)
        {
            b = board; a = assets;
            foreach (var c in b.All())
            {
                string code = b.Code(c); float h = Top(c);
                var col = code == "." && (c.X + c.Y) % 2 == 0 ? a.CellColor("alt") : a.CellColor(code == "M" ? "H" : code);
                var tile = Prim(PrimitiveType.Cube, "cell:" + c.Name, World(c, h / 2), new Vector3(0.96f, h, 0.96f), col, true);
                // un tema può sostituire la geometria di un tipo di casella con un modello: "models": { "W": "models/muro.obj" }
                var model = a.Theme.Models.TryGetValue(code, out var rel) ? a.Model(rel) : null;
                if (model != null) { tile.GetComponent<MeshFilter>().sharedMesh = model; tile.transform.localPosition = World(c, 0); tile.transform.localScale = Vector3.one; }
                if (code == "T1" || code == "T2") Prim(PrimitiveType.Cylinder, "tower", World(c, h + 0.6f), new Vector3(0.7f, 0.6f, 0.7f), a.CellColor(code));
                if (code == "M") Prim(PrimitiveType.Sphere, "monster", World(c, h + 0.3f), Vector3.one * 0.6f, a.CellColor("M"));
            }
            Physics.SyncTransforms();
        }

        public void SetTowerColor(int player, Color col)
        {
            var t = b.Tower(player); if (t == null) return;
            foreach (Transform ch in Root.transform)
                if (ch.name == "tower" && Vector3.Distance(ch.localPosition, World(t.Value, ch.localPosition.y)) < 0.01f) ch.GetComponent<Renderer>().sharedMaterial = Assets.Mat(col);
        }

        /// <summary>Allinea i token allo stato della partita. Token: modello della carta se esiste, altrimenti segnalino generico.</summary>
        public void Sync(Game g)
        {
            foreach (var u in g.Units)
            {
                if (!u.Placed)
                {   // un alleato sconfitto lascia la board
                    if (tokens.TryGetValue(u.Id, out var gone)) { Kill(gone); tokens.Remove(u.Id); }
                    continue;
                }
                if (!tokens.TryGetValue(u.Id, out var t))
                {
                    var def = g.Def(u); var col = a.GuildColor(def.Guild);
                    t = new GameObject("token:" + def.Name); t.transform.SetParent(Root.transform, false);
                    var disc = Prim(PrimitiveType.Cylinder, "base", new Vector3(0, 0.03f, 0), new Vector3(0.8f, 0.03f, 0.8f), col);
                    disc.transform.SetParent(t.transform, false);
                    var mesh = a.Model(def.TokenModel);
                    GameObject body;
                    if (mesh != null)
                    {
                        body = new GameObject("model"); body.AddComponent<MeshFilter>().sharedMesh = mesh;
                        body.AddComponent<MeshRenderer>().sharedMaterial = Assets.Mat(Color.Lerp(col, Color.white, 0.35f));
                        body.transform.localPosition = new Vector3(0, 0.06f, 0);
                    }
                    else body = Prim(PrimitiveType.Capsule, "generic", new Vector3(0, 0.5f, 0), new Vector3(0.45f, 0.45f, 0.45f), Color.Lerp(col, Color.white, 0.35f));
                    body.transform.SetParent(t.transform, false);
                    tokens[u.Id] = t;
                }
                t.transform.localPosition = World(u.Pos, Top(u.Pos));
                // tappato = ruotato di lato, come una carta
                t.transform.localRotation = Quaternion.Euler(0, (u.Owner == 0 ? 0 : 180) + (u.Tapped ? 90 : 0), u.Tapped ? 18 : 0);
            }
        }

        // ---------- carte in gioco (risorse e oggetti) come lastre 3D sul tavolo, dal lato della Torre di chi le ha giocate ----------
        readonly List<GameObject> table = new List<GameObject>();
        public readonly List<(Vector3 Pos, string Text)> TableLabels = new List<(Vector3, string)>();
        public void SyncTable(Game g)
        {
            // shortcut: le lastre si ricreano a ogni cambio di stato (poche decine di oggetti); da riusare se le partite diventano lente
            foreach (var o in table) Kill(o); table.Clear(); TableLabels.Clear();
            for (int p = 0; p < 2; p++)
            {
                var t = b.Tower(p); var v = t != null ? World(t.Value, 0) : Vector3.zero;
                var dir = Mathf.Abs(v.x) > Mathf.Abs(v.z) ? new Vector3(Mathf.Sign(v.x), 0, 0) : new Vector3(0, 0, v.z != 0 ? Mathf.Sign(v.z) : p == 0 ? -1 : 1);
                var axis = new Vector3(dir.z, 0, dir.x); float half = (dir.x != 0 ? b.W : b.H) / 2f;
                var rows = new[]
                {
                    g.Players[p].Resources.Select((r, i) => (Id: r.CardId, r.Tapped, Name: "res:" + p + ":" + i)).ToList(),
                    g.Gm(p).Items.Select((id, i) => (Id: id, Tapped: false, Name: "item:" + p + ":" + i)).ToList(),
                };
                for (int row = 0; row < 2; row++)
                    for (int i = 0; i < rows[row].Count; i++)
                    {
                        var c = rows[row][i]; var def = g.Def(c.Id); var col = Color.Lerp(a.GuildColor(def.Guild), Color.black, c.Tapped ? 0.6f : 0.15f);
                        float step = Mathf.Min(0.72f, (half * 2 + 2) / Mathf.Max(1, rows[row].Count));
                        var pos = dir * (half + 0.75f + row * 0.95f) + axis * ((i - (rows[row].Count - 1) / 2f) * step) + Vector3.up * (0.02f + i * 0.002f);
                        var slab = Prim(PrimitiveType.Cube, "card:" + c.Id, pos, new Vector3(0.6f, 0.03f, 0.84f), col, true);
                        // dritta verso chi l'ha giocata; tappata = ruotata di lato
                        slab.transform.localRotation = Quaternion.LookRotation(-dir) * Quaternion.Euler(0, c.Tapped ? 90 : 0, 0);
                        var tex = a.CardArt(def.Id) ?? a.Symbol(Names.SymbolId(def.Guild, g.Preset));
                        if (tex != null)
                        {
                            var face = Prim(PrimitiveType.Quad, "face", Vector3.zero, Vector3.one, c.Tapped ? Color.gray : Color.white, false, true);
                            face.GetComponent<Renderer>().sharedMaterial.mainTexture = tex;
                            face.transform.SetParent(slab.transform, false);
                            face.transform.localPosition = new Vector3(0, 0.6f, 0.08f); face.transform.localRotation = Quaternion.Euler(90, 0, 0); face.transform.localScale = new Vector3(0.8f, 0.6f, 1);
                        }
                        table.Add(slab);
                        TableLabels.Add((pos + Vector3.up * 0.05f, def.Name.Replace(" (test)", "").Split(' ')[0] + (c.Tapped ? " (tap)" : "")));
                    }
            }
            Physics.SyncTransforms();
        }
        /// <summary>ID della carta 3D sotto il puntatore, o null.</summary>
        public string PickCard(Ray ray) => Physics.Raycast(ray, out var hit, 500f) && hit.collider.name.StartsWith("card:") ? hit.collider.name.Substring(5) : null;

        public void ClearMarks() { foreach (var m in marks) Kill(m); marks.Clear(); }

        /// <summary>onUnit: riquadro più largo e più alto della base del token, così resta visibile sotto l'unità.</summary>
        public void Mark(IEnumerable<Cell> cells, Color col, bool onUnit = false, bool zone = false)
        {
            foreach (var c in cells)
            {
                // zone: fondo della casella, più basso e più largo dei riquadri di mossa, che gli restano sopra
                var q = zone ? Prim(PrimitiveType.Cube, "zone", World(c, Top(c) + 0.01f), new Vector3(0.96f, 0.02f, 0.96f), col, false, true) : Prim(PrimitiveType.Cube, "mark", World(c, Top(c) + (onUnit ? 0.09f : 0.03f)), onUnit ? new Vector3(1.02f, 0.05f, 1.02f) : new Vector3(0.9f, 0.04f, 0.9f), col, false, true);
                marks.Add(q);
            }
        }

        public void Line((float x0, float y0, float x1, float y1) s, Color col)
        {
            var go = new GameObject("los"); go.transform.SetParent(Root.transform, false);
            var lr = go.AddComponent<LineRenderer>(); lr.useWorldSpace = false; lr.positionCount = 2; lr.startWidth = lr.endWidth = 0.07f;
            lr.sharedMaterial = Assets.Mat(col, true);
            lr.SetPosition(0, new Vector3(s.x0 - b.W / 2f, 1.05f, s.y0 - b.H / 2f)); lr.SetPosition(1, new Vector3(s.x1 - b.W / 2f, 1.05f, s.y1 - b.H / 2f));
            marks.Add(go);
        }

        public Cell? Pick(Ray ray)
        {
            if (Physics.Raycast(ray, out var hit, 500f) && hit.collider.name.StartsWith("cell:")) return Cell.Parse(hit.collider.name.Substring(5));
            return null;
        }

        public void Destroy() { Kill(Root); }
    }
}
