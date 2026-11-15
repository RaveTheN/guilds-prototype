// Comandi di menu e da riga di comando: generano la scena e verificano il progetto senza configurazione manuale.
using System;
using System.IO;
using Guilds.Core;
using Guilds.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Guilds.EditorTools
{
    public static class GuildsSetup
    {
        const string ScenePath = "Assets/Guilds/Scenes/Guilds.unity";

        [MenuItem("Guilds/Genera scena")]
        public static void GenerateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera").AddComponent<Camera>(); cam.tag = "MainCamera";
            var light = new GameObject("Directional Light").AddComponent<Light>(); light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(55, -30, 0); light.intensity = 1.1f;
            new GameObject("GuildsApp").AddComponent<GuildsApp>();
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("Scena generata: " + ScenePath);
        }

        [MenuItem("Guilds/Gioca %g")]
        public static void Play()
        {
            if (!File.Exists(ScenePath)) GenerateScene();
            if (EditorSceneManager.GetActiveScene().path != ScenePath) EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.EnterPlaymode();
        }

        /// <summary>Unity.exe -batchmode -projectPath . -executeMethod Guilds.EditorTools.GuildsSetup.BatchCheck -quit</summary>
        public static void BatchCheck()
        {
            try
            {
                GenerateScene();
                var root = Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..")), "data");
                var repo = Repo.Load(root);
                foreach (var id in new[] { "P01", "P01-9x7", "P01m", "P01d" })
                {
                    var g = Bot.Play(repo.NewGame(repo.PresetById(id), repo.Deck("deck_red"), repo.Deck("deck_green"), 20261114), 7);
                    if (g.Phase != Phase.Over) throw new Exception("partita simulata non conclusa: " + id);
                    var view = new BoardView(); view.Build(g.Board, new Assets(root, "semplice")); view.Sync(g);
                    Debug.Log($"GUILDS_CHECK {id}: {g.Turn} turni, vincitore G{g.Winner + 1}, oggetti board {view.Root.transform.childCount}");
                    view.Destroy();
                }
                Debug.Log("GUILDS_CHECK_OK");
            }
            catch (Exception e) { Debug.LogError("GUILDS_CHECK_FAIL " + e); EditorApplication.Exit(1); }
        }

        /// <summary>Unity.exe -projectPath . -executeMethod Guilds.EditorTools.GuildsSetup.Shots -guildsShot &lt;cartella&gt; (senza -batchmode)</summary>
        public static void Shots()
        {
            if (!File.Exists(ScenePath)) GenerateScene();
            EditorSceneManager.OpenScene(ScenePath);
            if (!Application.isBatchMode) EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.EnterPlaymode();
        }
    }
}
