using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAttributes : MonoBehaviour
{
    [Header("Needs")]
    [Tooltip("The hunger and health of the animal")]
    [Range(0,100)]
    public int health;
    [Tooltip("The desire for water")]
    [Range(0, 100)]
    public int thirst;
    [Tooltip("The desire for a mate")]
    [Range(0, 100)]
    public int libido;

    [Header("BirthAttributes")]
    [Tooltip("How fast the animal can travel")]
    [Range(30, 70)]
    public int intSpeed;
    public float speed
    {
        get { return (float)intSpeed / 10f; }
    }
    [Tooltip("How much damage the animal does to others")]
    [Range(1, 9)]
    public int strength;


    public GameObject[] allGrass;
    public GameObject nearestGrass;
    public float distance;
    public float nearestDistance = 10000;

    Vector3[] path;
    int targetIndex;
    public Transform target;

    public AnimalAttributes() //default values
    {
        health = 100;
        thirst = 100;
        libido = 100;
        strength = 5;
        intSpeed = 50;
    }

    [ContextMenu("Set Default Values")] //method to assign default values
    public void SetDefaults()
    {
        health = 100;
        thirst = 100;
        libido = 100;
        strength = UnityEngine.Random.Range(1,10);
        intSpeed = (UnityEngine.Random.Range(30,70) / 100);
    }


    #region Animal Methods

    [ContextMenu("Find nearest food")]
    public virtual void LocateFood() //default find food method to be overwritten
    {
        Debug.Log("Finding food");
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    [ContextMenu("Eat nearest food")]
    public virtual void EatFood() //default eat food method to be overwritten
    {
        this.gameObject.GetComponent<AnimalAttributes>().health += 20;

        if (this.gameObject.GetComponent<AnimalAttributes>().health > 100) //keep within 0-100
        {
            this.gameObject.GetComponent<AnimalAttributes>().health -= (this.gameObject.GetComponent<AnimalAttributes>().health - 100);
        }
    }
    #endregion

    #region Start/Update methods
    private void Update()
    {
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();


        if (this.gameObject.GetComponent<AnimalAttributes>().health <= 60) 
        {
            if (nearestGrass != null) //if health meets threshold and the nearest grass has been located, pathfind
            {
                target = nearestGrass.transform;
                this.gameObject.GetComponent<Rabbit>().LocateFood();
            }
            else
            {
                this.gameObject.GetComponent<Rabbit>().GetClosestFood();
            }
        }
    }
    #endregion

    #region Pathfinding

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        Debug.Log("Path found");
        if (pathSuccessful)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = path[0];
        while (true)
        {
            if (transform.position == currentWaypoint)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, (float)speed * Time.deltaTime); //move towards target
            StartCoroutine(waitBeforeEating());
            yield return null;
        }
    }

    IEnumerator waitBeforeEating()
    {
        yield return new WaitForSeconds(3);
        Debug.Log("Eating food");
        this.gameObject.GetComponent<Rabbit>().EatFood();

    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i-1], path[i]);
                }
            }
        }
    }
    #endregion
}
