using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AnimalRayCast : MonoBehaviour
{
    public AnimalAttributes animalAttributes;
    public GameObject obj;
    void Start()
    {
        FindLand();
        animalAttributes.SetDefaults();
    }
 
    public void FindLand()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position, -transform.up, Color.green);
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            if (hit.point.y > 4)
            {
                transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
}
