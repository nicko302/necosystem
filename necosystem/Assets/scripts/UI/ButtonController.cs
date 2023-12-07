using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public int animalKey;
    public GameObject foxButtonObj;
    public GameObject rabbitButtonObj;
    private ButtonScript foxButton;
    private ButtonScript rabbitButton;
    public GameObject foxSpawnPrefab;
    public GameObject rabbitSpawnPrefab;
    [SerializeField] AudioSource audioSource;
    private bool runOnce;

    private void Start()
    {
        foxButton = foxButtonObj.GetComponent<ButtonScript>();
        rabbitButton = rabbitButtonObj.GetComponent<ButtonScript>();
    }

    public void FoxButton()
    {
        audioSource.Play();
        if (foxButton.selected == false) // if not selected already
        {
            animalKey = 1;
            foxButton.selected = true;  // select it
            foxButtonObj.transform.localScale = foxButtonObj.transform.localScale * 1.06f;
            if (rabbitButton.selected)
            {
                Destroy(GameObject.Find("RabbitSpawn(Clone)"));
                rabbitButton.selected = false;
                rabbitButtonObj.transform.localScale = rabbitButtonObj.transform.localScale / 1.06f;
            }
            runOnce = true;
        }
        else // if already selected
        {
            animalKey = 0;
            Destroy(GameObject.Find("FoxSpawn(Clone)"));
            foxButton.selected = false; // unselect it
            foxButtonObj.transform.localScale = foxButtonObj.transform.localScale / 1.06f;
        }
    }

    public void RabbitButton()
    {
        audioSource.Play();
        if (rabbitButton.selected == false) // if not selected already
        {
            animalKey = 2;
            rabbitButton.selected = true; // select it
            rabbitButtonObj.transform.localScale = rabbitButtonObj.transform.localScale * 1.06f;
            if (foxButton.selected)
            {
                foxButton.selected = false;
                Destroy(GameObject.Find("FoxSpawn(Clone)"));
                foxButtonObj.transform.localScale = foxButtonObj.transform.localScale / 1.06f;
            }
            runOnce = true;
        }
        else // if already selected
        {
            animalKey = 0;
            rabbitButton.selected = false; // unselect it
            Destroy(GameObject.Find("RabbitSpawn(Clone)"));
            rabbitButtonObj.transform.localScale = rabbitButtonObj.transform.localScale / 1.06f;
        }
    }

    void Update()
    {
        // checks for button presses
        if (Input.GetKeyDown("1")) // fox number
        {
            FoxButton();
        }
        if (Input.GetKeyDown("2")) // rabbit number
        {
            RabbitButton();
        }

        // respond to button selection
        
        if (rabbitButton.selected == false)
        {
            Destroy(GameObject.Find("RabbitSpawn(Clone)"));
        }
        if (foxButton.selected == false)
        {
            Destroy(GameObject.Find("FoxSpawn(Clone)"));
        }

        if (rabbitButton.selected || foxButton.selected)
        {
            if (runOnce)
            {
                Destroy(GameObject.Find("FoxSpawn(Clone)"));
                Destroy(GameObject.Find("RabbitSpawn(Clone)"));
                GameObject foxSpawn = Instantiate(foxSpawnPrefab, transform);
                foxSpawn.transform.localScale = Vector3.one * 20;
                GameObject rabbitSpawn = Instantiate(rabbitSpawnPrefab, transform);
                rabbitSpawn.transform.localScale = Vector3.one * 20;
                if (rabbitButton.selected)
                {
                    Destroy(GameObject.Find("FoxSpawn(Clone)"));
                }
                if (foxButton.selected)
                {
                    Destroy(GameObject.Find("RabbitSpawn(Clone)"));
                }
                runOnce = false;
            }
        }
    }
}
