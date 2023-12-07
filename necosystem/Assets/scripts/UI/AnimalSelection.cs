using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class AnimalSelection : MonoBehaviour
{
    public bool selected;
    public bool favourited;
    public bool inList;
    public bool inFavourites;
    private GameObject selectedAnimalsObj;
    private SelectedAnimals selectedAnimals;

    private void OnMouseUpAsButton()
    {
        if (gameObject.GetComponent<Rabbit>() != null && selected == false)
        {
            Debug.Log("rabbit clicked");
            selected = true;
        }
        else if (gameObject.GetComponent<Rabbit>() != null && selected == true)
        {
            Debug.Log("rabbit unclicked");
            selected = false;
        }

        if (gameObject.GetComponent<Fox>() != null && selected == false)
        {
            Debug.Log("fox clicked");
            selected = true;
        }
        else if (gameObject.GetComponent<Fox>() != null && selected == true)
        {
            Debug.Log("fox unclicked");
            selected = false;
        }
    }

    private void Update()
    {
        if (selected && !favourited)
        {
            gameObject.GetComponent<Animal>().outline.enabled = true;
            gameObject.GetComponent<Animal>().outline.OutlineMode = Outline.Mode.OutlineAll;
            gameObject.GetComponent<Animal>().outline.OutlineColor = Color.blue;
            gameObject.GetComponent<Animal>().outline.OutlineWidth = 4f;
            if (!inList)
            {
                selectedAnimals.selectedAnimalsList.Add(gameObject);
                inList = true;
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                gameObject.GetComponent<Animal>().outline.enabled = true;
                gameObject.GetComponent<Animal>().outline.OutlineMode = Outline.Mode.OutlineAll;
                gameObject.GetComponent<Animal>().outline.OutlineColor = Color.yellow;
                gameObject.GetComponent<Animal>().outline.OutlineWidth = 6f;
                favourited = true;
                if (!inFavourites)
                {
                    selectedAnimals.favouritedAnimalsList.Add(gameObject);
                    inFavourites = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
            {
                selectedAnimals.selectedAnimalsList.Remove(gameObject); inList = false;
                if (inFavourites)
                {
                    selectedAnimals.favouritedAnimalsList.Remove(gameObject);
                    inFavourites = false;
                }
                selected = false;
                Destroy(gameObject);
            }
        }
        else if (selected && favourited)
        {
            gameObject.GetComponent<Animal>().outline.enabled = true;
            gameObject.GetComponent<Animal>().outline.OutlineMode = Outline.Mode.OutlineAll;
            gameObject.GetComponent<Animal>().outline.OutlineColor = new Color(255, 192, 0, 1);
            gameObject.GetComponent<Animal>().outline.OutlineWidth = 6f;
            if (!inList)
            {
                selectedAnimals.selectedAnimalsList.Add(gameObject);
                inList = true;
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                gameObject.GetComponent<Animal>().outline.OutlineMode = Outline.Mode.OutlineAll;
                gameObject.GetComponent<Animal>().outline.OutlineColor = Color.blue;
                gameObject.GetComponent<Animal>().outline.OutlineWidth = 4f;
                gameObject.GetComponent<Animal>().outline.enabled = false;
                favourited = false; selected = true;
                if (inFavourites)
                {
                    selectedAnimals.favouritedAnimalsList.Remove(gameObject);
                    inFavourites = false;
                }
            }
        }
        else if (!selected && !favourited)
        {
            if (gameObject.GetComponent<Outline>().enabled == true)
            {
                gameObject.GetComponent<Animal>().outline.enabled = false;
            }
            selectedAnimals.selectedAnimalsList.Remove(gameObject);
            inList = false;
        }
        else if (!selected && favourited)
        {
            gameObject.GetComponent<Animal>().outline.enabled = true;
            gameObject.GetComponent<Animal>().outline.OutlineMode = Outline.Mode.OutlineAll;
            gameObject.GetComponent<Animal>().outline.OutlineColor = Color.yellow;
            gameObject.GetComponent<Animal>().outline.OutlineWidth = 6f;
            selectedAnimals.selectedAnimalsList.Remove(gameObject);
            inList = false;
        }

        if (selectedAnimals.selectedAnimalsList.Count > 1)
        {
            gameObject.GetComponent<Animal>().outline.enabled = false;
            selectedAnimals.selectedAnimalsList[0].GetComponent<AnimalSelection>().selected = false;
            selectedAnimals.selectedAnimalsList.RemoveAt(0);
            selectedAnimals.Move(1, 0);
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !favourited)
        {
            gameObject.GetComponent<Animal>().outline.enabled = false;
            selectedAnimals.selectedAnimalsList.Remove(gameObject);
            selected = false;
        }
    }

    private void Awake()
    {
        selectedAnimalsObj = GameObject.Find("Selected Animals");
        selectedAnimals = selectedAnimalsObj.GetComponent<SelectedAnimals>();
    }
}