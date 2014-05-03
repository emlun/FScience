using UnityEngine;

namespace FScience {
    public static class Resources {
        public static Texture2D IconOff = new Texture2D(32, 32, TextureFormat.ARGB32, false);
        public static Texture2D IconOn = new Texture2D(32, 32, TextureFormat.ARGB32, false);

        public static GUIStyle WindowStyle;
        public static GUIStyle IconStyle;
        public static GUIStyle ButtonToggledStyle;
        public static GUIStyle ButtonToggledRedStyle;
        public static GUIStyle ButtonStyle;
        public static GUIStyle ErrorLabelRedStyle;
        public static GUIStyle LabelStyle;
        public static GUIStyle LabelStyleRed;
        public static GUIStyle LabelStyleYellow;

        public static Rect DefaultWindowRect = new Rect(20, 20, 640, 360);

        public static string FromContainerTextPrefix = "From: ";
        public static string FromContainerViewTitle = "From Container:";
        public static string FromExperimentViewTitle = "From Experiment:";
        public static string FromExperimentTextPrefix = "To: ";
        public static string MainButtonText = "ScT";
        public static string MainWindowTitle = "Science Transfer";
        public static string NoneSelectedText = "No Part Selected";
        public static string ToContainerTextPrefix = "To: ";
        public static string ToContainerViewTitle = "To Container:";
        public static string TransferAllContainersButtonText = "Transfer from all containers";
        public static string TransferAllExperimentsButtonText = "Transfer from all experiments";
        public static string TransferButtonText = "Transfer";

        public static void SetupGUI() {
            GUI.skin = HighLogic.Skin;
            if(WindowStyle == null) {
                SetStyles();
            }
        }

        public static void SetStyles() {
            WindowStyle = new GUIStyle(GUI.skin.window);
            IconStyle = new GUIStyle();

            ButtonToggledStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledStyle.normal.textColor = Color.green;
            ButtonToggledStyle.normal.background = ButtonToggledStyle.onActive.background;

            ButtonToggledRedStyle = new GUIStyle(ButtonToggledStyle);
            ButtonToggledRedStyle.normal.textColor = Color.red;

            ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.normal.textColor = Color.white;

            ErrorLabelRedStyle = new GUIStyle(GUI.skin.label);
            ErrorLabelRedStyle.normal.textColor = Color.red;
            ErrorLabelRedStyle.fontSize = 10;

            LabelStyle = new GUIStyle(GUI.skin.label);

            LabelStyleRed = new GUIStyle(LabelStyle);
            LabelStyleRed.normal.textColor = Color.red;

            LabelStyleYellow = new GUIStyle(LabelStyle);
            LabelStyleYellow.normal.textColor = Color.yellow;
        }
    }
}

