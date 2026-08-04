using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement: MonoBehaviour
{

    public float speed = 5f;
    public float maxDistance = 10f;
    private float startXPosition;



    private void Start()
    {
        startXPosition = transform.position.x;
    }

    void Update()
    {

        float screenCenterNormalized = (Input.mousePosition.x - (Screen.width / 2f)) / (Screen.width / 2f);
        float targetX = startXPosition + (screenCenterNormalized * maxDistance);
        Vector3 targetedPoint = transform.position;
        targetedPoint.x = targetX;
        transform.position = Vector3.MoveTowards(transform.position, targetedPoint, speed * Time.deltaTime);
    }

   
}