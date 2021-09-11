using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    float rotationleft = 360;
    float rotationspeed = 10;
    // Update is called once per frame
    void Update()
    {
        float rotation = rotationspeed * Time.deltaTime;

        // if (rotationleft > rotation)
        // {
        //     rotationleft -= rotation;
        // } else {
        //     rotation = rotationleft;
        //     rotationleft = 0;
        // }

        transform.Rotate(0, 0, rotation);
    }
}
