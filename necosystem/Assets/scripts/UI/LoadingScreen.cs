using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingScreen;
    public GameObject holder1;
    public GameObject holder2;
    public float timer;
    public bool frame2 = false;

    private void Update()
    {
        if (loadingScreen.activeInHierarchy)
        {
            if (timer <= 0)
            {
                frame2 = !frame2;

                timer = .5f; // reset the interval timer
            }
            else
            {
                timer -= Time.deltaTime; // timer counts 
            }
        }

        if (frame2 == true)
        {
            holder2.SetActive(true);
        }
        else
        {
            holder2.SetActive(false);
        }
    }

    private void Start()
    {
        loadingScreen.SetActive(true);
        StartCoroutine(LoadingWait());
    }

    IEnumerator LoadingWait()
    {
        yield return new WaitForSeconds(5);
        loadingScreen.SetActive(false);
    }
}
