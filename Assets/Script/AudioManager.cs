using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicsource;
    [SerializeField] AudioSource SFXSource;      //cheese & bump
    [SerializeField] AudioSource movementSource; //walk, run & set Loop = true

    public AudioClip WalkSFX;
    public AudioClip RunningSFX;
    public AudioClip EatCheeseSFX;
    public AudioClip BumpSFX;

    //pause menu
    void Awake()
    {
        musicsource.ignoreListenerPause = true; //BGM keeps playing while paused
    }

    public void PauseSFX()
    {
        AudioListener.pause = true;
    }

    public void ResumeSFX()
    {
        AudioListener.pause = false;
    }

    //one-shot sfx
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        SFXSource.PlayOneShot(clip, volume);
    }

    //looping movement sfx
    public void PlayMovement(AudioClip clip, float volume = 1f)
    {
        movementSource.volume = volume;

        if (movementSource.clip == clip && movementSource.isPlaying) return;

        movementSource.clip = clip;
        movementSource.loop = true;
        movementSource.Play();
    }

    public void StopMovement()
    {
        movementSource.Stop();
    }
}