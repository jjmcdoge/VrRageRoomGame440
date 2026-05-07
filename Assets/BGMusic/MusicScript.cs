using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    void Awake()
    {
        // If another MusicManager already exists, destroy this duplicate
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Keep this MusicManager alive when changing scenes
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}