using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rabbit : Animal
{
    #region Food methods
    [ContextMenu("R - Locate nearest food")]
    public void GetClosestFood() //locates the nearest grass item
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
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();

        Debug.Log("destroying grass");
        //Destroy(nearestGrass.transform.parent.gameObject);
        nearestGrass.transform.parent.position = new Vector3(nearestGrass.transform.parent.position.x, 200, nearestGrass.transform.parent.position.z);
        Debug.Log("grass destroyed");

        this.gameObject.GetComponent<Animal>().health = 100;

        isFindingFood = false;
        isHungry = false;

        animator.SetBool("RabbitEat", false);
        animator.SetBool("RabbitWalking", false);
    }
    #endregion

    #region Mate methods

    [ContextMenu("R - Locate nearest mate")]
    public override void FindNearestMate()
    {
        nearestMate = null;
        allPotentialMates = null;

        allPotentialMates = GameObject.FindGameObjectsWithTag("Rabbit").ToList(); // adds all animals to list
        for (int i = 0; i < allPotentialMates.Count; i++)
        {
            if (!allPotentialMates[i].gameObject.GetComponent<Animal>().readyToMate || allPotentialMates[i].gameObject.GetComponent<Animal>().isLookingForMate)
            {
                allPotentialMates.RemoveAt(i); // removes the animal from the list of mates if they are not also ready to mate or are currently looking for one
                i--;
            }
        }

        if (allPotentialMates.Count == 0)
        {
            return;
        }

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

    [ContextMenu("R - Begin pathfinding")]
    public override void Pathfind()
    {
        animator.SetBool("RabbitWalking", true);
        animator.SetBool("RabbitEat", false);
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
    }

    public override void Die()
    {
        Debug.Log("dead");

        // stop animal from pathfinding
        isHungry = false; isFindingFood = true; moving = true;
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
        isHungry = false; isFindingFood = true; moving = true;
        StopCoroutine("DelayForWanderAI"); StopCoroutine("FollowPath");

        // stop current animations
        animator.SetBool("RabbitWalking", false);
        animator.SetBool("RabbitEat", false);

        // die
        animator.SetBool("RabbitDie", true);
        yield return new WaitForSeconds(5.9f);
        Destroy(this.gameObject);
    }

    #endregion
}
