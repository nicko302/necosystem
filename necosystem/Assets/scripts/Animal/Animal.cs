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
    public string animalName;
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
    [SerializeField]
    protected float minInterval;
    [SerializeField]
    protected float maxInterval;
    [SerializeField]
    protected float timer;
    public Vector3 target;
    public float dstFromTarget;
    public float oldDstFromTarget;
    public Vector3 comparePos = Vector3.zero;
    public float dstFromComparePos;
    protected float oldDstFromComparePos;
    public float dstFromComparePos1;
    protected float oldDstFromComparePos1;
    public float movementCheckTimer;
    public float pathfindTimeElapsed = 50f;
    protected bool repeatForMate = true;
    protected bool repeatForFood = true;

    protected bool exceptionCaught;

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
    protected List<GameObject> allFoodItems;
    [SerializeField]
    protected GameObject nearestFoodItem;
    public bool beingHunted;
    [SerializeField]
    protected float foodPathFindingTimer;

    [Header("Mating variables")]
    public bool isBaby = false;
    public bool mateFound = false;
    public bool readyToMate = false;
    public bool mateConditionsMet = false;
    public GameObject nearestMate;
    public List<GameObject> allPotentialMates;
    protected const float posOffset = 3f;
    [SerializeField]
    protected GameObject babyPrefab;


    [Header("Wander variables")]
    [SerializeField]
    [Tooltip("X / 10,000 chance to wander")]
    protected int chance = 1;
    protected int randNum = 0;
    

    [SerializeField]
    protected float boundSize = 240f;
    [SerializeField]
    protected float height = 50f;
    public bool moving = false;

    public bool canWander = false;
    public bool afterSceneLoad = false;
    Path path;

    [Header("Animator")]
    public Animator animator;

    [Header("Other")]
    public bool runOnce = true;
    public ValueBar valueBar;
    public Outline outline;
    [SerializeField] SelectedAnimals selectedAnimals;

    Grid grid;
    #endregion

    #region Animal functions

    [ContextMenu("Set Default Values")] // method to assign default values for debugging
    public void SetDefaults()
    {
        health = UnityEngine.Random.Range(90, 100);
        libido = UnityEngine.Random.Range(90, 100);
        if (GetComponent<Rabbit>() != null) { lifespan = UnityEngine.Random.Range(7, 11); animalName = "Rabbit";  }
        else if (GetComponent<Fox>() != null) { lifespan = UnityEngine.Random.Range(4, 7); animalName = "Fox"; }

        age = UnityEngine.Random.Range(1, 2);
        ageCounter = UnityEngine.Random.Range(0, 3);

        strength = UnityEngine.Random.Range(1, 10);

        if (GetComponent<Rabbit>() != null) { intSpeed = UnityEngine.Random.Range(30, 60); }
        else if (GetComponent<Fox>() != null) { intSpeed = UnityEngine.Random.Range(50, 80); }
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

    protected void Die()
    {
        // deselect animal
        selectedAnimals.selectedAnimalsList.Remove(gameObject);
        selectedAnimals.favouritedAnimalsList.Remove(gameObject);

        // stop movement
        health = 100; libido = 100;
        canWander = false;
        StopAllCoroutines();

        // stop current animations
        animator.SetBool("Walking", false);
        animator.SetBool("Eat", false);

        // die
        animator.SetBool("Die", true);
        Invoke("DieDestroy", 4.4f);
    }

    public virtual void DieDestroy()
    {
        Destroy(this.gameObject);
    }

    protected void WaitBeforeEating()
    {
        if (gameObject.GetComponent<Fox>() != null) // stop rabbit moving before killing it
        {
            if (nearestFoodItem != null)
            {
                nearestFoodItem.GetComponent<Animal>().beingHunted = false;
                nearestFoodItem.GetComponent<Animal>().moving = false;
                nearestFoodItem.GetComponent<Animal>().canWander = false;
                nearestFoodItem.GetComponent<Animal>().animator.SetBool("Walking", false);
                nearestFoodItem.GetComponent<Animal>().StopCoroutine("FollowPath");
                nearestFoodItem.GetComponent<Animal>().StopCoroutine("UpdatePath");
            }
            else
            {
                GetClosestFood();
                StartFoodPathfinding();
                WaitBeforeEating();
            }
        }
        animator.SetBool("Walking", false);
        animator.SetBool("Eat", true);
        animator.SetBool("Walking", false);
        Invoke ("EatFood", 3);
    }

    protected void WaitBeforeMating()
    {
        runOnce = false;
        animator.SetBool("Walking", false);
        Invoke("Mate", 3);
    }

    #endregion

    #region Start/Update methods
    private void Start()
    {
        selectedAnimals = GameObject.Find("Selected Animals").GetComponent<SelectedAnimals>();
        StartCoroutine("DelayForWanderAI");

        minInterval = 1;
        maxInterval = 6;
    }

    private void Awake()
    {
        outline = gameObject.GetComponent<Outline>();
        outline.enabled = false;
    }

    private void Update()
    {
        CheckDeath();
        CheckHunger();
        CheckLibido();

        if (!(isHungry || mateFound)) // if the animal does not currently have a strong need
        {
            // checks every frame to see if the timer has reached zero
            if (canWander)
            {
                if (timer <= 0)
                {
                    if (this.gameObject.GetComponent<Fox>() != null)
                        health -= UnityEngine.Random.Range(3, 4);
                    else
                        health -= UnityEngine.Random.Range(4, 8);

                    if (!isBaby)
                        libido -= UnityEngine.Random.Range(5, 9);
                    else
                        libido = 100;

                    if (ageCounter < 5)
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

                    // when timer reaches zero, and until the baby has reached full size, its scale will increase
                    if (transform.localScale.x < 0.63)
                    {
                        transform.localScale += new Vector3(.04f, .04f, .04f);
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

        if (afterSceneLoad)
        {
            // food pathfinding
            if (isHungry && !isFindingFood) // if hungry and not currently finding food, find food
            {
                StartFoodPathfinding();
                isHungry = false;
                isFindingFood = true;
            }
           
            // mate pathfinding
            if (readyToMate && !mateFound && !isHungry && !isFindingFood && !moving) // if wants to mate but hasnt found one, animal is able to mate
            {
                mateConditionsMet = true;
            }
            else
            {
                mateConditionsMet = false;
            }
            if (mateConditionsMet) // if able to mate...
            {
                FindNearestMate(); // locate the nearest potential mate
                if (nearestMate != null) // only pathfind to a potential mate if the mate is also ready to mate
                {
                    StartMatePathfinding();
                    readyToMate = false;
                    mateFound = true;
                }
            }

            // pathfinding bug fixes / error handling
            if (mateFound || isFindingFood) // if currently pathfinding
            {
                if (nearestMate != null && nearestMate.gameObject.GetComponent<Animal>().libido > 30) // ensures the nearest mate cannot have libido above 30, then retries
                {
                    nearestMate.gameObject.GetComponent<Animal>().readyToMate = false;
                    nearestMate = null;
                    FindNearestMate();
                    StartMatePathfinding();
                }

                if (movementCheckTimer <= 0)
                {
                    oldDstFromComparePos = dstFromComparePos;
                    dstFromComparePos = Vector3.Distance(this.gameObject.transform.position, comparePos);

                    if (dstFromComparePos == oldDstFromComparePos) // if animal hasnt moved in 2 seconds, reattempt pathfinding
                    {
                        StopCoroutine(UpdatePath()); StopCoroutine(FollowPath()); StopAllCoroutines();
                        moving = false;

                        if (mateFound)
                        {
                            repeatForMate = !repeatForMate;

                            if (repeatForMate == true)
                            {
                                WaitBeforeMating();
                                mateFound = false; libido = 100;
                            }
                        }
                        else if (mateFound && repeatForMate == false)
                        {
                            FindNearestMate();
                            StartMatePathfinding();
                        }

                        if (isFindingFood)
                        {
                            repeatForFood = !repeatForFood;

                            if (repeatForFood == true)
                            {
                                WaitBeforeEating();
                                isFindingFood = false; health = 100;
                            }
                        }
                        else if (isFindingFood && repeatForFood == false)
                        {
                            GetClosestFood();
                            StartFoodPathfinding();
                        }
                    }

                    dstFromTarget = Vector3.Distance(this.gameObject.transform.position, target); // calculate distance to target

                    // if animal is close enough to target, eat/mate accordingly
                    if (dstFromTarget < 6)
                    {
                        StopCoroutine("UpdatePath"); StopCoroutine("FollowPath"); moving = false;

                        if (mateFound)
                            WaitBeforeMating();
                        else if (isFindingFood)
                            WaitBeforeEating();

                    }

                    movementCheckTimer = 2; // reset the interval timer
                }
                else
                {
                    movementCheckTimer -= Time.deltaTime; // timer counts 
                }

                pathfindTimeElapsed -= Time.deltaTime;

                if (pathfindTimeElapsed <= 0) // if animal is stuck on pathfinding for 30 seconds, die
                {
                    Die();  
                }
            }
            else
                pathfindTimeElapsed = 50f; // if not pathfinding, time elapsed resets

            if (moving)
            {
                oldDstFromComparePos1 = dstFromComparePos1;
                dstFromComparePos1 = Vector3.Distance(this.gameObject.transform.position, comparePos);

                if (dstFromComparePos1 == oldDstFromComparePos1) // if animal hasnt moved while moving is true
                {
                    moving = false; canWander = true;
                }
            }

            // baby determination
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

            // movement animation check
            if (moving)
                animator.SetBool("Walking", true);
            else
                animator.SetBool("Walking", false);

            // death check
            if (age == lifespan)
            {
                Die();
            }
        }
    }

    #endregion

    #region Check needs functions
    private void CheckHunger() //checks if the health value meets the isHungry threshold
    {
        if (this.gameObject.GetComponent<Animal>().health <= 50 && this.gameObject.GetComponent<Animal>().health > 15)
        {
            isHungry = true;
        }
        else
        {
            isHungry = false;
        }
    }

    private void CheckLibido() //checks if the libido value meets the ready to mate threshold
    {
        if (this.gameObject.GetComponent<Animal>().libido <= 30)
        {
            readyToMate = true;
        }
    }
    private void CheckDeath() //checks if the health value meets the isHungry threshold
    {
        if (this.gameObject.GetComponent<Animal>().health <= 10)
        {
            Die();
        }
    }

    #endregion

    #region Wander functions

    IEnumerator DelayForWanderAI()
    {
        yield return new WaitForSeconds(4);
        canWander = true;
        afterSceneLoad = true;
    }

    [ContextMenu("Random Movement")]
    void RandomMovement()
    {
        target = new Vector3(UnityEngine.Random.Range(-boundSize, boundSize), height, UnityEngine.Random.Range(-boundSize, boundSize));
        Pathfind();
    }

    void StopRandomMovement()
    {
        moving = false;

        canWander = true;
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

    public virtual void StartFoodPathfinding() //calls the required subroutines to pathfind towards food
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

        animator.SetBool("Walking", true);
        animator.SetBool("Eat", false);

        canWander = false; moving = true;

        Pathfind();
    }

    private void StartMatePathfinding() //calls the required subroutines to pathfind towards a mate
    {
        moving = true;
        canWander = false;

        animator.SetBool("Walking", true);
        animator.SetBool("Eat", false);

        try
        {
            target = new Vector3(nearestMate.transform.position.x, nearestMate.transform.position.y, nearestMate.transform.position.z);
        }
        catch
        {
            nearestMate = null;
        }
        moving = true;
        StartCoroutine("UpdatePath");
    }

    #endregion

    #region Pathfinding methods

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            try
            {
                path = new Path(waypoints, transform.position, turnDst, stoppingDst);
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
            catch
            {
                StopCoroutine(FollowPath());
                if (mateFound)
                {
                    FindNearestMate();
                    path = new Path(waypoints, transform.position, turnDst, stoppingDst);
                    StartCoroutine("FollowPath");
                }
                else if (isFindingFood)
                {
                    GetClosestFood();
                    path = new Path(waypoints, transform.position, turnDst, stoppingDst);
                    StartCoroutine("FollowPath");
                }
            }
        }
        else
        {
            StopCoroutine(FollowPath());
            moving = false;
            if (mateFound)
                mateFound = false;
            if (isFindingFood)
                isFindingFood = false;
        }
    }

    public virtual IEnumerator UpdatePath() // updates the path to ensure it always points towards the target location
    {
        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }

        PathRequestManager.RequestPath(transform.position, target, OnPathFound);

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target;
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((target - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(transform.position, target, OnPathFound);
                targetPosOld = target;
                if (isFindingFood)
                    GetClosestFood();
                if (mateFound)
                    FindNearestMate();
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
            followingPath = false;
            StopCoroutine(FollowPath());
            StopCoroutine(UpdatePath());
            canWander = true;
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
                StopCoroutine(FollowPath());
                StopCoroutine(UpdatePath());
                canWander = true;
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
                        StopCoroutine(FollowPath());
                        StopCoroutine(UpdatePath());
                        canWander = true;
                    }
                }

                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
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
