using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Quartz.Debugging
{
    internal class QuartzDebug : MonoBehaviour
    {
        private static QuartzDebug instance = null;

        private static bool debug = false;
        private static Color color = Color.white;
        private static int fontSize = 0; 

        public static bool DebugRaycast
        {
            get
            {
                return QuartzDebug.debug;
            }
            set
            {
                QuartzDebug.debug = value;
                if (value && Application.isPlaying)
                {
                    QuartzDebug.CreateInstance();
                }
            }
        }

        public static Color FontColor 
        { 
            get { return color; } 
            set { color = value; }
        }

        public static int FontSize 
        {
            get { return fontSize; } 
            set { fontSize = value; }
        }

        public static void CreateInstance()
        {
            if (QuartzDebug.instance == null)
            {
                GameObject gameObject = new GameObject("Quartz Debug");
                QuartzDebug.instance = gameObject.AddComponent<QuartzDebug>();
                DontDestroyOnLoad(gameObject);
            }
        }

        public static void DrawBounds(Bounds b)
        {
            Vector3 center = b.center;
            Vector3 vector = b.center - b.extents;
            Vector3 vector2 = b.center + b.extents;
            Debug.DrawLine(new Vector3(vector.x, vector.y, center.z), new Vector3(vector2.x, vector.y, center.z), Color.red);
            Debug.DrawLine(new Vector3(vector.x, vector.y, center.z), new Vector3(vector.x, vector2.y, center.z), Color.red);
            Debug.DrawLine(new Vector3(vector2.x, vector.y, center.z), new Vector3(vector2.x, vector2.y, center.z), Color.red);
            Debug.DrawLine(new Vector3(vector.x, vector2.y, center.z), new Vector3(vector2.x, vector2.y, center.z), Color.red);
        }

        private void OnGUI()
        {
            if (debug)
            {
                Rect rect = new Rect(5f, 5f, 1000f, fontSize == 0 ? 22f : fontSize + 9f);

                string text = "Scheme: " + UICamera.currentScheme.ToString();
                GUI.skin.label.fontSize = fontSize;
                float lineHeight = GUI.skin.label.lineHeight;
                GUI.color = Color.black;
                GUI.Label(rect, text);
                rect.y -= 1f;
                rect.x -= 1f;
                GUI.color = color;
                GUI.Label(rect, text);
                rect.y += lineHeight + 3f;
                rect.x += 1f;

                text = "Hover: " + NGUITools.GetHierarchy(UICamera.hoveredObject).Replace("\"", "");
                GUI.color = Color.black;
                GUI.Label(rect, text);
                rect.y -= 1f;
                rect.x -= 1f;
                GUI.color = color;
                GUI.Label(rect, text);
                rect.y += lineHeight + 3f;
                rect.x += 1f;

                text = "Selection: " + NGUITools.GetHierarchy(UICamera.selectedObject).Replace("\"", "");
                GUI.color = Color.black;
                GUI.Label(rect, text);
                rect.y -= 1f;
                rect.x -= 1f;
                GUI.color = color;
                GUI.Label(rect, text);
                rect.y += lineHeight + 3f;
                rect.x += 1f;

                text = "Active events: " + UICamera.CountInputSources().ToString();
                if (UICamera.disableController)
                {
                    text += ", disabled controller";
                }
                if (UICamera.ignoreControllerInput)
                {
                    text += ", ignore controller";
                }
                if (UICamera.inputHasFocus)
                {
                    text += ", input focus";
                }
                GUI.color = Color.black;
                GUI.Label(rect, text);
                rect.y -= 1f;
                rect.x -= 1f;
                GUI.color = color;
                GUI.Label(rect, text);
                rect.y += lineHeight + 3f;
                rect.x += 1f;
            }
        }
    }
}
