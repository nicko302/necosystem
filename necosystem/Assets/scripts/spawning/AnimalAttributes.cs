using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAttributes : MonoBehaviour
{
    [Header("Needs")]
    [Tooltip("The hunger and health of the animal")]
    [Range(0,100)]
    public int health;
    [Tooltip("The desire for water")]
    [Range(0, 100)]
    public int thirst;
    [Tooltip("The desire for a mate")]
    [Range(0, 100)]
    public int libido;

    [Header("BirthAttributes")]
    [Tooltip("How fast the animal can travel")]
    [Range(0, 10)]
    public int speed;
    [Tooltip("How much damage the animal does to others")]
    [Range(0, 10)]
    public int strength;

    [ContextMenu("Set Default Values")]
    public void SetDefaults()
    {
        health = 100;
        thirst = 100;
        libido = 100;
        strength = UnityEngine.Random.Range(1,10);
        speed = UnityEngine.Random.Range(1,10);
    }
}
