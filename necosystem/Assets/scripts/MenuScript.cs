using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;
using static UnityEngine.GraphicsBuffer;
using Cinemachine;
using UnityEngine.Playables;
using System.Security.Cryptography.X509Certificates;
using TMPro;

public class MenuScript : MonoBehaviour
{
    public bool meshActive = true;

    [Header("Menu assets")]
    public Slider sizeSlider;
    public GameObject seedInput;
    public GameObject menuGroup;
    public GameObject generatorGroup;

    [Header("Island data")]
    public GameObject islandMesh;
    public noiseData noiseData;
    public terrainData terrainData;
    public mapGenerator mapGenerator;

    [Header("Screen Transition")]
    public CinemachineVirtualCamera menuCam;
    public CinemachineVirtualCamera genCam;
    [SerializeField] private bool menuFadeIn = false;
    [SerializeField] private bool genFadeIn = false;

    private string seed;
    private int x = -1;
    private int z = 1;


    private void Start()
    {
        islandMesh.SetActive(true);

        System.Random randSeed = new System.Random();
        z = randSeed.Next(1, 1000000000);
        noiseData.seed = z;
        mapGenerator.DrawMapInEditor();
        Debug.Log(noiseData.seed);

        menuCam.Priority = 1;
        genCam.Priority = 0;
    }
    private void Awake()
    {
        islandMesh.SetActive(true);

        terrainData.meshHeightMultiplier = 9;
        terrainData.uniformScale = 3;

        menuGroup.GetComponent<CanvasGroup>().alpha = 1;
        generatorGroup.GetComponent<CanvasGroup>().alpha = 0;
    }

    private void Update()
    {
        CanvasGroup generatorOptions = generatorGroup.GetComponent<CanvasGroup>();
        CanvasGroup menuOptions = menuGroup.GetComponent<CanvasGroup>();

        if (meshActive)
        {
            islandMesh.SetActive(true);
        }

        //generator transition
        if (genFadeIn)
        {
            menuOptions.interactable = false;

            if (menuOptions.alpha >= 0) //menu opacity to 0
            {
                menuOptions.alpha -= Time.deltaTime;
            }
            if (generatorOptions.alpha < 1) //generator opacity to 1
            {
                generatorOptions.alpha += Time.deltaTime;
            }
            if (menuOptions.alpha == 0 || generatorOptions.alpha >= 1)
            {
                menuGroup.SetActive(false);
                genFadeIn = false;
            }
        }

        //main menu transition
        if (menuFadeIn)
        {
            generatorOptions.interactable = false;

            if (menuOptions.alpha < 1) //menu opacity to 1
            {
                menuOptions.alpha += Time.deltaTime;
            }
            if (generatorOptions.alpha >= 0) //generator opacity to 0
            {
                generatorOptions.alpha -= Time.deltaTime;
            }
            if (menuOptions.alpha >= 1 || generatorOptions.alpha == 0)
            {
                generatorGroup.SetActive(false);
                menuFadeIn = false;
            }
        }
    }

    public void Play()
    {
        generatorGroup.SetActive(true);

        menuCam.Priority = 0; //cut to genCam (priority is higher)
        genCam.Priority = 1;

        genFadeIn = true;
    }

    public void Back()
    {
        menuGroup.SetActive(true);

        menuCam.Priority = 1; //cut to menuCam
        genCam.Priority = 0;

        menuFadeIn = true;
    }
    public void Load()
    {
        Debug.Log("Load");
    }

    public void Settings()
    {
        Debug.Log("Settings");
    }

    public void Exit()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    public void RandomiseSeed() //seed randomiser
    {
        int s = UnityEngine.Random.Range(1, 1000000000);
        string f = s.ToString();
        ReadInputSeed(f);

        TMP_InputField seedInputField = seedInput.GetComponent<TMP_InputField>();
        seedInputField.text = f;
    }


    #region generator inputs
    public void ReadInputSeed(string s) //when seed is changed
    {
        if (Int32.TryParse(s, out x))
        {
            noiseData.seed = x;
            Debug.Log("seed: " + noiseData.seed);

            mapGenerator.DrawMapInEditor();
            seed = s;
        }
        else
        {
            Debug.Log("bad");
        }
    }

    public void ReadInputSize() //when size is changed
    {
        //default meshHeight = 20, default uniformScale = 3
        float s = sizeSlider.value;
        s = s / 10;

        float meshHeight = terrainData.meshHeightMultiplier * (terrainData.uniformScale / s);

        terrainData.uniformScale = s;
        terrainData.meshHeightMultiplier = meshHeight;
        Debug.Log("size: " + terrainData.uniformScale);

        mapGenerator.DrawMapInEditor();
    }

    public void OnClick() //when generator button is clicked
    {
        PlayerPrefs.SetString("seed", seed);
        SceneManager.LoadScene(2);
    }
    #endregion
}