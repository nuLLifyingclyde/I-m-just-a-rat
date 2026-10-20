using UnityEngine;

public class AudioManager : MonoBehaviour
{
   [SerializeField] AudioSource musicsource;
    [SerializeField] AudioSource SFXSource;

    public AudioClip WalkSFX;
    public AudioClip RunningSFX;
    public AudioClip EatCheeseSFX;
    public AudioClip BumpSFX;
    public AudioClip CarSFX;
    public AudioClip BarrelSFX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
   public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
