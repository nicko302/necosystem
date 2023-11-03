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

            this.gameObject.GetComponent<AnimalAttributes>().health = 100;


            /*if (this.gameObject.GetComponent<Rabbit>().health > 100) //keep within 0-100
            {
                this.gameObject.GetComponent<Rabbit>().health -= (this.gameObject.GetComponent<AnimalAttributes>().health - 100);
            }*/
            //this.gameObject.GetComponent<Rabbit>().GetClosestFood();

        }
    }
    /*************************
    [ContextMenu("R - Random movement")]
    public void RandomMovement()
    {
        
        StopCoroutine(DelayForRandomMovement());
        StopCoroutine(DelayForStopRandomMovement());
        StopCoroutine("FollowPath");
        

        StartCoroutine(DelayForRandomMovement());
        StartCoroutine(DelayForStopRandomMovement());
    }
    public override IEnumerator DelayForRandomMovement()
    {
        //yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 6f)); //wait 1-6 seconds

        GameObject randomPosObj = Instantiate(randomMovementPrefab);
        randomPosObj.transform.position = new Vector3(Random.Range(transform.position.x - 30, transform.position.x + 30), 50, Random.Range(transform.position.z - 30, transform.position.z + 30)); //instantiate empty prefab in random pos around the rabbit
        target = randomPosObj.transform; //target = the instantiated prefab

        Debug.Log("starting movement");

        LocateFood();

        Destroy(randomPosObj);
        yield return null;
    }
    IEnumerator DelayForStopRandomMovement()
    {
        yield return new WaitForSeconds(Random.Range(1f, 3f)); //wait 3-6 seconds
        Debug.Log("stopping movement");
        StopCoroutine(DelayForRandomMovement());
        StopCoroutine("FollowPath");
    }

    private void Update()
    {
        randNum = Random.Range(5, 10);
        /*if (randNum == 1)
        {
            //RandomMovement();
        }
    }

    private void Start()
    {
        //InvokeRepeating("RandomMovement", Random.Range(5, 10), randNum);
    }
    *////////////////////////////////
}
