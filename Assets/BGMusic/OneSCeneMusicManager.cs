using UnityEngine;

public class OneSceneMusicManager : MonoBehaviour
{
    public AudioSource lobbyMusic;
    public AudioSource gameplayMusic;

    void Start()
    {
        Debug.Log("Music Manager started.");

        if (lobbyMusic == null)
        {
            Debug.LogError("Lobby Music is NOT assigned.");
            return;
        }

        if (gameplayMusic == null)
        {
            Debug.LogError("Gameplay Music is NOT assigned.");
            return;
        }

        Debug.Log("Starting lobby music.");

        lobbyMusic.loop = true;
        lobbyMusic.volume = 1f;
        lobbyMusic.mute = false;
        lobbyMusic.spatialBlend = 0f;
        lobbyMusic.Play();

        gameplayMusic.Stop();
    }

    public void StartGame()
    {
        Debug.Log("Start button pressed. Switching music.");

        lobbyMusic.Stop();

        gameplayMusic.loop = true;
        gameplayMusic.volume = 1f;
        gameplayMusic.mute = false;
        gameplayMusic.spatialBlend = 0f;
        gameplayMusic.Play();
    }
}