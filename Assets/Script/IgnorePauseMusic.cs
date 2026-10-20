using UnityEngine;

public class IgnorePauseMusic : MonoBehaviour
{
    void Awake()
    {
        GetComponent<AudioSource>().ignoreListenerPause = true;
    }
}