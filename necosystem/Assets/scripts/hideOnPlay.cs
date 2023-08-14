using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hideOnPlay : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }
}
