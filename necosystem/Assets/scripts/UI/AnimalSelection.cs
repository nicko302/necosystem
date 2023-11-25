using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class AnimalSelection : MonoBehaviour
{
    public bool selected;
    public bool inList;
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
        if (selected)
        {
            gameObject.GetComponent<Animal>().outline.enabled = true;
            gameObject.GetComponent<Animal>().outline.OutlineMode = Outline.Mode.OutlineAll;
            gameObject.GetComponent<Animal>().outline.OutlineColor = Color.cyan;
            gameObject.GetComponent<Animal>().outline.OutlineWidth = 6f;
            if (!inList)
            {
                selectedAnimals.selectedAnimalsList.Add(gameObject);
                inList = true;
            }
        }
        else
        {
            gameObject.GetComponent<Animal>().outline.enabled = false;
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
    }

    private void Awake()
    {
        selectedAnimalsObj = GameObject.Find("Selected Animals");
        selectedAnimals = selectedAnimalsObj.GetComponent<SelectedAnimals>();
    }

    /*void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (rabbit == GetClickedObject(out RaycastHit hit))
            {
                print("rabbit clicked!");
            }
            
        }
        if (Input.GetMouseButtonUp(0))
        {
            print("Mouse is off!");
        }
    }

    GameObject GetClickedObject(out RaycastHit hit)
    {
        GameObject target = null;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction * 10, out hit))
        {
            if (!isPointerOverUIObject()) { target = hit.collider.gameObject; }
        }
        return target;
    }
    private bool isPointerOverUIObject()
    {
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);
        return results.Count > 0;
    }*/
}