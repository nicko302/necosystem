using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rabbit : AnimalAttributes
{
    [ContextMenu("R - Find food")]
    public override void LocateFood()
    {
        if (this.gameObject.GetComponent<Rabbit>().health <= 40)
        {
            Debug.Log("Finding food");

        }
    }

    [ContextMenu("R - Eat food")]
    public override void EatFood()
    {
        this.gameObject.GetComponent<AnimalAttributes>().health += 1;

        if (this.gameObject.GetComponent<Rabbit>().health > 100) //keep within 0-100
        {
            this.gameObject.GetComponent<Rabbit>().health -= (this.gameObject.GetComponent<AnimalAttributes>().health - 100);
        }
    }
}
