using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class SimpleAutoSave20Min
{
    // 20 minutes = 1200 seconds
    private const double saveInterval = 1200.0;

    private static double nextSaveTime;

    static SimpleAutoSave20Min()
    {
        // Set the first auto-save time
        nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;

        // Runs while Unity Editor is open
        EditorApplication.update += AutoSaveUpdate;
    }

    private static void AutoSaveUpdate()
    {
        // Do not auto-save while the game is running
        if (EditorApplication.isPlaying)
            return;

        // If 20 minutes have passed, save everything
        if (EditorApplication.timeSinceStartup >= nextSaveTime)
        {
            SaveProject();

            // Schedule the next save
            nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
        }
    }

    private static void SaveProject()
    {
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log("Auto-saved open scenes and project assets.");
    }
}