using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAttributes : MonoBehaviour
{
    [Header("Needs")]
    [Tooltip("The hunger and health of the animal")]
    [Range(0, 100)]
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
    public float turnDst = 5;
    public float turnSpeed = 3;
    public float stoppingDst = 10;
    public float speed
    {
        get { return (float)intSpeed / 5f; }
    }
    [Tooltip("How much damage the animal does to others")]
    [Range(1, 9)]
    public int strength;


    public GameObject[] allGrass;
    public GameObject nearestGrass;
    public float distance;
    public float nearestDistance = 10000;


    Path path;

    public Transform target;
    const float pathUpdateMoveThreshold = .5f;
    const float minPathUpdateTime = .2f;
    public bool isFindingFood = false;
    public bool hungry = false;
    public bool stop = false;
    public GameObject randomMovementPrefab;

    Grid grid;

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
        strength = UnityEngine.Random.Range(1, 10);
        intSpeed = UnityEngine.Random.Range(70, 100);
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
    private void FixedUpdate()
    {
        //this.gameObject.GetComponent<Rabbit>().GetClosestFood();

        if (hungry == false)
        {
            CheckHunger();
            if (stop == false)
            {
                RandomMovement();
            }
        }

        if (hungry && !isFindingFood)
        {
            //this.gameObject.GetComponent<Rabbit>().GetClosestFood();
            StartPathfinding();
            hungry = false;
            isFindingFood = true;
        }
    }

    private void CheckHunger() //check if the health value meets the hungry threshold
    {
        if (this.gameObject.GetComponent<AnimalAttributes>().health <= 60)
        {
            Debug.Log("hungry");
            hungry = true;
        }
    }

    private void RandomMovement()
    {
        StartCoroutine(WaitSecondsToStart());

        
        if (stop == false)
        {
            StartCoroutine(WaitSecondsToStop());
            stop = true;
        }

        //stop movement (using bool and if?)
    }

    IEnumerator WaitSecondsToStart()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1, 15)); //wait a random amount of seconds

        GameObject randomPosObj = Instantiate(randomMovementPrefab, this.transform);
        randomPosObj.transform.position = new Vector3(UnityEngine.Random.Range(-240, 240), 50, UnityEngine.Random.Range(-240, 240)); //instantiate empty prefab in random x and z pos within range of island size
        target = randomPosObj.transform; //target = the instantiated prefab
        Debug.Log("random target found");
        Destroy(randomPosObj);

        this.gameObject.GetComponent<Rabbit>().RandomPathfind();
    }
    IEnumerator WaitSecondsToStop()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1, 5)); //wait a random amount of seconds
        stop = true;
    }

    private void StartPathfinding() //call the subroutines required to pathfind towards food
    {
        StartCoroutine(UpdatePath());
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();
        target = nearestGrass.transform;
        Debug.Log("Finding food");
        this.gameObject.GetComponent<Rabbit>().LocateFood();
    }

    #endregion

    #region Pathfinding

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        Debug.Log("Path found");
        if (pathSuccessful)
        {
            path = new Path(waypoints, transform.position, turnDst, stoppingDst); ;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator UpdatePath() //updates the path to ensure it always points towards the foods location
    {
        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }
        PathRequestManager.RequestPath(this.transform.position, target.position, OnPathFound);

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                targetPosOld = target.position;
                this.gameObject.GetComponent<Rabbit>().GetClosestFood();
            }

        }

    }

    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        while (true)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                    pathIndex++;

            if (followingPath)
            {
                if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                    if (speedPercent < 0.01f)
                    {
                        followingPath = false;
                    }
                }

                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }
            StartCoroutine(WaitBeforeEating());
            yield return null;
        }
    }

    IEnumerator WaitBeforeEating()
    {
        yield return new WaitForSeconds(3);
        this.gameObject.GetComponent<Rabbit>().EatFood();
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    }
    #endregion
}
