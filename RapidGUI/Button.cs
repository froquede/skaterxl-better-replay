using UnityEngine;

namespace RapidGUI
{
    public static partial class RGUI
    {
        public static class ButtonSetting
        {
            public static float minWidth = 200f;
            public static float fieldWidth = 40f;
        }

        public static bool Button(bool v, string label, params GUILayoutOption[] options)
        {
            using (new GUILayout.VerticalScope(options))
            using (new GUILayout.HorizontalScope())
            {
                string text;
                GUILayout.Label("<b>" + label + "</b>", GUILayout.Height(24f));

                if (v)
                {
                    GUI.backgroundColor = Color.green;
                    text = "Enabled";
                }
                else
                {
                    text = "Disabled";
                }

                v = GUILayout.Button("<b>" + text + "</b>", RGUIStyle.button, GUILayout.Width(72f), GUILayout.Height(24f));
            }

            GUI.backgroundColor = Color.black;

            return v;
        }
    }
}