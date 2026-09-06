using UnityEngine;

#if MODULE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Watermelon
{
    /// <summary>
    /// Thin wrapper so gameplay code works with Input System Only on Android.
    /// </summary>
    public static class PointerInput
    {
        public static bool GetButtonDown()
        {
#if MODULE_INPUT_SYSTEM
            var pointer = Pointer.current;
            return pointer != null && pointer.press.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        public static bool GetButtonUp()
        {
#if MODULE_INPUT_SYSTEM
            var pointer = Pointer.current;
            return pointer != null && pointer.press.wasReleasedThisFrame;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }

        public static bool GetButton()
        {
#if MODULE_INPUT_SYSTEM
            var pointer = Pointer.current;
            return pointer != null && pointer.press.isPressed;
#else
            return Input.GetMouseButton(0);
#endif
        }

        public static Vector2 Position
        {
            get
            {
#if MODULE_INPUT_SYSTEM
                var pointer = Pointer.current;
                return pointer != null ? pointer.position.ReadValue() : Vector2.zero;
#else
                return Input.mousePosition;
#endif
            }
        }
    }
}
