using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartSceneButton : MonoBehaviour
{
    // This public function can be called by a UI Button.
    public void RestartScene()
    {
        // Reset time scale first so the reloaded scene is not frozen.
        Time.timeScale = 1f;

        // Reload the currently active scene.
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}