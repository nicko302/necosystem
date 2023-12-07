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

        // creates a list of all rabbits
        allFoodItems = GameObject.FindGameObjectsWithTag("Rabbit").ToList(); // adds all rabbits to the list
        
        for (int i = 0; i < allFoodItems.Count; i++)
        {
            if (allFoodItems[i].gameObject == null || allFoodItems[i].gameObject.GetComponent<Animal>().dead)
            {
                allFoodItems.RemoveAt(i); // removes rabbit from the list if they are already being hunted
                i--;
            }
        }
        

        if (allFoodItems.Count == 0)
        {
            return;
        }

        // iterates through the list to locate the closest food item
        distance = 0;
        nearestDistance = 10000;

        for (int i = 0; i < allFoodItems.Count; i++)
        {
            distance = Vector3.Distance(this.transform.position, allFoodItems[i].transform.position);

            if (distance < nearestDistance)
            {
                nearestFoodItem = allFoodItems[i];
                nearestDistance = distance;
                nearestFoodItem.gameObject.GetComponent<Rabbit>().StopCoroutine("FollowPath");
                nearestFoodItem.gameObject.GetComponent<Rabbit>().beingHunted = true;
            }
        }
    }

    [ContextMenu("R - Eat food")]
    public override void EatFood() //destroys/eats grass object
    {
        Debug.Log("EatFood");
        if (nearestFoodItem == null)
        {
            StopCoroutine("FollowPath");
            StopCoroutine("UpdatePath");
            StartFoodPathfinding();
            return;
        }
        GameObject rabbit = nearestFoodItem.gameObject;

        rabbit.GetComponent<Rabbit>().beingHunted = false;
        rabbit.GetComponent<Rabbit>().canWander = false;
        rabbit.GetComponent<Rabbit>().StopAllCoroutines();


        isFindingFood = false;
        isHungry = false;

        rabbit.GetComponent<Rabbit>().StartCoroutine("Die");

        this.gameObject.GetComponent<Animal>().health = 100;

        animator.SetBool("Eat", false);
        animator.SetBool("Walking", false);

        canWander = true;
        moving = false;
    }
    
    public override void StartFoodPathfinding()
    {
        {
            GetClosestFood();
            try
            {
                target = nearestFoodItem.transform.position;
            }
            catch
            {
                GetClosestFood();
                target = nearestFoodItem.transform.position;
            }

            nearestFoodItem.GetComponent<Animal>().beingHunted = true;

            animator.SetBool("Walking", true);
            animator.SetBool("Eat", false);

            Debug.Log("JAJAJAJAJA");
            canWander = false; moving = true;
            StartCoroutine(UpdatePath());
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
        if (chance <= 0.35f)
        {
            SpawnFox();
        }
        animator.SetBool("Walking", false);
        runOnce = true;
        canWander = true;

        allPotentialMates = null;
    }

    void SpawnFox()
    {
        Vector3 foxPos = gameObject.transform.position;
        Vector3 newPos = foxPos + (Vector3.one * posOffset);

        GameObject Foxes = GameObject.Find("Foxes");
        GameObject babyFox = Instantiate(babyPrefab, Foxes.transform); // instantiate new babyFox with the animal spawner object as a parent in hierarchy
        babyFox.transform.position = newPos;
        babyFox.name = "Fox";

        animator.SetBool("Walking", false);

        if (babyFox != null)
            StartCoroutine(DelayForBabyValues(babyFox));
    }

    public IEnumerator DelayForBabyValues(GameObject babyFox)
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
            babyFox.GetComponent<Fox>().outline = babyFox.gameObject.GetComponent<Outline>();
            babyFox.GetComponent<Fox>().outline.enabled = false;
        }
        catch
        {
            //do nothing;
        }
    }

    #endregion

    #region Pathfinding methods

    [ContextMenu("R - Pathfind")]
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
                {
                    if (nearestMate == null)
                        yield break;
                    else
                        target = nearestMate.transform.position;
                }
                else if (isFindingFood)
                {
                    if (nearestFoodItem == null)
                        yield break;
                    else
                        target = nearestFoodItem.transform.position;
                }

                PathRequestManager.RequestPath(transform.position, target, OnPathFound);
            }
        }
    }
    #endregion
}
