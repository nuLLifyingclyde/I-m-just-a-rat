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

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        deactivateCoroutine = StartCoroutine(DeactivateCar());
    }

    private void OnDisable()
    {
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
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

        FoodManager.instance.RemoveFood(FoodManager.instance.foodcount);
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
