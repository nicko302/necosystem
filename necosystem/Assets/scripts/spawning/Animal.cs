using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Animal : MonoBehaviour
{
    [Header("Needs")]
    [Tooltip("The hunger and health of the animal")]
    [Range(0, 100)]
    public int health;
    /*************************************** THIRST
    [Tooltip("The desire for water")]
    [Range(0, 100)]
    public int thirst;
    *///////////////////////////////////////
    [Tooltip("The desire for a mate")]
    [Range(0, 100)]
    public int libido;

    [Header("BirthAttributes")]
    [Tooltip("How fast the animal can travel")]
    [Range(30, 70)]
    public int intSpeed; // an easier value of speed to compare and alter

    public float turnDst = 5;
    public float turnSpeed = 3;
    public float stoppingDst = 10;
    public float speed // the actual value of speed to be used
    {
        get { return (float)intSpeed / 5f; }
    }
    [Tooltip("How much damage the animal can do to others")]
    [Range(1, 9)]
    public int strength;
    public bool dead;


    [Header("Pathfinding variables")]
    public Vector3 target;

    protected const float pathUpdateMoveThreshold = .5f;
    protected const float minPathUpdateTime = .2f;
    [SerializeField]

    protected float distance;
    [SerializeField]
    protected float nearestDistance = 10000;

    [SerializeField]
    protected GameObject[] allGrass;
    protected GameObject nearestGrass;
    public bool isFindingFood = false;
    public bool isHungry = false;

    public bool isLookingForMate = false;
    public bool readyToMate = false;
    public GameObject nearestMate;
    public List<GameObject> allPotentialMates;
    /*************************************** water pathfinding variables
    public bool isFindingWater = false;
    public bool thirsty = false;
    *///////////////////////////////////////


    [Header("Wander variables")]
    protected int randNum = 0;
    [SerializeField]
    [Tooltip("X / 10,000 chance to wander")]
    protected int chance = 1;
    [SerializeField]
    protected float minInterval;
    [SerializeField]
    protected float maxInterval;
    protected float timer;

    [SerializeField]
    protected float boundSize = 240f;
    [SerializeField]
    protected float height = 50f;
    public bool moving = true;

    private Rabbit rabbit;
    Path path;

    [Header("Animator")]
    public Animator animator;

    [Header("Other")]
    [SerializeField]
    private bool startFunctions = false;
    [SerializeField]
    private bool runOnce = false;


    [ContextMenu("Set Default Values")] // method to assign default values for debugging
    public void SetDefaults()
    {
        health = 100;
        //thirst = 100;
        libido = 100;
        strength = UnityEngine.Random.Range(1, 10);
        intSpeed = UnityEngine.Random.Range(30, 60);
    }


    #region Animal functions

    public virtual void Pathfind() //default find food method to be overwritten
    {
        Debug.Log("Finding food");
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
    }

    public virtual void EatFood() //default eat food method to be overwritten
    {
        this.gameObject.GetComponent<Animal>().health += 20;

        if (this.gameObject.GetComponent<Animal>().health > 100) //keep within 0-100
        {
            this.gameObject.GetComponent<Animal>().health -= (this.gameObject.GetComponent<Animal>().health - 100);
        }
    }

    public virtual void Die()
    {
        Debug.Log("dead");

        // stop animal from pathfinding
        isHungry = false; isFindingFood = true; moving = true;
        StopCoroutine("DelayForWanderAI"); StopCoroutine("FollowPath");

        // stop current animations

        // die animation

        StartCoroutine("DestroyDelay");
    }
    public virtual IEnumerator DestroyDelay()
    {
        yield return new WaitForSeconds(3);
        Destroy(this.gameObject);
    }

    /************************ drink water

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
    */////////////////////////
    #endregion

    #region Start/Update methods
    private void Update()
    {
        if (startFunctions)
        {
            if (!(isHungry || isLookingForMate))
            {
                CheckHunger();
                CheckLibido();
                CheckDeath();

                // checks every frame to see if the timer has reached zero
                if (timer <= 0)
                {
                    // decrements needs values
                    health -= UnityEngine.Random.Range(3, 5);
                    //libido -= UnityEngine.Random.Range(2, 3);
                    //thirst -= UnityEngine.Random.Range(2, 4);

                    if (!isHungry)
                    {
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
                    }
                    timer = UnityEngine.Random.Range(minInterval, maxInterval); // reset the interval timer
                }
                else
                {
                    // otherwise, if the timer is greater than zero, reduce the timer by Time.deltaTime (the time in seconds since the last frame)
                    timer -= Time.deltaTime; // timer counts down
                }
            }

            


            if (isHungry && !isFindingFood && !isLookingForMate)
            {
                StartFoodPathfinding();
                moving = true;
                isHungry = false;
                isFindingFood = true;
            }

            else if (readyToMate && !isLookingForMate && !isHungry && !isFindingFood)
            {
                FindNearestMate(); // locate the nearest potential mate
                if (nearestMate != null) // only pathfinds to a potential mate if the mate is also ready to mate
                    StartMatePathfinding();

                moving = true;
                readyToMate = false;
                isLookingForMate = true;
            }

            if (dead)
            {
                Die();
            }
        }
    }

    private void Start()
    {
        rabbit = this.GetComponent<Rabbit>();
        animator = this.GetComponent<Animator>();

        StartCoroutine("DelayForWanderAI");

        minInterval = 1;
        maxInterval = 6;

        startFunctions = false;
    }
    #endregion

    #region Pathfinding

    IEnumerator DelayForWanderAI()
    {
        yield return new WaitForSeconds(4);
        startFunctions = true;
    }

    [ContextMenu("Random Movement")]
    void RandomMovement()
    {
        Debug.Log("starting wander");
        target = new Vector3(UnityEngine.Random.Range(-boundSize, boundSize), height, UnityEngine.Random.Range(-boundSize, boundSize));
        rabbit.Pathfind();

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

    private void CheckHunger() //checks if the health value meets the isHungry threshold
    {
        if (this.gameObject.GetComponent<Animal>().health <= 50 && this.gameObject.GetComponent<Animal>().health > 15)
        {
            Debug.Log("hungry");
            isHungry = true;
        }

        //if(this.gameObject.GetComponent<Animal>().health < 35)
    }

    private void CheckLibido() //checks if the libido value meets the ready to mate threshold
    {
        if (this.gameObject.GetComponent<Animal>().libido <= 30)
        {
            Debug.Log("ready to mate");
            readyToMate = true;
        }
    }
    private void CheckDeath() //checks if the health value meets the isHungry threshold
    {
        if (this.gameObject.GetComponent<Animal>().health <= 10)
        {
            dead = true;
        }
    }

    public virtual void FindNearestMate()
    {
        nearestMate = null;
        allPotentialMates = null;

        allPotentialMates.AddRange(GameObject.FindGameObjectsWithTag("Rabbit")); // adds all animals to list
        for (int i = 0; i < allPotentialMates.Count; i++)
        {
            if (!allPotentialMates[i].gameObject.GetComponent<Animal>().readyToMate || allPotentialMates[i].gameObject.GetComponent<Animal>().isLookingForMate)
            {
                allPotentialMates.RemoveAt(i); // removes the animal from the list of mates if they are not also ready to mate or are currently looking for one
                i--;
            }
        }

        distance = 0;
        nearestDistance = 10000;

        for (int i = 0; i < allPotentialMates.Count; i++)
        {
            distance = Vector3.Distance(this.transform.position, allGrass[i].transform.position);

            if (distance < nearestDistance)
            {
                nearestMate = allPotentialMates[i];
                nearestDistance = distance;
            }
        }
    }

    /************************* CheckThirst & StartWaterPathfinding
    private void CheckThirst() //checks if the thirst value meets the thirsty threshold
    {
        if (this.gameObject.GetComponent<AnimalAttributes>().thirst <= 50)
        {
            Debug.Log("thirsty");
            thirsty = true;
        }
    }

    private void StartWaterPathfinding() //calls the required subroutines to pathfind towards water
    {
        StartCoroutine(UpdatePath());
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();
        target = nearestGrass.transform.position;
        Debug.Log("Finding food");f
        this.gameObject.GetComponent<Rabbit>().Pathfind();
    }

    *//////////////////////////

    private void StartFoodPathfinding() //calls the required subroutines to pathfind towards food
    {

        StartCoroutine(UpdatePath());
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();
        target = nearestGrass.transform.position;
        Debug.Log("Finding food");
        this.gameObject.GetComponent<Rabbit>().Pathfind();
    }

    private void StartMatePathfinding() //calls the required subroutines to pathfind towards a mate
    {
        StartCoroutine(UpdatePath());
        target = nearestMate.transform.position;
        Debug.Log("Finding mate");
        this.gameObject.GetComponent<Rabbit>().Pathfind();
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

    /// <summary>
    /// Updates the path to ensure it always points towards the correct target location
    /// </summary>
    IEnumerator UpdatePath()
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
                this.gameObject.GetComponent<Rabbit>().FindNearestMate();
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
            runOnce = true;

            if (followingPath == false)
            {
                if (isHungry && runOnce)
                {
                    animator.SetBool("RabbitEat", true);
                    StartCoroutine(WaitBeforeEating());
                    runOnce = false;
                }
                /********************************** Tell rabbit to Drink water
                else if (thirsty == true)
                {
                    animator.SetBool("RabbitEat", true);
                    StartCoroutine(WaitBeforeDrinking());
                }
                *//////////////////////////////////
            }
            yield return null;
        }
    }

    IEnumerator WaitBeforeEating()
    {
        runOnce = false;

        animator.SetBool("RabbitWalking", false);
        yield return new WaitForSeconds(3);
        animator.SetBool("RabbitWalking", false);
        this.gameObject.GetComponent<Rabbit>().EatFood();
    }

    /**********************
    IEnumerator WaitBeforeDrinking()
    {
        animator.SetBool("RabbitWalking", false);
        yield return new WaitForSeconds(3);
        animator.SetBool("RabbitWalking", false);
        this.gameObject.GetComponent<Rabbit>().DrinkWater();
    }
    *///////////////////////

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    }
    #endregion
}
