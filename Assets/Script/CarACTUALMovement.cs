using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarACTUALMovement : MonoBehaviour
{
    public float carspeed = 5f;
    public string RatTag = "Rat";
    public GameObject rat;
    public FoodManager fm;
    public float slowspeed = 1f;

    public CarPool carPool;
    private Coroutine deactivateCoroutine;
    public MouseDirectionController mdc;

    public AudioManager audioManager;
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioManager == null)
            audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void OnEnable()
    {
        deactivateCoroutine = StartCoroutine(DeactivateCar());

        if (audioSource != null)
        {
            audioSource.time = Random.Range(0f, audioSource.clip.length); 
            audioSource.Play();
        }
    }

    private void OnDisable()
    {
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0, 0, carspeed);
        //transform.Translate(0, 0, carspeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(RatTag))
        {
            return;
        }

        // Survivable now: the rat is knocked into a ragdoll and recovers, instead of the hit
        // reloading the scene.
        Ragdoll ragdoll = collision.gameObject.GetComponent<Ragdoll>();

        if (ragdoll == null || !ragdoll.CanBeHit)
        {
            return;
        }

        audioManager.PlaySFX(audioManager.BumpSFX, 0.4f);

        FoodManager.instance.RemoveFood(FoodManager.instance.foodcount - 10);
        mdc.moveSpeed -= slowspeed;
        

        Debug.Log("Player gets slowed down");

        ragdoll.HitByCar(transform.position, carspeed);

        Debug.Log("Player got hit by a car");


    }

    IEnumerator DeactivateCar()
    {
        yield return new WaitForSeconds(15f);
        
        if (carPool != null)
        {
            carPool.ReturnObject(gameObject);
        }
    }
}
