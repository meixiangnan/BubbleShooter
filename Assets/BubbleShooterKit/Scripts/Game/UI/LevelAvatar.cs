using UnityEngine;
using UnityEngine.UI;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class manages the player avatar that is displayed on the level scene.
    /// </summary>
    public class LevelAvatar : MonoBehaviour
    {
        private bool floating;
        private float runningTime;

        private void Awake()
        {
            // Avatar is decorative; keep clicks going to LevelMapButton underneath.
            var graphics = GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] != null)
                    graphics[i].raycastTarget = false;
            }
        }

        private void Update()
        {
            if (!floating)
                return;

            var deltaHeight = Mathf.Sin(runningTime + Time.deltaTime);
            var newPos = transform.position;
            newPos.y += deltaHeight * 0.002f;
            transform.position = newPos;
            runningTime += Time.deltaTime * 2;
        }

        public void StartFloatingAnimation()
        {
            floating = true;
        }
    }
}
