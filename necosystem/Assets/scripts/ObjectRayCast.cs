using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectRayCast : MonoBehaviour
{
    public GameObject obj;
    void Start()
    {
        FindLand();
    }

    public void FindLand()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hitInfo;

        if (transform.position.y > 20)
        {
            if (Physics.Raycast(ray, out hitInfo)) //if theres ground beneath, set object position on ground
            {
                transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
            }
            else
            {
                ray = new Ray(transform.position, transform.up);
                if (Physics.Raycast(ray, out hitInfo))
                {
                    transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z); //if no ground beneath, but there is ground above - set position there
                }
            }
        }
        else
        {
            obj.SetActive(false);
        }
    }
}
