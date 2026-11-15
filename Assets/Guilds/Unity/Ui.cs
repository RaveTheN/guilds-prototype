// Coordinate virtuali (altezza 1080) convertite in pixel reali a ogni chiamata: i caratteri sono
// rasterizzati alla dimensione finale invece di essere ingranditi da GUI.matrix (testo nitido).
using UnityEngine;

namespace Guilds.Unity
{
    static class G
    {
        public static float S = 1;
        public static Rect R(Rect r) => new Rect(Mathf.Round(r.x * S), Mathf.Round(r.y * S), Mathf.Round(r.width * S), Mathf.Round(r.height * S));
        public static void Label(Rect r, string t, GUIStyle s) => GUI.Label(R(r), t, s);
        public static bool Button(Rect r, string t) => GUI.Button(R(r), t);
        public static bool Button(Rect r, string t, GUIStyle s) => GUI.Button(R(r), t, s);
        public static bool Button(Rect r, GUIContent c, GUIStyle s) => GUI.Button(R(r), c, s);
        public static bool Toggle(Rect r, bool v, string t) => GUI.Toggle(R(r), v, t);
        public static string TextField(Rect r, string t) => GUI.TextField(R(r), t);
        public static string TextArea(Rect r, string t) => GUI.TextArea(R(r), t);
        public static void DrawTexture(Rect r, Texture t) => GUI.DrawTexture(R(r), t);
        public static void DrawTexture(Rect r, Texture t, ScaleMode m) => GUI.DrawTexture(R(r), t, m);
        public static float HorizontalSlider(Rect r, float v, float a, float b) => GUI.HorizontalSlider(R(r), v, a, b);
        public static int Toolbar(Rect r, int i, string[] t) => GUI.Toolbar(R(r), i, t);
        public static Vector2 BeginScrollView(Rect view, Vector2 pos, Rect content) => GUI.BeginScrollView(R(view), pos, R(content));
        public static void EndScrollView() => GUI.EndScrollView();
        public static float Height(GUIStyle s, string text, float width) => s.CalcHeight(new GUIContent(text), width * S) / S + 4;
        public static Vector2 Size(GUIStyle s, string text) => s.CalcSize(new GUIContent(text)) / S;
    }
}
