using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rabbit : AnimalAttributes
{
    [ContextMenu("R - Locate nearest food")]
    public void GetClosestFood() //locates the nearest grass item (rabbit)
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
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    [ContextMenu("R - Eat food")]
    public override void EatFood() //destroys/eats grass object
    {
        this.gameObject.GetComponent<Rabbit>().GetClosestFood();

        if (nearestDistance < 2)
        {
            isFindingFood = false;
            hungry = false;

            Debug.Log("destroying grass");
            Destroy(nearestGrass.transform.parent.gameObject);
            Debug.Log("grass destroyed");

            this.gameObject.GetComponent<AnimalAttributes>().health = 100;


            /*if (this.gameObject.GetComponent<Rabbit>().health > 100) //keep within 0-100
            {
                this.gameObject.GetComponent<Rabbit>().health -= (this.gameObject.GetComponent<AnimalAttributes>().health - 100);
            }*/
            //this.gameObject.GetComponent<Rabbit>().GetClosestFood();

        }
    }
}
