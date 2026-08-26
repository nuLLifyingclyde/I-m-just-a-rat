using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DamageTaken : MonoBehaviour
{

    public string CarTag = "Car";
    public Transform SpawnPoint;
    public GameObject Rat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(CarTag))
        {

            SceneManager.LoadScene("Trash alleyway");


            Debug.Log("Player is Dead");

        }


    }
}
