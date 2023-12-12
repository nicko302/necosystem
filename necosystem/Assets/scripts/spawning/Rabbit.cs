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

        allFoodItems = GameObject.FindGameObjectsWithTag("Grass").ToList();

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
            }
        }
    }

    [ContextMenu("R - Eat food")]
    public override void EatFood() //destroys/eats grass object
    {
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();

        isFindingFood = false;
        isHungry = false;

        if (nearestFoodItem != null)
        {
            Destroy(nearestFoodItem.transform.parent.gameObject);
        }

        this.gameObject.GetComponent<Animal>().health = 100;

        animator.SetBool("Eat", false);
        animator.SetBool("Walking", false);
        moving = false; canWander = true;
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
                nearestMate = allPotentialMates[i];
                nearestDistance = distance;
            }
        }
    }

    public override void Mate()
    {
        StopCoroutine("FollowPath");

        libido = 100;
        mateFound = false;

        float chance = Random.Range(0f, 1f);
        if (chance <= 0.4f)
        {
            SpawnRabbit();
        }
        animator.SetBool("Walking", false);
        runOnce = true;
        canWander = true;

        allPotentialMates = null;
    }

    void SpawnRabbit()
    {
        Vector3 rabbitPos = gameObject.transform.position;
        Vector3 newPos = rabbitPos + (Vector3.one * posOffset);

        GameObject Rabbits = GameObject.Find("Rabbits");
        GameObject babyRabbit = Instantiate(babyPrefab, Rabbits.transform); // instantiate new babyRabbit with the animal spawner object as a parent in hierarchy
        babyRabbit.transform.position = newPos;
        babyRabbit.name = "Rabbit";

        animator.SetBool("Walking", false);

        if (babyRabbit != null)
            StartCoroutine(DelayForBabyValues(babyRabbit));
    }

    public IEnumerator DelayForBabyValues(GameObject babyRabbit)
    {
        yield return new WaitForSeconds(0.2f);

        try
        {
            var rotation = babyRabbit.transform.rotation.eulerAngles;
            rotation.x = 0;
            transform.rotation = Quaternion.Euler(rotation); // make sure baby is standing upright

            babyRabbit.transform.localScale = Vector3.one * 0.126f; // make baby small
            babyRabbit.GetComponent<Rabbit>().isBaby = true; // allows baby to start growing in Animal Update() method
            babyRabbit.GetComponent<Rabbit>().health = 100;
            babyRabbit.GetComponent<Rabbit>().libido = 100;
            babyRabbit.GetComponent<Rabbit>().age = 0;
            babyRabbit.GetComponent<Rabbit>().ageCounter = 0;
            babyRabbit.GetComponent<Rabbit>().isFindingFood = false;
            babyRabbit.GetComponent<Rabbit>().isHungry = false;
            babyRabbit.GetComponent<Fox>().outline = babyRabbit.gameObject.GetComponent<Outline>();
            babyRabbit.GetComponent<Fox>().outline.enabled = false;
        }
        catch
        {
           //do nothing;
        }
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
