using UnityEngine;
using System.Collections.Generic;

public class CarPool : MonoBehaviour
{
    public GameObject prefab_1;
    public GameObject prefab_2;
    public GameObject prefab_3;

    public int carsPerPrefab = 10;

    private List<GameObject> pool = new List<GameObject>();

    void Start()
    {
        CreateCars(prefab_1);
        CreateCars(prefab_2);
        CreateCars(prefab_3);
    }

    void CreateCars(GameObject prefab)
    {
        for (int i = 0; i < carsPerPrefab; i++)
        {
            GameObject car = Instantiate(prefab);
            car.SetActive(false);
            CarACTUALMovement movement = car.GetComponent<CarACTUALMovement>();

            if (movement != null)
            {
                movement.carPool = this;
            }

            pool.Add(car);
        }
    }

    public GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        foreach (GameObject car in pool)
        {
            if (!car.activeSelf && car.name.StartsWith(prefab.name))
            {
                car.transform.position = position;
                car.transform.rotation = rotation;
                car.SetActive(true);
                return car;
            }
        }

        Debug.Log("No available cars in pool!");
        return null;
    }

    public void ReturnObject(GameObject car)
    {
        car.SetActive(false);
    }
}