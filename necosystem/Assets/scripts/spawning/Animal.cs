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
    [Range(7, 11)]
    public int lifespan;
    [Range(0, 11)]
    public int age;
    public int ageCounter;
    public bool dead;


    [Header("Pathfinding variables")]
    public Vector3 target;

    public bool exceptionCaught;

    protected const float pathUpdateMoveThreshold = .5f;
    protected const float minPathUpdateTime = .2f;
    [SerializeField]
    protected float distance;
    [SerializeField]
    protected float nearestDistance = 10000;

    [Header("Eating variables")]
    [SerializeField]
    protected GameObject[] allGrass;
    [SerializeField]
    protected GameObject nearestGrass;
    public bool isFindingFood = false;
    public bool isHungry = false;

    [Header("Mating variables")]
    public bool isBaby = false;
    public bool mateFound = false;
    public bool readyToMate = false;
    public bool mateConditionsMet = false;
    public GameObject nearestMate;
    public List<GameObject> allPotentialMates;
    public float dstFromMate;
    protected const int posOffset = 1;
    [SerializeField]
    protected GameObject babyPrefab;


    [Header("Wander variables")]
    [SerializeField]
    [Tooltip("X / 10,000 chance to wander")]
    protected int chance = 1;
    protected int randNum = 0;
    [SerializeField]
    protected float minInterval;
    [SerializeField]
    protected float maxInterval;
    protected float timer;

    [SerializeField]
    protected float boundSize = 240f;
    [SerializeField]
    protected float height = 50f;
    public bool moving = false;

    private Rabbit rabbit;
    public bool canWander = false;
    Path path;

    [Header("Animator")]
    public Animator animator;

    [Header("Other")]
    public bool runOnce = true;

    Grid grid;

    [ContextMenu("Set Default Values")] // method to assign default values for debugging
    public void SetDefaults()
    {
        health = UnityEngine.Random.Range(80, 100);
        libido = UnityEngine.Random.Range(80, 100);
        lifespan = UnityEngine.Random.Range(7, 11);
        age = UnityEngine.Random.Range(3, 5);
        ageCounter = UnityEngine.Random.Range(0, 3);

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
        isHungry = false; isFindingFood = true; moving = true; canWander = false;
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
        if (!(isHungry || mateFound)) // if the animal does not currently have a strong need
        {
            CheckHunger();
            CheckLibido();
            CheckDeath();
            animator.SetBool("RabbitEat", false);

            // checks every frame to see if the timer has reached zero
            if (canWander)
            {
                if (timer <= 0)
                {
                    health -= UnityEngine.Random.Range(4, 7);
                    if (!isBaby)
                        libido -= UnityEngine.Random.Range(5, 9);
                    else
                        libido = 100;

                    if (ageCounter < 4)
                    {
                        ageCounter += 1;
                    }
                    else
                    {
                        ageCounter = 0;
                        age += 1;
                    }


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

                    if (isBaby)
                    {
                        // when timer reaches zero, and until the baby has reached full size, its scale will increase
                        if (transform.localScale.x < 0.63)
                        {
                            transform.localScale += new Vector3(.09f, .09f, .09f);
                        }
                        else
                        {
                            isBaby = false;
                        }
                    }

                    //timer = UnityEngine.Random.Range(minInterval, maxInterval); // reset the interval timer
                    timer = 6;
                }
                else
                {
                    // otherwise, if the timer is greater than zero, reduce the timer by Time.deltaTime (the time in seconds since the last frame)
                    timer -= Time.deltaTime; // timer counts down
                }
            }
        }

        if (isHungry && !isFindingFood)
        {
            StartFoodPathfinding();
            isHungry = false;
            isFindingFood = true;
        }
        else if (isHungry && isFindingFood && nearestGrass == null)
        {
            StartFoodPathfinding(); // finds a new grass if current one has been eaten
        }

        if (readyToMate && !mateFound && !isHungry && !isFindingFood)
        {
            mateConditionsMet = true;
        }
        else
        {
            mateConditionsMet = false;
        }

        if (mateConditionsMet)
        {
            FindNearestMate(); // locate the nearest potential mate
            if (nearestMate != null) // only pathfinds to a potential mate if the mate is also ready to mate
            {
                StartMatePathfinding();
                moving = true;
                readyToMate = false;
                mateFound = true;
            }
        }

        if (age > 2)
        {
            isBaby = false;
        }
        if (age == lifespan)
        {
            Die();
        }

        /************************************* start water pathfinding
        else if (thirsty && !isFindingWater)
        {
            StartWaterPathfinding();
            thirsty = false;
            isFindingWater = true;
        }
        */////////////////////////////////////

        if (dead)
        {
            Die();
        }
    }

    private void Start()
    {
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
            if (!allPotentialMates[i].gameObject.GetComponent<Animal>().mateConditionsMet)
            {
                allPotentialMates.RemoveAt(i); // removes the animal from the list of mates if they do not meet the mate conditions
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
        //StartCoroutine(UpdatePath());
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();
        try
        {
            target = nearestGrass.transform.position;
        }
        catch
        {
            this.gameObject.GetComponent<Rabbit>().GetClosestFood();
            target = nearestGrass.transform.position;
        }
        Debug.Log("Finding food");

        animator.SetBool("RabbitWalking", true);

        Debug.Log("-1");

        animator.SetBool("RabbitEat", false);

        Debug.Log("0");

        this.gameObject.GetComponent<Rabbit>().Pathfind();
    }

    private void StartMatePathfinding() //calls the required subroutines to pathfind towards a mate
    {
        try
        {
            target = nearestMate.transform.position;
        }
        catch
        {
            allPotentialMates = null;
            this.gameObject.GetComponent<Rabbit>().FindNearestMate();
            target = nearestMate.transform.position;
        }
        Debug.Log("Finding mate");

        animator.SetBool("RabbitWalking", true);
        animator.SetBool("RabbitEat", false);

        StartCoroutine("UpdatePath");

        //this.gameObject.GetComponent<Rabbit>().Pathfind();
    }

    public virtual void Mate()
    {
        Debug.Log("mating...");
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

    public virtual IEnumerator UpdatePath() // updates the path to ensure it always points towards the target location
    {
        Debug.Log("1");

        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }

        Debug.Log("2");

        PathRequestManager.RequestPath(transform.position, target, OnPathFound);

        Debug.Log("3");

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target;
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((target - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                Debug.Log("4");

                PathRequestManager.RequestPath(transform.position, target, OnPathFound);
                targetPosOld = target;
                if (isFindingFood)
                    gameObject.GetComponent<Rabbit>().GetClosestFood();
                if (mateFound)
                    gameObject.GetComponent<Rabbit>().FindNearestMate();

                Debug.Log("5");
            }
        }
    }

    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        try
        {
            transform.LookAt(path.lookPoints[0]);
        }
        catch
        {
            Die();
        }

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
                if (isHungry)
                {
                    animator.SetBool("RabbitEat", true);
                    StartCoroutine(WaitBeforeEating());
                }
                else if (mateFound)
                {
                    if (runOnce)
                    Debug.Log("#Close to mate");
                    StartCoroutine(WaitBeforeMating());
                    StopCoroutine("FollowPath");
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
        Debug.Log("WaitBeforeEating");
        animator.SetBool("RabbitWalking", false);
        yield return new WaitForSeconds(3);
        animator.SetBool("RabbitWalking", false);
        this.gameObject.GetComponent<Rabbit>().EatFood();
    }

    protected IEnumerator WaitBeforeMating()
    {
        Debug.Log("WaitBeforeMating");
        animator.SetBool("RabbitWalking", true);
        yield return new WaitForSeconds(3);
        this.gameObject.GetComponent<Rabbit>().Mate();
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
