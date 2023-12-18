using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RegrownGrassRaycast : MonoBehaviour
{
    [Header("Grass grow")]
    [SerializeField]
    private float timer = 0;
    [SerializeField]
    private float minInterval = 3;
    [SerializeField]
    private float maxInterval = 6;

    void Start()
    {
        FindLand();
    }

    private void Update()
    {
        if (timer <= 0)
        {
            // if timer reaches zero, and until the grass has reached full size, its scale will increase
            if (transform.localScale.x <= 1.6)
            {
                transform.localScale += new Vector3(.1f, .1f, .1f);
            }
            timer = UnityEngine.Random.Range(minInterval, maxInterval); // reset the interval timer
        }
        else
        {
            // otherwise, if the timer is greater than zero, reduce the timer by Time.deltaTime (the time in seconds since the last frame)
            timer -= Time.deltaTime; // timer counts down
        }
    }
        
    public void FindLand()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position, -transform.up, Color.green);
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            if (hit.point.y > 5)
            {
                transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                transform.position = transform.position + new Vector3(Random.Range(2,5), 20, Random.Range(2,5));
                FindLand();
            }
        }
    }
}
