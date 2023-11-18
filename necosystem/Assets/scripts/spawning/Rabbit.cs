using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Rabbit : Animal
{
    #region Food methods
    [ContextMenu("R - Locate nearest food")]
    public override void GetClosestFood() // locates the nearest grass item
    {
        nearestFoodItem = null;
        allFoodItems = null;

        allFoodItems = GameObject.FindGameObjectsWithTag("Grass");

        distance = 0;
        nearestDistance = 10000;

        for (int i = 0; i < allFoodItems.Length; i++)
        {
            distance = Vector3.Distance(this.transform.position, allFoodItems[i].transform.position);

            if (distance < nearestDistance)
            {
                nearestFoodItem = allFoodItems[i];
                nearestDistance = distance;
            }
        }
    }

    [ContextMenu("R - Eat food")]
    public override void EatFood() //destroys/eats grass object
    {
        Debug.Log("EatFood");
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();

        if (nearestDistance < 5)
        {
            Debug.Log("NearestDistance < 5");

            isFindingFood = false;
            isHungry = false;

            Debug.Log("destroying grass");
            Destroy(nearestFoodItem.transform.parent.gameObject);
            //nearestFoodItem.transform.parent.position = new Vector3(nearestFoodItem.transform.parent.position.x, 200, nearestFoodItem.transform.parent.position.z);
            Debug.Log("grass destroyed");

            this.gameObject.GetComponent<Animal>().health = 100;

            animator.SetBool("Eat", false);
            animator.SetBool("Walking", false);
        }
    }
    #endregion

    #region Mate methods

    [ContextMenu("R - Locate nearest mate")]
    public override void FindNearestMate()
    {
        nearestMate = null;
        allPotentialMates = null;

        // creates a list of all animals who meet the mate conditions
        allPotentialMates = GameObject.FindGameObjectsWithTag("Rabbit").ToList(); // adds all animals to list
        for (int i = 0; i < allPotentialMates.Count; i++)
        {
            if (!allPotentialMates[i].gameObject.GetComponent<Animal>().mateConditionsMet)
            {
                allPotentialMates.RemoveAt(i); // removes the animal from the list of mates if they do not meet the mate conditions
                i--;
            }
            else if (allPotentialMates[i].gameObject == this.gameObject)
            {
                allPotentialMates.RemoveAt(i); // removes itself from the list of mates
                i--;
            }
        }

        if (allPotentialMates.Count == 0)
        {
            Debug.Log("No mate found");
            return;
        }

        // iterates through the list to locate the closest animal
        distance = 0;
        nearestDistance = 10000;

        for (int i = 0; i < allPotentialMates.Count; i++)
        {
            distance = Vector3.Distance(this.transform.position, allPotentialMates[i].transform.position);

            if (distance < nearestDistance)
            {
                Debug.Log("Mate found");
                nearestMate = allPotentialMates[i];
                nearestDistance = distance;
            }
        }
    }

    public override void Mate()
    {
        StopCoroutine("FollowPath");

        Debug.Log("mating...");
        libido = 100;
        mateFound = false;

        float chance = Random.Range(0f, 1f);
        if (chance <= 0.5f)
        {
            SpawnRabbit();
        }
        animator.SetBool("Walking", false);
        runOnce = true;

        allPotentialMates = null;
    }

    void SpawnRabbit()
    {
        Vector3 rabbitPos = gameObject.transform.position;
        Vector3 newPos = rabbitPos + (Vector3.one * posOffset);

        GameObject AnimalSpawner = GameObject.Find("Animal Spawner");
        GameObject babyRabbit = Instantiate(babyPrefab, AnimalSpawner.transform); // instantiate new babyRabbit with the animal spawner object as a parent in hierarchy
        babyRabbit.transform.position = newPos;

        animator.SetBool("Walking", false);

        if (babyRabbit != null)
            StartCoroutine(DelayForBabyValues(babyRabbit));
    }

    private IEnumerator DelayForBabyValues(GameObject babyRabbit)
    {
        yield return new WaitForSeconds(0.1f);

        try
        {
            babyRabbit.transform.localScale = Vector3.one * 0.126f; // make baby small
            babyRabbit.GetComponent<Rabbit>().isBaby = true; // allows baby to start growing in Animal Update() method
            babyRabbit.GetComponent<Rabbit>().health = 100;
            babyRabbit.GetComponent<Rabbit>().libido = 100;
            babyRabbit.GetComponent<Rabbit>().age = 0;
            babyRabbit.GetComponent<Rabbit>().ageCounter = 0;
        }
        catch
        {
           //do nothing;
        }
    }

    #endregion

    #region Water methods
    /************************************ Rabbit drink water
    [ContextMenu("R - Locate nearest water")]
    public void GetClosestWater() //locates the nearest water
    {
        //////////// CALCULATE NEAREST NODE BELOW Y = 2.5

        nearestFoodItem = null;
        allFoodItems = null;

        allFoodItems = GameObject.FindGameObjectsWithTag("Grass");

        distance = 0;
        nearestDistance = 10000;

        for (int i = 0; i < allFoodItems.Length; i++)
        {
            distance = Vector3.Distance(this.transform.position, allFoodItems[i].transform.position);

            if (distance < nearestDistance)
            {
                nearestFoodItem = allFoodItems[i];
                nearestDistance = distance;
            }
        }
    }

    [ContextMenu("R - Pathfind food")]
    public override void LocateWater()
    {
        animator.SetBool("Walking", true);
        animator.SetBool("Eat", false);
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
    }

    
    [ContextMenu("R - Drink Water")]
    public override void DrinkWater() //destroys/eats grass object
    {
        this.gameObject.GetComponent<Rabbit>().GetClosestWater();

        if (nearestDistance < 3)
        {
            //isFindingWater = false;
            //thirsty = false;

            Debug.Log("drinking water");

            this.gameObject.GetComponent<AnimalAttributes>().thirst = 100;

            animator.SetBool("Eat", false);
            animator.SetBool("Walking", false);
        }
    }
    *////////////////////////////////////////
    #endregion

    #region Animal functions
    public override void Die()
    {
        Debug.Log("dead");

        // stop animal from pathfinding
        isHungry = false; isFindingFood = true; moving = true; canWander = false;
        StopCoroutine("DelayForWanderAI"); StopCoroutine("FollowPath");

        // stop current animations
        animator.SetBool("Walking", false);
        animator.SetBool("Eat", false);

        // die
        animator.SetBool("Die", true);

        StartCoroutine("DestroyDelay");
    }
    public override IEnumerator DestroyDelay()
    {
        Debug.Log("dead");

        // stop animal from pathfinding
        isHungry = false; isFindingFood = true; moving = true; canWander = false;
        StopCoroutine("DelayForWanderAI"); StopCoroutine("FollowPath");

        // stop current animations
        animator.SetBool("Walking", false);
        animator.SetBool("Eat", false);

        // die
        animator.SetBool("Die", true);
        yield return new WaitForSeconds(5.5f);
        Destroy(this.gameObject);
    }

    #endregion

    #region Pathfinding methods

    [ContextMenu("R - Pathfind food")]
    public override void Pathfind()
    {
        animator.SetBool("Walking", true);
        animator.SetBool("Eat", false);
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
    }

    public override IEnumerator UpdatePath() // updates the path to ensure it always points towards the target location
    {
        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }

        PathRequestManager.RequestPath(transform.position, target, OnPathFound);

        //float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = Vector3.zero;
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if (target != targetPosOld)
            {

                targetPosOld = target;
                target = nearestMate.transform.position;

                PathRequestManager.RequestPath(transform.position, target, OnPathFound);


                dstFromMate = Vector3.Distance(target, targetPosOld);
                if (dstFromMate < 1)
                {
                    StopCoroutine("FollowPath");
                    StopCoroutine("UpdatePath");
                }
            }
        }
    }
    #endregion
}
