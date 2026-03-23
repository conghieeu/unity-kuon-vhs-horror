using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using UHFPS.Tools;

namespace UHFPS.Editors
{
    public class WelcomeScreen : EditorWindow
    {
        public const string UHFPS_VERSION = "1.4.1";
        public const string UPDATE_TYPE = "Patch";

        const string SHOWN_KEY = "UHFPS.WelcomeState";
        const string LOGO = "uhfps_welcome";

        public static GUIStyle wordWrappedLabelCenter
        {
            get => new(EditorStyles.wordWrappedLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
        }

        public static GUIStyle miniBoldLabelCenter
        {
            get => new(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
        }

        static WelcomeScreen()
        {
            EditorApplication.delayCall += ShowWelcome;
        }

        static void ShowWelcome()
        {
            EditorApplication.delayCall -= ShowWelcome;

            if (!EditorPrefs.GetBool(SHOWN_KEY, false))
            {
                ShowWindow();
                EditorPrefs.SetBool(SHOWN_KEY, true);
            }
        }

        [MenuItem("Tools/UHFPS/Welcome Screen", false, 0)]
        static void ShowWindow()
        {
            EditorWindow window = GetWindow<WelcomeScreen>(true, "Ultimate Horror FPS KIT - Welcome Screen", true);
            window.minSize = new Vector2(500, 550);
            window.maxSize = new Vector2(500, 550);
            window.Show();
        }

        [MenuItem("Tools/UHFPS/Documentation", false, 1)]
        static void ShowDocumentation()
        {
            Application.OpenURL("https://docs.twgamesdev.com/uhfps/");
        }

        private void OnGUI()
        {
            Rect rect = new(0, 0, position.width, position.height);
            rect.width -= 15;
            rect.x += 7;
            rect.y += 7;

            GUILayout.BeginArea(rect);
            {
                Rect logoRect = GUILayoutUtility.GetRect(1, 70);
                Texture2D uhfpsLogo = Resources.Load<Texture2D>(LOGO);
                GUI.DrawTexture(logoRect, uhfpsLogo);

                var centerLabel = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    richText = true
                };

                EditorGUILayout.Space(3);
                string updateType = UPDATE_TYPE.IsEmpty() ? "" : " (" + UPDATE_TYPE + ")";
                EditorGUILayout.LabelField($"<b>Version {UHFPS_VERSION}{updateType}</b>", centerLabel);
                EditorGUILayout.Space(3);

                DrawLine();
                EditorGUILayout.Space(7);

                DrawSection("Acknowledgement", EditorGUIUtility.singleLineHeight);
                EditorGUILayout.LabelField("I would like to personally thank you for purchasing the <b>Ultimate Horror FPS KIT</b>. It brings me great joy to see my work being used and appreciated by others. I have put a lot of effort and passion into creating this kit, and I am confident that it will help you create the HORROR FPS game that you envision.", wordWrappedLabelCenter);

                EditorGUILayout.Space(10);
                DrawSection("Preparation", EditorGUIUtility.singleLineHeight);
                EditorGUILayout.LabelField("To ensure that you are fully equipped with the knowledge required to use the kit effectively, I highly recommend reading through the entire documentation prior to usage. This will enable you to have a thorough understanding of the kit's functionality and how to utilize its components.", wordWrappedLabelCenter);

                EditorGUILayout.Space(10);
                DrawSection("Support & Feedback", EditorGUIUtility.singleLineHeight);
                EditorGUILayout.LabelField("If you encounter any issues or have any feedback, please don't hesitate to reach out to me. I am always open to suggestions and I am committed to providing the best possible support to my customers. You can contact me on my Discord server, where I'm mostly active.", wordWrappedLabelCenter);

                GUILayout.FlexibleSpace();

                GUIContent documentation = EditorGUIUtility.TrTextContentWithIcon(" Documentation", "Linked");
                if (GUILayout.Button(documentation, GUILayout.MinHeight(30)))
                {
                    Application.OpenURL("https://docs.twgamesdev.com/uhfps/");
                }

                GUIContent discord = EditorGUIUtility.TrTextContentWithIcon(" Discord", "Linked");
                if (GUILayout.Button(discord, GUILayout.MinHeight(30)))
                {
                    Application.OpenURL("https://discord.gg/p6vdaNC");
                }

                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            }
            GUILayout.EndArea();
        }

        private void DrawLine(int height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        private void DrawSection(string title, float height)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;

            EditorDrawing.DrawHeader(rect, new GUIContent(title), miniBoldLabelCenter);
        }
    }
}