#region Packages

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#endregion

namespace GameDev.Common
{
    public static class CommonDebug
    {
        public static void DrawString(string text, Vector3 worldPos, int? fontsize)
        {
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.fontSize = fontsize.HasValue ? fontsize.Value : 30;

            Handles.BeginGUI();

            try
            {
                SceneView view = SceneView.currentDrawingSceneView;
                Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
                Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
                GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y),
                    text, style);
            }
            catch
            {
                //Ignore
            }

            Handles.EndGUI();
#endif
        }
    }
}