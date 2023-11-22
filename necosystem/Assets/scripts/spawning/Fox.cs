using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Fox : Animal
{
    #region Food methods
    [ContextMenu("R - Locate nearest food")]
    public override void GetClosestFood() // locates the nearest rabbit
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
        this.gameObject.GetComponent<Fox>().GetClosestFood();

        if (nearestDistance < 5)
        {
            Debug.Log("NearestDistance < 5");

            isFindingFood = false;
            isHungry = false;

            Debug.Log("destroying fox");
            Destroy(nearestFoodItem.transform.parent.gameObject);
            Debug.Log("fox destroyed");

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
        allPotentialMates = GameObject.FindGameObjectsWithTag("Fox").ToList(); // adds all animals to list
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
            else if (allPotentialMates[i].gameObject.GetComponent<Animal>().age < allPotentialMates[i].gameObject.GetComponent<Animal>().lifespan - 1)
            {
                allPotentialMates.RemoveAt(i); // removes animal from the list of mates if age is close to death
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
        if (chance <= 0.6f)
        {
            SpawnFox();
        }
        animator.SetBool("Walking", false);
        runOnce = true;

        allPotentialMates = null;
    }

    void SpawnFox()
    {
        Vector3 foxPos = gameObject.transform.position;
        Vector3 newPos = foxPos + (Vector3.one * posOffset);

        GameObject AnimalSpawner = GameObject.Find("Animal Spawner");
        GameObject babyFox = Instantiate(babyPrefab, AnimalSpawner.transform); // instantiate new babyFox with the animal spawner object as a parent in hierarchy
        babyFox.transform.position = newPos;

        animator.SetBool("Walking", false);

        if (babyFox != null)
            StartCoroutine(DelayForBabyValues(babyFox));
    }

    private IEnumerator DelayForBabyValues(GameObject babyFox)
    {
        yield return new WaitForSeconds(0.2f);

        try
        {
            var rotation = babyFox.transform.rotation.eulerAngles;
            rotation.x = 0;
            transform.rotation = Quaternion.Euler(rotation); // make sure baby is standing upright

            babyFox.transform.localScale = Vector3.one * 0.126f; // make baby small
            babyFox.GetComponent<Fox>().isBaby = true; // allows baby to start growing in Animal Update() method
            babyFox.GetComponent<Fox>().health = 100;
            babyFox.GetComponent<Fox>().libido = 100;
            babyFox.GetComponent<Fox>().age = 0;
            babyFox.GetComponent<Fox>().ageCounter = 0;
            babyFox.GetComponent<Fox>().isFindingFood = false;
            babyFox.GetComponent<Fox>().isHungry = false;
        }
        catch
        {
            //do nothing;
        }
    }

    #endregion

    #region Animal functions
    public override void Die()
    {
        Debug.Log("dead");

        // stop animal from pathfinding
        isHungry = false; isFindingFood = true; canWander = false;
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
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
        isHungry = false; isFindingFood = true; canWander = false;
        StopCoroutine("DelayForWanderAI"); StopCoroutine("FollowPath");

        // stop current animations
        animator.SetBool("Walking", false);
        animator.SetBool("Eat", false);

        // die
        animator.SetBool("Die", true);
        yield return new WaitForSeconds(6f);
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

                if (mateFound)
                    target = nearestMate.transform.position;
                else if (isFindingFood)
                    target = nearestFoodItem.transform.position;

                PathRequestManager.RequestPath(transform.position, target, OnPathFound);


                dstFromTarget = Vector3.Distance(target, targetPosOld);
                if (dstFromTarget < 1.5f)
                {
                    StopCoroutine("FollowPath");
                    StopCoroutine("UpdatePath");
                }
            }
        }
    }
    #endregion
}
