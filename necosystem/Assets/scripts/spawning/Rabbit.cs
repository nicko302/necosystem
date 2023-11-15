using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Rabbit : Animal
{
    #region Food methods
    [ContextMenu("R - Locate nearest food")]
    public void GetClosestFood() // locates the nearest grass item
    {
        nearestGrass = null;
        allGrass = null;

        allGrass = GameObject.FindGameObjectsWithTag("Grass");

        distance = 0;
        nearestDistance = 10000;

        for (int i = 0; i < allGrass.Length; i++)
        {
            distance = Vector3.Distance(this.transform.position, allGrass[i].transform.position);

            if (distance < nearestDistance)
            {
                nearestGrass = allGrass[i];
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
            //Destroy(nearestGrass.transform.parent.gameObject);
            nearestGrass.transform.parent.position = new Vector3(nearestGrass.transform.parent.position.x, 200, nearestGrass.transform.parent.position.z);
            Debug.Log("grass destroyed");

            this.gameObject.GetComponent<Animal>().health = 100;

            animator.SetBool("RabbitEat", false);
            animator.SetBool("RabbitWalking", false);
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
        animator.SetBool("RabbitWalking", false);
        runOnce = true;
    }

    void SpawnRabbit()
    {
        Vector3 rabbitPos = gameObject.transform.position;
        Vector3 newPos = rabbitPos + (Vector3.one * posOffset);

        GameObject AnimalSpawner = GameObject.Find("Animal Spawner");
        GameObject babyRabbit = Instantiate(babyPrefab, AnimalSpawner.transform); // instantiate new babyRabbit with the animal spawner object as a parent in hierarchy
        babyRabbit.transform.position = newPos;

        babyRabbit.transform.localScale = this.gameObject.transform.localScale * 0.2f; // make baby small
        babyRabbit.GetComponent<Rabbit>().isBaby = true; // allows baby to start growing in Animal Update() method
    }

    #endregion

    #region Water methods
    /************************************ Rabbit drink water
    [ContextMenu("R - Locate nearest water")]
    public void GetClosestWater() //locates the nearest water
    {
        //////////// CALCULATE NEAREST NODE BELOW Y = 2.5

        nearestGrass = null;
        allGrass = null;

        allGrass = GameObject.FindGameObjectsWithTag("Grass");

        distance = 0;
        nearestDistance = 10000;

        for (int i = 0; i < allGrass.Length; i++)
        {
            distance = Vector3.Distance(this.transform.position, allGrass[i].transform.position);

            if (distance < nearestDistance)
            {
                nearestGrass = allGrass[i];
                nearestDistance = distance;
            }
        }
    }

    [ContextMenu("R - Pathfind food")]
    public override void LocateWater()
    {
        animator.SetBool("RabbitWalking", true);
        animator.SetBool("RabbitEat", false);
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

            animator.SetBool("RabbitEat", false);
            animator.SetBool("RabbitWalking", false);
        }
    }
    *////////////////////////////////////////
    #endregion

    #region Other methods

    [ContextMenu("R - Pathfind food")]
    public override void Pathfind()
    {
        animator.SetBool("RabbitWalking", true);
        animator.SetBool("RabbitEat", false);
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
    }

    public override IEnumerator UpdatePath() // updates the path to ensure it always points towards the target location
    {
        Debug.Log("1");

        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }

        Debug.Log("2");

        PathRequestManager.RequestPath(transform.position, target, OnPathFound);

        Debug.Log("3");

        //float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = Vector3.zero;
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            Debug.Log("4");

            if (target != targetPosOld)
            {
                Debug.Log("5");

                targetPosOld = target;
                target = nearestMate.transform.position;

                PathRequestManager.RequestPath(transform.position, target, OnPathFound);

                Debug.Log("6");

                dstFromMate = Vector3.Distance(target, targetPosOld);
                if (dstFromMate < 1)
                {
                    StopCoroutine("FollowPath");
                    StopCoroutine("UpdatePath");
                }
            }
        }
    }


    public override void Die()
    {
        Debug.Log("dead");

        // stop animal from pathfinding
        isHungry = false; isFindingFood = true; moving = true; canWander = false;
        StopCoroutine("DelayForWanderAI"); StopCoroutine("FollowPath");

        // stop current animations
        animator.SetBool("RabbitWalking", false);
        animator.SetBool("RabbitEat", false);

        // die
        animator.SetBool("RabbitDie", true);

        StartCoroutine("DestroyDelay");
    }
    public override IEnumerator DestroyDelay()
    {
        Debug.Log("dead");

        // stop animal from pathfinding
        isHungry = false; isFindingFood = true; moving = true; canWander = false;
        StopCoroutine("DelayForWanderAI"); StopCoroutine("FollowPath");

        // stop current animations
        animator.SetBool("RabbitWalking", false);
        animator.SetBool("RabbitEat", false);

        // die
        animator.SetBool("RabbitDie", true);
        yield return new WaitForSeconds(5.5f);
        Destroy(this.gameObject);
    }

    #endregion
}
