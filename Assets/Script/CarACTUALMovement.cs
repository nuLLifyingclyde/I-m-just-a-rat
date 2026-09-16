using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarACTUALMovement : MonoBehaviour
{
    public float carspeed = 5f;
    public string RatTag = "Rat";
    public GameObject rat;

    public CarPool carPool;
    private Coroutine deactivateCoroutine;

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
        if (collision.gameObject.CompareTag("Rat"))
        {
            //Destroy(gameObject);
            SceneManager.LoadScene("Trash alleyway");

            Debug.Log("Player is Dead");
        }
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
