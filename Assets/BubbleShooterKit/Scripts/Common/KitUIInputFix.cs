using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace BubbleShooterKit
{
    /// <summary>
    /// Hard reset Kit UI input when entered from Watermelon.
    /// DDOL EventSystem / missing UI actions leave HomeScreen/LevelScreen unclickable.
    /// </summary>
    public static class KitUIInputFix
    {
#if ENABLE_INPUT_SYSTEM
        private static readonly string[] KitSceneNames = { "HomeScreen", "LevelScreen", "GameScreen" };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Apply();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Apply();
        }

        private static void Apply()
        {
            var activeScene = SceneManager.GetActiveScene();
            bool kitScene = IsKitScene(activeScene.name);

            FixAudioListeners(kitScene);
            FixEventSystems(kitScene);
            MuteDdolCanvasRaycasters(kitScene);

            if (kitScene)
                EnsureKitEventSystem(activeScene);
        }

        private static bool IsKitScene(string name)
        {
            for (int i = 0; i < KitSceneNames.Length; i++)
            {
                if (KitSceneNames[i] == name)
                    return true;
            }
            return false;
        }

        private static void FixAudioListeners(bool kitScene)
        {
            var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < listeners.Length; i++)
            {
                var listener = listeners[i];
                if (listener == null)
                    continue;

                bool ddol = !listener.gameObject.scene.IsValid() || listener.gameObject.scene.name == "DontDestroyOnLoad";
                if (ddol)
                    listener.enabled = !kitScene;
            }
        }

        private static void FixEventSystems(bool kitScene)
        {
            var systems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < systems.Length; i++)
            {
                var es = systems[i];
                if (es == null)
                    continue;

                bool ddol = !es.gameObject.scene.IsValid() || es.gameObject.scene.name == "DontDestroyOnLoad";
                if (!ddol)
                    continue;

                es.enabled = !kitScene;
                es.gameObject.SetActive(!kitScene);
            }
        }

        private static void MuteDdolCanvasRaycasters(bool kitScene)
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (canvas == null)
                    continue;

                bool ddol = !canvas.gameObject.scene.IsValid() || canvas.gameObject.scene.name == "DontDestroyOnLoad";
                if (!ddol)
                    continue;

                if (canvas.gameObject.name == "SystemCanvas")
                {
                    canvas.enabled = false;
                    var systemRaycaster = canvas.GetComponent<GraphicRaycaster>();
                    if (systemRaycaster != null)
                        systemRaycaster.enabled = false;
                    continue;
                }

                var raycasters = canvas.GetComponents<GraphicRaycaster>();
                for (int r = 0; r < raycasters.Length; r++)
                {
                    if (raycasters[r] != null)
                        raycasters[r].enabled = !kitScene;
                }
            }
        }

        private static void EnsureKitEventSystem(Scene activeScene)
        {
            EventSystem keep = null;
            var systems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < systems.Length; i++)
            {
                var es = systems[i];
                if (es == null)
                    continue;

                bool ddol = !es.gameObject.scene.IsValid() || es.gameObject.scene.name == "DontDestroyOnLoad";
                if (ddol)
                    continue;

                if (es.gameObject.scene == activeScene)
                {
                    if (keep == null)
                        keep = es;
                    else
                        Object.Destroy(es.gameObject);
                }
            }

            if (keep == null)
            {
                var go = new GameObject("KitEventSystem");
                SceneManager.MoveGameObjectToScene(go, activeScene);
                keep = go.AddComponent<EventSystem>();
                var module = go.AddComponent<InputSystemUIInputModule>();
                module.AssignDefaultActions();
            }
            else
            {
                var legacy = keep.GetComponent<StandaloneInputModule>();
                if (legacy != null)
                    Object.DestroyImmediate(legacy);

                var module = keep.GetComponent<InputSystemUIInputModule>();
                if (module == null)
                    module = keep.gameObject.AddComponent<InputSystemUIInputModule>();
                module.AssignDefaultActions();
                keep.gameObject.SetActive(true);
                keep.enabled = true;
            }
        }
#endif
    }
}
