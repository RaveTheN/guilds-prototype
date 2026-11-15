// Guilds.Core — board: tipi di casella, percorsi, linea di vista geometrica, ritocco delle mappe.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Guilds.Core
{
    public class Board
    {
        public readonly MapDef Map;
        public readonly int W, H;
        readonly int[,] hgRegion; // regione di highground connessa (0 = non highground)

        public Board(MapDef map)
        {
            Map = map; W = map.Width; H = map.Height;
            hgRegion = new int[W, H];
            int next = 0;
            foreach (var c in All().Where(IsHighground))
            {
                if (hgRegion[c.X, c.Y] != 0) continue;
                next++;
                var q = new Queue<Cell>(); q.Enqueue(c); hgRegion[c.X, c.Y] = next;
                while (q.Count > 0)
                {
                    var k = q.Dequeue();
                    foreach (var n in new[] { new Cell(k.X + 1, k.Y), new Cell(k.X - 1, k.Y), new Cell(k.X, k.Y + 1), new Cell(k.X, k.Y - 1) })
                        if (IsHighground(n) && hgRegion[n.X, n.Y] == 0) { hgRegion[n.X, n.Y] = next; q.Enqueue(n); }
                }
            }
        }

        public IEnumerable<Cell> All() { for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) if (Code(new Cell(x, y)) != "") yield return new Cell(x, y); }
        public string Code(Cell c) => c.X < 0 || c.Y < 0 || c.X >= W || c.Y >= H ? "" : Map.Cells[H - 1 - c.Y][c.X] ?? "";
        public bool InMap(Cell c) => Code(c) != "";
        // Ipotesi H13: la casella mostro (M) sta su highground; senza regole dei mostri è un ostacolo inerte.
        public bool IsHighground(Cell c) { var k = Code(c); return k == "H" || k == "M"; }
        // Ipotesi H12: la Torre inerte occupa la casella e blocca movimento e linea di vista come un muro.
        public bool Opaque(Cell c) { var k = Code(c); return k == "W" || k == "T1" || k == "T2"; }
        public bool BlocksMove(Cell c) { var k = Code(c); return k == "" || k == "W" || k == "M" || k == "T1" || k == "T2"; }
        public Cell? Tower(int player) { var t = All().Where(c => Code(c) == "T" + (player + 1)).ToList(); return t.Count == 1 ? t[0] : (Cell?)null; }

        public static bool Adjacent(Cell a, Cell b, int adjacency)
        {
            int dx = Math.Abs(a.X - b.X), dy = Math.Abs(a.Y - b.Y);
            return adjacency == 4 ? dx + dy == 1 : Math.Max(dx, dy) == 1;
        }

        /// <summary>Caselle raggiungibili in 1..max passi (anche diagonali), col percorso. passable: si può attraversare; canStop: ci si può fermare.</summary>
        public Dictionary<Cell, List<Cell>> Reach(Cell from, int max, Func<Cell, bool> passable, Func<Cell, bool> canStop)
        {
            var paths = new Dictionary<Cell, List<Cell>> { [from] = new List<Cell>() };
            var q = new Queue<Cell>(); q.Enqueue(from);
            while (q.Count > 0)
            {
                var k = q.Dequeue();
                if (paths[k].Count >= max) continue;
                for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++)
                {
                    var n = new Cell(k.X + dx, k.Y + dy);
                    if ((dx == 0 && dy == 0) || paths.ContainsKey(n) || BlocksMove(n) || !passable(n)) continue;
                    paths[n] = new List<Cell>(paths[k]) { n }; q.Enqueue(n);
                }
            }
            paths.Remove(from);
            return paths.Where(p => canStop(p.Key)).ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>Caselle di partenza del giocatore. S-B: libere adiacenti alla propria Torre. S-A: angolo libero, se esiste una coppia opposta.</summary>
        public List<Cell> StartCells(int player, string mode)
        {
            if (mode == "S-B")
            {
                var t = Tower(player);
                return t == null ? new List<Cell>() : All().Where(c => Code(c) == "." && Adjacent(c, t.Value, 8)).ToList();
            }
            var pairs = new[] { (new Cell(0, 0), new Cell(W - 1, H - 1)), (new Cell(W - 1, 0), new Cell(0, H - 1)) }
                .Where(p => Code(p.Item1) == "." && Code(p.Item2) == ".").ToList();
            if (pairs.Count != 1) return new List<Cell>(); // nessuna o ambigua: la fonte non dice quali angoli (D11)
            return new List<Cell> { player == 0 ? pairs[0].Item1 : pairs[0].Item2 };
        }

        /// <summary>LOS-G (F1-36/37/38 + U-05): retta da un angolo qualsiasi di a a un angolo qualsiasi di b.
        /// Bloccano muri, Torri, caselle con unità nemiche (unitBlocks) e l'highground attraversato (non quello da cui si parte o in cui si arriva).
        /// Toccare solo un angolo di una casella bloccante è valido; correre lungo un suo bordo no.
        /// mode: «angoli» (questa regola), «angoli-netta» (toccare anche solo un angolo di un muro, di una Torre o di un'unità non amica
        /// invalida quella retta; per l'highground resta la regola base), «centro» (una sola retta, dal centro di a al centro di b).</summary>
        public bool Los(Cell a, Cell b, Func<Cell, bool> unitBlocks, string mode = "angoli") => LosLine(a, b, unitBlocks, mode) != null;

        public (float x0, float y0, float x1, float y1)? LosLine(Cell a, Cell b, Func<Cell, bool> unitBlocks, string mode = "angoli")
        {
            if (a.Equals(b)) return (a.X + .5f, a.Y + .5f, a.X + .5f, a.Y + .5f);
            int ra = hgRegion[a.X, a.Y], rb = hgRegion[b.X, b.Y];
            var others = All().Where(c => !c.Equals(a) && !c.Equals(b)).ToList();
            var hard = others.Where(c => Opaque(c) || unitBlocks(c)).ToList();
            var high = others.Where(c => hgRegion[c.X, c.Y] != 0 && hgRegion[c.X, c.Y] != ra && hgRegion[c.X, c.Y] != rb).ToList();
            double limit = mode == "angoli-netta" ? -0.5 : 1e-9; // netta: anche il solo contatto (lunghezza 0) blocca
            (float, float, float, float)? Try(double x0, double y0, double x1, double y1) =>
                hard.All(c => ClipLength(x0, y0, x1, y1, c) < limit) && high.All(c => ClipLength(x0, y0, x1, y1, c) < 1e-9)
                    ? ((float)x0, (float)y0, (float)x1, (float)y1) : ((float, float, float, float)?)null;
            if (mode == "centro") return Try(a.X + .5, a.Y + .5, b.X + .5, b.Y + .5);
            for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++)
            {
                var l = Try(a.X + (i & 1), a.Y + (i >> 1), b.X + (j & 1), b.Y + (j >> 1));
                if (l != null) return l;
            }
            return null;
        }

        // Lunghezza del tratto di segmento dentro il quadrato chiuso della casella (Liang–Barsky); 0 = lo tocca soltanto, -1 = non lo tocca.
        static double ClipLength(double x0, double y0, double x1, double y1, Cell c)
        {
            double dx = x1 - x0, dy = y1 - y0, t0 = 0, t1 = 1;
            double[] p = { -dx, dx, -dy, dy }, q = { x0 - c.X, c.X + 1 - x0, y0 - c.Y, c.Y + 1 - y0 };
            for (int i = 0; i < 4; i++)
            {
                if (Math.Abs(p[i]) < 1e-12) { if (q[i] < 0) return -1; continue; }
                double t = q[i] / p[i];
                if (p[i] < 0) t0 = Math.Max(t0, t); else t1 = Math.Min(t1, t);
            }
            return t1 - t0 > 1e-9 ? (t1 - t0) * Math.Sqrt(dx * dx + dy * dy) : t1 - t0 > -1e-9 ? 0 : -1;
        }
    }

    /// <summary>Ritocco delle mappe fra una partita e l'altra: solo varianti nuove, mai sovrascrittura.</summary>
    public static class MapOps
    {
        public static readonly string[] CellTypes = { ".", "W", "H", "J", "M", "T1", "T2" };

        public static List<string> Validate(MapDef m, string startMode = "S-B")
        {
            var err = new List<string>(); var b = new Board(m);
            for (int p = 0; p < 2; p++)
            {
                int n = b.All().Count(c => b.Code(c) == "T" + (p + 1));
                if (n != 1) err.Add($"Serve esattamente una Torre per il giocatore {p + 1} (trovate: {n}).");
            }
            if (err.Count > 0) return err;
            var s0 = b.StartCells(0, startMode); var s1 = b.StartCells(1, startMode);
            if (s0.Count == 0) err.Add($"Giocatore 1: nessuna casella di partenza libera ({startMode}).");
            if (s1.Count == 0) err.Add($"Giocatore 2: nessuna casella di partenza libera ({startMode}).");
            if (err.Count > 0) return err;
            var reach = b.Reach(s0[0], 999, c => true, c => true); reach[s0[0]] = null;
            foreach (var c in s0.Concat(s1)) if (!reach.ContainsKey(c)) err.Add($"La casella di partenza {c} non è raggiungibile dalle altre.");
            return err;
        }

        public static List<string> Warnings(MapDef m)
        {
            var w = new List<string>(m.ZoneWarnings); var b = new Board(m);
            if (b.All().Any(c => b.Code(c) == "J")) w.Add("La mappa contiene caselle Jungle: in questo preset non hanno regole (F4b-07 rinviata) e valgono come caselle normali.");
            if (b.All().Any(c => b.Code(c) == "M")) w.Add("Le caselle mostro sono ostacoli inerti in questo preset (F3-06 rinviata).");
            return w;
        }

        /// <summary>Crea una variante con le caselle indicate cambiate. Le zone esplicite sono ereditate; le differenze sono segnalate.</summary>
        public static MapDef MakeVariant(MapDef src, Dictionary<Cell, string> changes, string newId)
        {
            var m = new MapDef
            {
                Id = newId, Name = src.Name + " — variante " + newId, Source = src.Source, BaseMap = src.BaseMap ?? src.Id,
                Width = src.Width, Height = src.Height, Legend = src.Legend,
                Cells = src.Cells.Select(r => r.ToList()).ToList(), ExplicitZones = new Dictionary<string, string>(src.ExplicitZones),
                ChangedCells = new List<string>(src.ChangedCells), ZoneWarnings = new List<string>(src.ZoneWarnings)
            };
            var b = new Board(src);
            foreach (var ch in changes)
            {
                string old = b.Code(ch.Key);
                if (old == "") throw new ArgumentException($"{ch.Key} è fuori mappa: non si può cambiare la forma della mappa.");
                if (old == ch.Value) continue;
                m.Cells[m.Height - 1 - ch.Key.Y][ch.Key.X] = ch.Value;
                m.ChangedCells.Add($"{ch.Key}: {old} → {ch.Value}");
                if (m.ExplicitZones.TryGetValue(ch.Key.Name, out var z) && z != "da definire")
                    m.ZoneWarnings.Add($"{ch.Key}: zona esplicita ereditata «{z}», ma la casella ora è «{ch.Value}».");
            }
            return m;
        }
    }
}
