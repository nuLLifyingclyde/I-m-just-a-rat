using UnityEngine;

public class BGM : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip music;

    private static BGM instance;

    private void Awake()
    {
        //if a BGM already exists, the new one is deleted
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        //first BGM
        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource.clip = music;
        audioSource.loop = true;
        audioSource.Play();
    }
}