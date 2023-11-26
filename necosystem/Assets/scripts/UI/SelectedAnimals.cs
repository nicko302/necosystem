using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelectedAnimals : MonoBehaviour
{
    public List<GameObject> selectedAnimalsList = new List<GameObject>();
    public List<GameObject> favouritedAnimalsList = new List<GameObject>();
    [Header("Health")]
    public ValueBar healthBar;
    public TextMeshProUGUI healthVal;
    [Header("Libido")]
    public ValueBar libidoBar;
    public TextMeshProUGUI libidoVal;
    [Header("Age")]
    public ValueBar ageBar;
    public TextMeshProUGUI ageVal;
    [Header("Details")]
    public Sprite rabbitPortrait;
    public Sprite foxPortrait;
    public Image portraitImage;
    public GameObject nameInput;
    private TMP_InputField nameInputField;
    //public TextMeshProUGUI text;
    [Header("Other")]
    public GameObject display;
    public bool isEditing;

    public void Move(int oldIndex, int newIndex)
    {
        try
        {
            GameObject item = selectedAnimalsList[oldIndex];
            selectedAnimalsList.RemoveAt(oldIndex);
            selectedAnimalsList.Insert(newIndex, item);
        }
        catch
        {
            // do nothing
        }
    }

    private void Update()
    {
        if (selectedAnimalsList.Count > 0)
        {
            display.SetActive(true);
            if (selectedAnimalsList[0].GetComponent<Rabbit>() != null)
            {
                portraitImage.sprite = rabbitPortrait;
            }
            else if (selectedAnimalsList[0].GetComponent<Fox>() != null)
            {
                portraitImage.sprite = foxPortrait;
            }
            if (!isEditing)
                nameInputField.text = selectedAnimalsList[0].GetComponent<Animal>().animalName;
            healthBar.SetValue(selectedAnimalsList[0].GetComponent<Animal>().health); healthVal.text = selectedAnimalsList[0].GetComponent<Animal>().health.ToString();
            libidoBar.SetValue(selectedAnimalsList[0].GetComponent<Animal>().libido); libidoVal.text = selectedAnimalsList[0].GetComponent<Animal>().libido.ToString();
            ageBar.SetMaxValue(selectedAnimalsList[0].GetComponent<Animal>().lifespan);
            ageBar.SetValue(selectedAnimalsList[0].GetComponent<Animal>().age); ageVal.text = selectedAnimalsList[0].GetComponent<Animal>().age.ToString();
        }
        else
        {
            display.SetActive(false);
        }
    }

    private void Start()
    {
        nameInputField = nameInput.GetComponent<TMP_InputField>();
    }

    public void ReadStringInput(string s)
    {
        selectedAnimalsList[0].GetComponent<Animal>().animalName = s;
        selectedAnimalsList[0].name = s;
        isEditing = false;
    }

    public void StringEdit()
    {
        isEditing = true;
    }
}
