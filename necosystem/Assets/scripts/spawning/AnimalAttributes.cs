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


    [Header("Pathfinding variables")]
    const float pathUpdateMoveThreshold = .5f;
    const float minPathUpdateTime = .2f;
    public float distance;
    public float nearestDistance = 10000;

    public GameObject[] allGrass;
    public GameObject nearestGrass;
    public Vector3 target;
    public bool isFindingFood = false;
    public bool hungry = false;


    public bool isFindingWater = false;
    public bool thirsty = false;


    [Header("Random movement variables")]
    public int randNum = 0;
    [SerializeField]
    [Tooltip("X / 10,000 chance to wander")]
    public int chance = 1;
    [SerializeField]
    public float minInterval;
    [SerializeField]
    public float maxInterval;
    public float timer;

    [SerializeField]
    public float boundSize = 240f;
    [SerializeField]
    public float height = 50f;
    [SerializeField]
    private bool moving = false;

    private RandomMovement randomMovement;
    private Rabbit rabbit;
    private bool canWander = false;
    Path path;

    [Header("Animator")]
    public Animator animator;

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
        intSpeed = UnityEngine.Random.Range(30, 60);
    }


    #region Animal Methods

    public virtual void LocateFood() //default find food method to be overwritten
    {
        Debug.Log("Finding food");
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
    }

    public virtual void EatFood() //default eat food method to be overwritten
    {
        this.gameObject.GetComponent<AnimalAttributes>().health += 20;

        if (this.gameObject.GetComponent<AnimalAttributes>().health > 100) //keep within 0-100
        {
            this.gameObject.GetComponent<AnimalAttributes>().health -= (this.gameObject.GetComponent<AnimalAttributes>().health - 100);
        }
    }

    public virtual void LocateWater() //default find water method to be overwritten
    {
        Debug.Log("Finding water");
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
    }

    public virtual void DrinkWater() //default drink water method to be overwritten
    {
        this.gameObject.GetComponent<AnimalAttributes>().thirst += 20;

        if (this.gameObject.GetComponent<AnimalAttributes>().thirst > 100) //keep within 0-100
        {
            this.gameObject.GetComponent<AnimalAttributes>().thirst -= (this.gameObject.GetComponent<AnimalAttributes>().thirst - 100);
        }
    }
    #endregion

    #region Start/Update methods
    private void Update()
    {
        if (hungry == false && thirsty == false)
        {
            CheckHunger();
            CheckThirst();
            randomMovement.isHungry = false; //allow animal to wander
            animator.SetBool("RabbitEat", false);

            // checks every frame to see if the timer has reached zero
            if (canWander)
            {
                if (timer <= 0)
                {
                    health -= UnityEngine.Random.Range(3, 5);
                    thirst -= UnityEngine.Random.Range(2, 4);

                    if (!moving)
                    {
                        // if animal is not moving AND the timer has reached zero, determine whether movement happens based off chance
                        int randNum = UnityEngine.Random.Range(1, 10000);
                        if (randNum <= chance)
                        {
                            RandomMovement();
                        }
                    }
                    else //if animal is currently moving and timer reached zero
                    {
                        StopRandomMovement();
                    }
                    timer = UnityEngine.Random.Range(minInterval, maxInterval); // reset the interval timer
                }
                else
                {
                    // otherwise, if the timer is greater than zero, reduce the timer by Time.deltaTime (the time in seconds since the last frame)
                    timer -= Time.deltaTime; // timer counts down
                }
            }
        }

        if (hungry && !isFindingFood)
        {
            StartFoodPathfinding();
            hungry = false;
            isFindingFood = true;
        }

        else if (thirsty && !isFindingWater)
        {
            StartWaterPathfinding();
            thirsty = false;
            isFindingWater = true;
        }
    }

    private void Start()
    {
        randomMovement = this.GetComponent<RandomMovement>();
        rabbit = this.GetComponent<Rabbit>();
        animator = this.GetComponent<Animator>();

        StartCoroutine("DelayForWanderAI");

        minInterval = 1;
        maxInterval = 6;
    }
    #endregion

    #region Pathfinding

    IEnumerator DelayForWanderAI()
    {
        yield return new WaitForSeconds(4);
        canWander = true;
    }

    [ContextMenu("Random Movement")]
    void RandomMovement()
    {
        Debug.Log("starting wander");
        target = new Vector3(UnityEngine.Random.Range(-boundSize, boundSize), height, UnityEngine.Random.Range(-boundSize, boundSize));
        rabbit.LocateFood();

        moving = true;
    }

    void StopRandomMovement()
    {
        Debug.Log("stopping wander");
        StopCoroutine("FollowPath");
        animator.SetBool("RabbitWalking", false);
        animator.SetBool("RabbitEat", false);

        moving = false;
    }

    /*public virtual IEnumerator DelayForRandomMovement()
    {
        Debug.Log("Wrong random movement function");
        yield return null;
    }*/

    private void CheckHunger() //checks if the health value meets the hungry threshold
    {
        if (this.gameObject.GetComponent<AnimalAttributes>().health <= 50)
        {
            Debug.Log("hungry");
            hungry = true;
        }
    }

    private void CheckThirst() //checks if the thirst value meets the thirsty threshold
    {
        if (this.gameObject.GetComponent<AnimalAttributes>().thirst <= 50)
        {
            Debug.Log("thirsty");
            thirsty = true;
        }
    }

    private void StartFoodPathfinding() //calls the required subroutines to pathfind towards food
    {
        StartCoroutine(UpdatePath());
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();
        target = nearestGrass.transform.position;
        Debug.Log("Finding food");
        this.gameObject.GetComponent<Rabbit>().LocateFood();
    }

    private void StartWaterPathfinding() //calls the required subroutines to pathfind towards water
    {
        StartCoroutine(UpdatePath());
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();
        target = nearestGrass.transform.position;
        Debug.Log("Finding food");
        this.gameObject.GetComponent<Rabbit>().LocateFood();
    }

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
        PathRequestManager.RequestPath(this.transform.position, target, OnPathFound);

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target;
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((target - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(transform.position, target, OnPathFound);
                targetPosOld = target;
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
                    if (speedPercent < 0.05f)
                    {
                        followingPath = false;
                    }
                }

                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }
            if (followingPath == false)
            {
                if (hungry == true)
                {
                    animator.SetBool("RabbitEat", true);
                    StartCoroutine(WaitBeforeEating());
                }
                else if (thirsty == true)
                {
                    animator.SetBool("RabbitEat", true);
                    StartCoroutine(WaitBeforeDrinking());
                }
            }
            yield return null;
        }
    }

    IEnumerator WaitBeforeEating()
    {
        animator.SetBool("RabbitWalking", false);
        yield return new WaitForSeconds(3);
        animator.SetBool("RabbitWalking", false);
        this.gameObject.GetComponent<Rabbit>().EatFood();
    }

    IEnumerator WaitBeforeDrinking()
    {
        animator.SetBool("RabbitWalking", false);
        yield return new WaitForSeconds(3);
        animator.SetBool("RabbitWalking", false);
        this.gameObject.GetComponent<Rabbit>().DrinkWater();
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
