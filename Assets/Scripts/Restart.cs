using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class RestartSceneButton : MonoBehaviour
{
    [Header("VR Controller Restart Input")]
    [Tooltip("Assign the B button / Secondary Button input action here.")]
    public InputActionProperty bButtonAction;

    private void OnEnable()
    {
        if (bButtonAction.action != null)
        {
            bButtonAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (bButtonAction.action != null)
        {
            bButtonAction.action.Disable();
        }
    }

    private void Update()
    {
        if (bButtonAction.action != null && bButtonAction.action.WasPressedThisFrame())
        {
            RestartScene();
        }
    }

    // This public function can still be called by your UI Restart Button.
    public void RestartScene()
    {
        // Reset time scale first so the reloaded scene is not frozen.
        Time.timeScale = 1f;

        // Reload the currently active scene.
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}