using UnityEngine;

public class BGM : MonoBehaviour
{
    // Audio Source
    [SerializeField] private AudioSource audioSource;

    // Audio Clip
    [SerializeField] private AudioClip music;

    private void Awake()
    {
        audioSource.clip = music;
        audioSource.loop = true;
        audioSource.Play();

        DontDestroyOnLoad(gameObject);
    }
}