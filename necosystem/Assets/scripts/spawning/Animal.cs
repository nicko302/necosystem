using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Animal : MonoBehaviour
{
    #region Variables
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
    public bool isFindingFood = false;
    public bool isHungry = false;
    [SerializeField]
    protected GameObject[] allFoodItems;
    [SerializeField]
    protected GameObject nearestFoodItem;
    protected bool beingHunted;

    [Header("Mating variables")]
    public bool isBaby = false;
    public bool mateFound = false;
    public bool readyToMate = false;
    public bool mateConditionsMet = false;
    public GameObject nearestMate;
    public List<GameObject> allPotentialMates;
    public float dstFromMate;
    protected const float posOffset = 2f;
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

    public bool canWander = false;
    Path path;

    [Header("Animator")]
    public Animator animator;

    [Header("Other")]
    public bool runOnce = true;

    Grid grid;
    #endregion

    #region Animal functions

    [ContextMenu("Set Default Values")] // method to assign default values for debugging
    public void SetDefaults()
    {
        health = UnityEngine.Random.Range(90, 100);
        libido = UnityEngine.Random.Range(90, 100);
        if (GetComponent<Rabbit>() != null) { lifespan = UnityEngine.Random.Range(7, 11); }
        else if (GetComponent<Fox>() != null) { lifespan = UnityEngine.Random.Range(4, 7); }
        
        age = UnityEngine.Random.Range(1, 2);
        ageCounter = UnityEngine.Random.Range(0, 3);

        strength = UnityEngine.Random.Range(1, 10);

        if (GetComponent<Rabbit>() != null) { intSpeed = UnityEngine.Random.Range(30, 60); }
        else if (GetComponent<Fox>() != null) { intSpeed = UnityEngine.Random.Range(40, 70); }
    }

    public virtual void EatFood() //default eat food method to be overwritten
    {
        this.gameObject.GetComponent<Animal>().health += 20;

        if (this.gameObject.GetComponent<Animal>().health > 100) //keep within 0-100
        {
            this.gameObject.GetComponent<Animal>().health -= (this.gameObject.GetComponent<Animal>().health - 100);
        }
    }

    public virtual void Mate()
    {
        Debug.Log("mating...");
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

    IEnumerator WaitBeforeEating()
    {
        Debug.Log("WaitBeforeEating");
        animator.SetBool("Walking", false);
        yield return new WaitForSeconds(3);
        animator.SetBool("Walking", false);
        EatFood();
    }

    protected IEnumerator WaitBeforeMating()
    {
        runOnce = false;
        Debug.Log("WaitBeforeMating");
        animator.SetBool("Walking", false);
        yield return new WaitForSeconds(3);
        Mate();
    }

    #endregion

    #region Start/Update methods
    private void Start()
    {
        StartCoroutine("DelayForWanderAI");

        minInterval = 1;
        maxInterval = 6;
    }

    private void Update()
    {
        if (!(isHungry || mateFound)) // if the animal does not currently have a strong need
        {
            CheckHunger();
            CheckLibido();
            CheckDeath();

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
                            transform.localScale += new Vector3(.04f, .04f, .04f);
                        }
                        else
                        {
                            isBaby = false;
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
        }

        if (isHungry && !isFindingFood)
        {
            StartFoodPathfinding();
            isHungry = false;
            isFindingFood = true;
        }
        else if (isHungry && isFindingFood && nearestFoodItem != null)
        {
            StopCoroutine(FollowPath());
            nearestFoodItem = null;
            StartFoodPathfinding();
            isHungry = false;
            isFindingFood = true;
        }
        else if (isHungry && isFindingFood && nearestFoodItem == null)
        {
            StartFoodPathfinding(); // finds a new grass if current one has been eaten
        }
        else if (isHungry && isFindingFood && nearestFoodItem != null && !moving)
        {
            StopCoroutine(FollowPath());
            nearestFoodItem = null;
            GetClosestFood();
            StartFoodPathfinding();
        }

        if (readyToMate && !mateFound && !isHungry && !isFindingFood)
        {
            Debug.Log("mate conditions met should be true now");
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
                readyToMate = false;
                mateFound = true;
            }
        }

        if (GetComponent<Rabbit>() != null)
        {
            if (age > 2)
            {
                isBaby = false;
            }
        }
        else if (GetComponent<Fox>() != null)
        {
            if (age > 1)
            {
                isBaby = false;
            }
        }

        if (beingHunted)
        {
            intSpeed += 10;
            StopCoroutine("FollowPath");
            RandomMovement();
        }

        if (age == lifespan)
        {
            Die();
        }

        if (moving)
            animator.SetBool("Walking", true);
        else
            animator.SetBool("Walking", false);

        if (dead)
        {
            Die();
        }
    }

    #endregion

    #region Check needs functions
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

    #endregion

    #region Wander functions

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
        Pathfind();
    }

    void StopRandomMovement()
    {
        moving = false;

        Debug.Log("stopping wander");
        StopCoroutine("FollowPath");
        animator.SetBool("Walking", false);
        animator.SetBool("Eat", false);
    }

    #endregion

    #region Initial pathfinding methods
    public virtual void Pathfind() //default find food method to be overwritten
    {
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
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
            distance = Vector3.Distance(this.transform.position, allFoodItems[i].transform.position);

            if (distance < nearestDistance)
            {
                nearestMate = allPotentialMates[i];
                nearestDistance = distance;
            }
        }
    }

    public virtual void GetClosestFood()
    {

    }

    private void StartFoodPathfinding() //calls the required subroutines to pathfind towards food
    {
        //StartCoroutine(UpdatePath());
        GetClosestFood();
        bool doReturn = false;
        try
        {
            target = nearestFoodItem.transform.position;
        }
        catch
        {
            try
            {
                GetClosestFood();
                target = nearestFoodItem.transform.position;
            }
            catch
            {
                doReturn = true;
            }
        }
        if (doReturn)
        {
            return;
        }
        Debug.Log("Finding food");

        animator.SetBool("Walking", true);

        Debug.Log("-1");

        animator.SetBool("Eat", false);

        Debug.Log("0");

        Pathfind();
    }

    private void StartMatePathfinding() //calls the required subroutines to pathfind towards a mate
    {
        moving = true;
        Debug.Log("Finding mate");

        animator.SetBool("Walking", true);
        animator.SetBool("Eat", false);

        Debug.Log("!!!!!!!!!!!");
        StartCoroutine(UpdatePath());

        //this.gameObject.GetComponent<Rabbit>().Pathfind();
    }

    #endregion

    #region Pathfinding methods

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        Debug.Log("Path found");
        if (pathSuccessful)
        {
            try
            {
                path = new Path(waypoints, transform.position, turnDst, stoppingDst); ;
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
            catch
            {
                StopCoroutine(FollowPath());
                moving = false;
                mateFound = false;
                readyToMate = true;
                mateConditionsMet = true;
            }
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
                    GetClosestFood();
                if (mateFound)
                    FindNearestMate();

                Debug.Log("5");
            }
        }
    }

    IEnumerator FollowPath()
    {
        moving = true;

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
            try
            {
                while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
                    if (pathIndex == path.finishLineIndex)
                    {
                        followingPath = false;
                        break;
                    }
                    else
                        pathIndex++;
            }
            catch
            {
                followingPath = false;
                StopCoroutine("FollowPath");
            }

            if (followingPath)
            {
                if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
                {
                    try
                    {
                        speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                        if (speedPercent < 0.05f)
                        {
                            followingPath = false;
                        }
                    }
                    catch
                    {
                        followingPath = false;
                        StopCoroutine("FollowPath");
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
                    animator.SetBool("Eat", true);
                    StartCoroutine(WaitBeforeEating());
                }
                else if (mateFound)
                {
                    if (runOnce)
                    {
                        Debug.Log("#Close to mate");
                        StartCoroutine(WaitBeforeMating());
                        StopCoroutine("FollowPath");
                    }
                }
        }
        yield return null;
        }
    }

    #endregion

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    }
}
