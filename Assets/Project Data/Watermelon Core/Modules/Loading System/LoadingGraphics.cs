using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Watermelon
{
    public class LoadingGraphics : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI loadingText;
        [SerializeField] Image backgroundImage;
        [SerializeField] CanvasScaler canvasScaler;
        [SerializeField] Camera loadingCamera;

        public void Initialise()
        {
            DontDestroyOnLoad(gameObject);

            canvasScaler.matchWidthOrHeight = 0;

            OnLoading(0.0f, "");
        }

        private void OnEnable()
        {
            GameLoading.OnLoading += OnLoading;
            GameLoading.OnLoadingFinished += OnLoadingFinished;
        }

        private void OnDisable()
        {
            GameLoading.OnLoading -= OnLoading;
            GameLoading.OnLoadingFinished -= OnLoadingFinished;
        }

        private void OnLoading(float state, string message)
        {
            //loadingText.text = message;
        }

        private void OnLoadingFinished()
        {
            // Stop blocking clicks immediately; fade is visual only.
            if (backgroundImage != null)
                backgroundImage.raycastTarget = false;
            var raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster != null)
                raycaster.enabled = false;
            var canvas = GetComponent<Canvas>();
            if (canvas != null)
                canvas.enabled = false;

            loadingText.DOFade(0.0f, 0.6f, unscaledTime: true);
            backgroundImage.DOFade(0.0f, 0.6f, unscaledTime: true).OnComplete(delegate
            {
                Destroy(gameObject);
            });
        }
    }
}
