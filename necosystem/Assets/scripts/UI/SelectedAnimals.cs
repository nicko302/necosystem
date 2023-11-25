using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedAnimals : MonoBehaviour
{
    public List<GameObject> selectedAnimalsList = new List<GameObject>();
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
    public TextMeshProUGUI text;
    [Header("Other")]
    public GameObject display;

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
                text.text = "Rabbit";
            }
            else if (selectedAnimalsList[0].GetComponent<Fox>() != null)
            {
                portraitImage.sprite = foxPortrait;
                text.text = "Fox";
            }
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
}
