using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rabbit : AnimalAttributes
{
    [ContextMenu("R - Locate nearest food")]
    public void GetClosestFood()
    {
        nearestGrass = null;

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
    public override void EatFood()
    {
        if (nearestDistance < 2)
        {
            beHungry = false;
            //List<GameObject> allGrassList = allGrass.ToList();

            Destroy(nearestGrass.transform.parent.gameObject);
            //nearestGrass = null;

            this.gameObject.GetComponent<AnimalAttributes>().health = 100;
            /*if (this.gameObject.GetComponent<Rabbit>().health > 100) //keep within 0-100
            {
                this.gameObject.GetComponent<Rabbit>().health -= (this.gameObject.GetComponent<AnimalAttributes>().health - 100);
            }*/
            this.gameObject.GetComponent<Rabbit>().GetClosestFood();

        }
    }
}
