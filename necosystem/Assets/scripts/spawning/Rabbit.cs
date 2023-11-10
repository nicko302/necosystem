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

    [ContextMenu("R - Pathfind food")]
    public override void LocateFood()
    {
        animator.SetBool("RabbitWalking", true);
        animator.SetBool("RabbitEat", false);
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
    }

    [ContextMenu("R - Eat food")]
    public override void EatFood() //destroys/eats grass object
    {
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();

        if (nearestDistance < 3)
        {
            isFindingFood = false;
            hungry = false;

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

    public override void Die()
    {
        Debug.Log("dead");

        // stop animal from pathfinding
        hungry = false; isFindingFood = true; moving = true; canWander = false;
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
        hungry = false; isFindingFood = true; moving = true; canWander = false;
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
