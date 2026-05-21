using UnityEngine;
using UnityEditor;
using GPOyun.Core;

namespace GPOyun.Editor
{
    /// <summary>
    /// Adds a top-level menu item to Unity to automate project setup.
    /// Also attempts to auto-initialize on project load if the scene is empty.
    /// </summary>
    [InitializeOnLoad]
    public static class GPOyunEditorTools
    {
        static GPOyunEditorTools()
        {
            // This runs as soon as Unity finishes compiling/loading
            EditorApplication.delayCall += AutoCheckScene;
        }

        private static void AutoCheckScene()
        {
            if (SessionState.GetBool("GPOyun_AutoStarted", false)) return;

            if (Object.FindAnyObjectByType<GPOyunBootstrap>() == null)
            {
                bool setup = EditorUtility.DisplayDialog("GPOyun: Welcome!", 
                    "No Game Objects found in the current scene. Would you like to automatically initialize the Mediterranean Town Square and Bootstrap hierarchy?", 
                    "YES: Run Cold Start", "No thanks");
                
                if (setup)
                {
                    FullProjectColdStart();
                    SessionState.SetBool("GPOyun_AutoStarted", true);
                }
            }
        }

        [MenuItem("GPOyun/🚀 FULL PROJECT COLD START")]
        public static void FullProjectColdStart()
        {
            // 1. Find or create the [BOOTSTRAP] object
            GPOyunBootstrap bootstrap = Object.FindAnyObjectByType<GPOyunBootstrap>();
            
            if (bootstrap == null)
            {
                GameObject go = new GameObject("[BOOTSTRAP]");
                bootstrap = go.AddComponent<GPOyunBootstrap>();
                Debug.Log("[GPOyun] Created new [BOOTSTRAP] object and added component.");
            }
            else
            {
                Debug.Log("[GPOyun] Found existing [BOOTSTRAP] object.");
            }

            // 2. Select it so the user sees it
            Selection.activeGameObject = bootstrap.gameObject;

            // 3. Trigger the Nuclear Cold Start
            bootstrap.NuclearColdStart();
            
            // 4. Force a scene save so they don't lose it
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            
            Debug.Log("[GPOyun] SUCCESS: Scene reconstruction complete. Press Play!");
        }
    }
}
