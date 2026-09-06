#pragma warning disable 0649

using System;
using System.Collections;
using System.Threading.Tasks;
using Project_Data.SDK;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    [DefaultExecutionOrder(-999)]
    [HelpURL("https://docs.google.com/document/d/1ORNWkFMZ5_Cc-BUgu9Ds1DjMjR4ozMCyr6p_GGdyCZk")]
    public class Initialiser : MonoBehaviour
    {
        [SerializeField] ProjectInitSettings initSettings;
        [SerializeField] Canvas systemCanvas;
        [SerializeField] EventSystem eventSystem;
        SDKProxy sdkProxy = null;

        [Space]
        [SerializeField] ScreenSettings screenSettings;

        public static Canvas SystemCanvas;
        public static GameObject InitialiserGameObject;
        
        private GameGlobal gameGlobal;

        public static bool IsInititalized { get; private set; }
        public static bool IsStartInitialized { get; private set; }
        public static ProjectInitSettings InitSettings { get; private set; }

        public void Awake()
        {
            screenSettings.Initialise();
            
            
            if (!IsInititalized)
            {
                IsInititalized = true;

                // Stale flag from a previous process would wrongly open GameDispatch on cold start.
                PlayerPrefs.DeleteKey("open_game_dispatch");

                InitSettings = initSettings;
                SystemCanvas = systemCanvas;
                InitialiserGameObject = gameObject;
                HttpManager.Instance.Init();
                
                gameGlobal = this.AddComponent<GameGlobal>();
                gameGlobal.Init();

                // Remove leftover modules immediately (Destroy is deferred and can leave EventSystem stuck).
                var existingModules = eventSystem.GetComponents<BaseInputModule>();
                for (int i = 0; i < existingModules.Length; i++)
                    DestroyImmediate(existingModules[i]);

#if MODULE_INPUT_SYSTEM
                var uiModule = eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                uiModule.AssignDefaultActions();
#else
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
#endif

                // SystemCanvas is empty Overlay @ sorting 999; with Input System it can swallow all UI clicks.
                if (systemCanvas != null)
                {
                    systemCanvas.overrideSorting = true;
                    systemCanvas.sortingOrder = 0;
                    systemCanvas.enabled = false;
                    var systemRaycaster = systemCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                    if (systemRaycaster != null)
                        systemRaycaster.enabled = false;
                }

                DontDestroyOnLoad(gameObject);

                initSettings.Initialise(this);

                sdkProxy = new SDKProxy();
            }
        }

        public void Start()
        {
            InitSDKInfo();
        }

        async void InitSDKInfo()
        {
            sdkProxy.InitSDK();
            
            sdkProxy.LocalInit();
            
            Initialise(true);
        }

        public void Initialise(bool loadingScene)
        {
            if (!IsStartInitialized)
            {
                IsStartInitialized = true;

                if (loadingScene)
                {
                    GameLoading.LoadGameScene();
                }
                else
                {
                    GameLoading.SimpleLoad();
                }
            }
        }

        public static bool IsModuleInitialised(Type moduleType)
        {
            ProjectInitSettings projectInitSettings = InitSettings;

            InitModule[] coreModules = null;
            InitModule[] initModules = null;

#if UNITY_EDITOR
            if (!IsInititalized)
            {
                projectInitSettings = RuntimeEditorUtils.GetAssetByName<ProjectInitSettings>();
            }
#endif

            if (projectInitSettings != null)
            {
                coreModules = projectInitSettings.CoreModules;
                initModules = projectInitSettings.Modules;
            }

            for (int i = 0; i < coreModules.Length; i++)
            {
                if (coreModules[i].GetType() == moduleType)
                {
                    return true;
                }
            }

            for (int i = 0; i < initModules.Length; i++)
            {
                if (initModules[i].GetType() == moduleType)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDestroy()
        {
            IsInititalized = false;

#if UNITY_EDITOR
            SaveController.Save(true);
#endif
        }

        private void OnApplicationFocus(bool focus)
        {
#if !UNITY_EDITOR
            if(!focus) SaveController.Save();
#endif
        }
    }
}

// -----------------
// Initialiser v 0.4.4
// -----------------

// Changelog
// v 0.4.4
// • Added event system initialisation based on input module type
// v 0.4.3
// • Fixed editor adding core module bug
// v 0.4.2
// • Added loading scene logic
// v 0.4.1
// • Fixed error on module remove
// v 0.3.1
// • Added link to the documentation
// • Initializer renamed to Initialiser
// • Fixed problem with recompilation
// v 0.2
// • Added sorting feature
// • Initialiser MonoBehaviour will destroy after initialization
// v 0.1
// • Added basic version
