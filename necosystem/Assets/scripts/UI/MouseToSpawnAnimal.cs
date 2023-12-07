using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseToSpawnAnimal : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    private ButtonController buttonController;
    [Header("Spawn")]
    public GameObject rabbitPrefab;
    public GameObject foxPrefab;
    private Animal animal;

    private void Start()
    {
        buttonController = GameObject.Find("ButtonController").GetComponent<ButtonController>();
    }
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, interactableLayer))
        {
            transform.position = hit.point;
        }

        if (buttonController.animalKey == 0)
        {
            Destroy(gameObject);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (buttonController.animalKey == 1)
            {
                SpawnFox(hit.point);
            }
            if (buttonController.animalKey == 2)
            {
                SpawnRabbit(hit.point);
            }
        }
    }

    void SpawnFox(Vector3 spawnLoc)
    {
        GameObject Foxes = GameObject.Find("Foxes");
        GameObject babyFox = Instantiate(foxPrefab, Foxes.transform); // instantiate new babyRabbit with the animal spawner object as a parent in hierarchy
        babyFox.transform.position = spawnLoc;
        babyFox.name = "Fox";
        babyFox.transform.localScale = Vector3.one * 0.18f;

        babyFox.GetComponent<Animal>().animator.SetBool("Walking", false);

        if (babyFox != null)
            StartCoroutine(babyFox.GetComponent<Fox>().DelayForBabyValues(babyFox));
    }
    void SpawnRabbit(Vector3 spawnLoc)
    {
        GameObject Rabbits = GameObject.Find("Rabbits");
        GameObject babyRabbit = Instantiate(rabbitPrefab, Rabbits.transform); // instantiate new babyRabbit with the animal spawner object as a parent in hierarchy
        babyRabbit.transform.position = spawnLoc;
        babyRabbit.name = "Rabbit";
        babyRabbit.transform.localScale = Vector3.one * 0.18f;

        babyRabbit.GetComponent<Animal>().animator.SetBool("Walking", false);

        if (babyRabbit != null)
            StartCoroutine(babyRabbit.GetComponent<Rabbit>().DelayForBabyValues(babyRabbit));
    }
}
