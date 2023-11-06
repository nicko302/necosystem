using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovementObjectRaycast : MonoBehaviour
{
    void Start()
    {
        FindLand();
    }

    public void FindLand()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            if (hit.point.y < 3)
            {
                Destroy(gameObject);
            }
        }
    }
}
