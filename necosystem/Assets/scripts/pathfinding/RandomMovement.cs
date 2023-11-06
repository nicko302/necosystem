using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class RandomMovement : MonoBehaviour
{
    public float movementSpeed = 20f;
    public float rotationSpeed = 100f;

    public float objCheckDiameter;

    [HideInInspector] public bool isWandering = false;
    [HideInInspector] public bool isHungry = false;
    private bool isRotatingLeft = false;
    private bool isRotatingRight = false;
    public bool isWalking = false;

    public LayerMask unwalkableMask;
    public LayerMask seaMask;
    public LayerMask terrainChunkMask;
    public GameObject posForSeaRaycast;

    Rigidbody rb;
    Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!isWandering && !isHungry)
        {
            StartCoroutine(Wander());
        }

        if (isRotatingRight)
        {
            transform.Rotate(transform.up * Time.deltaTime * rotationSpeed);
        }
        if (isRotatingLeft)
        {
            transform.Rotate(transform.up * Time.deltaTime * -rotationSpeed);
        }

        if (isWalking)
        {
            rb.transform.position += transform.forward * movementSpeed/100;
            //animator.SetBool("isWalking", true);
            
            /*****************************
            RaycastHit hit;
            Debug.DrawRay(posForSeaRaycast.transform.position, -transform.up, Color.green, 2.5f);
            if (Physics.Raycast(posForSeaRaycast.transform.position, -transform.up, out hit)) //raycast to check proximity to the sea
            {
                if (hit.transform.position.y < 1)
                {
                    Debug.Log("is near to sea");
                    isWalking = false;
                }
                else
                {
                    Debug.Log("is not near to sea");
                    isWalking = true;
                }
            }
            
            if (Physics.Raycast(transform.position, transform.forward, 3f, unwalkableMask)) //raycast forward to check for obstacles
            {
                isWalking = false;
            }
            *//////////////////////////
        }
        else
        {
            //animator.SetBool("isWalking", false);
        }
    }

    IEnumerator Wander()
    {
        float rotationTime = Random.Range(.5f, 1f);
        int rotateWait = Random.Range(1, 3);
        int rotateDirection = Random.Range(1, 3);
        float walkWait = Random.Range(.5f, 2f);
        int walkTime = Random.Range(1, 3);

        isWandering = true;

        yield return new WaitForSeconds(walkWait);
        /*********************************************
        if (Physics.Raycast(transform.position, transform.forward, 3f, unwalkableMask)) //if there is an obstacle in front, don't walk forward
        {
            isWalking = false;
        }
        else if (Physics.Raycast(posForSeaRaycast.transform.position, -transform.up, 2f, seaMask)) //if the sea is in front, don't walk forward
        {
            isWalking = false;
        }
        else //if anything other than sea or obstacle (i.e. only terrain), can walk forward
        {
            isWalking = true;
        }
        *////////////////////////////////////////////////
        isWalking = true;

        yield return new WaitForSeconds(walkTime);
        isWalking = false;

        yield return new WaitForSeconds(rotateWait);

        if (rotateDirection == 1)
        {
            isRotatingLeft = true;
            yield return new WaitForSeconds(rotationTime);
            isRotatingLeft = false;
        }
        if (rotateDirection == 2)
        {
            isRotatingRight = true;
            yield return new WaitForSeconds(rotationTime);
            isRotatingRight = false;
        }

        isWandering = false;
    }
}