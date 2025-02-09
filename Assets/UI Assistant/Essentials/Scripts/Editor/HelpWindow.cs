using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UIAssistant
{
    public class HelpWindow : SettingsWindow
    {
        #region Variables
        readonly GUIContent WebsiteContent = new("Website", WebsiteLink);
        const string WebsiteLink = "https://sites.google.com/view/uiassistant/";
        readonly GUIContent YouTubeContent = new("YouTube", YouTubeLink);
        const string YouTubeLink = "https://www.youtube.com/@UIAssistant";
        readonly GUIContent DiscordContent = new("Discord", DiscordLink);
        const string DiscordLink = "https://discord.gg/Fg4AxguPwG";
        readonly GUIContent AssetStoreContent = new("Asset Store", AssetStoreLink);
        const string AssetStoreLink = "https://u3d.as/3dXj";
        readonly GUIContent PayPalContent = new("Donate", PayPalLink);
        const string PayPalLink = "https://www.paypal.com/donate/?hosted_button_id=3LCPSPT2GEF34";

        const string SuppressHelpEditorPref = "UIAssistantSuppressHelp";
        const string HelpSeenSessionState = "UIAssistantHelpSeen";

        GUIStyle TextStyle;
        #endregion

        #region Function
        [MenuItem("Tools/UI Assistant/Help", priority = -9999)]
        public static void OpenWindow()
        {
            GetWindow<HelpWindow>("UI Assistant Help");
        }
        [InitializeOnLoadMethod]
        static void OnEditorLoad()
        {
            if (!SessionState.GetBool(HelpSeenSessionState, false))
            {
                if (!EditorPrefs.GetBool(SuppressHelpEditorPref, false)) EditorApplication.delayCall += OpenWindow;
                SessionState.SetBool(HelpSeenSessionState, true);
            }
        }

        override protected void OnGUI()
        {
            base.OnGUI();

            TextStyle = new()
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
            };

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            float logoSize = 100;
            GUI.contentColor = Color.white;
            GUILayout.Label(ContentLibrary.LogoTexture, GUILayout.Width(logoSize), GUILayout.Height(logoSize));
            GUI.contentColor = ContentLibrary.ContentColor;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUILayout.Label("Thank you for using the UI Assistant!\nHere are some resources to help you get the most out of the tool.", TextStyle);
            Space();

            BeginScrollArea();
            EditorGUILayout.Space();

            TextStyle.alignment = TextAnchor.MiddleRight;
            URLButton("Read the documentation:", WebsiteContent, WebsiteLink);
            Space();
            URLButton("Watch step-by-step tutorials:", YouTubeContent, YouTubeLink);
            Space();
            URLButton("Ask questions, and share feedback:", DiscordContent, DiscordLink);
            Space();
            URLButton("Rate this package:", AssetStoreContent, AssetStoreLink);
            Space();
            URLButton("Support the developer:", PayPalContent, PayPalLink);

            EditorGUILayout.Space();
            EndScrollArea();
            GUILayout.FlexibleSpace();

            bool suppressHelp = EditorPrefs.GetBool(SuppressHelpEditorPref, false);
            bool suppressHelpPreference = GUILayout.Toggle(suppressHelp, "Do not show this window at startup");
            if (suppressHelp != suppressHelpPreference) EditorPrefs.SetBool(SuppressHelpEditorPref, suppressHelpPreference);
        }
        void Space() { GUILayout.Space(20); }
        void URLButton(string text, GUIContent content, string url)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(text, TextStyle, GUILayout.Width(230), GUILayout.Height(ContentLibrary.AddButtonHeight));

            EditorGUILayout.Space();

            if (GUILayout.Button(content, GUILayout.Width(150), GUILayout.Height(ContentLibrary.AddButtonHeight)))
                Application.OpenURL(url);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        #endregion
    }
}