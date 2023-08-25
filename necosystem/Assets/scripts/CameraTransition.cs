using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CameraTransition : MonoBehaviour
{
    PlayableDirector director;

    [Header("Screen Transition")]
    public CinemachineVirtualCamera menuCam;
    public CinemachineVirtualCamera genCam;

    private void Start()
    {
        director = GetComponent<PlayableDirector>();
        menuCam.Priority = 1;
        genCam.Priority = 0;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            menuCam.Priority = 0; //cut to genCam (priority is higher)
            genCam.Priority = 1;
        }
        if (Input.GetKeyDown(KeyCode.Backspace)) //main menu
        {
            menuCam.Priority = 1; //cut to menuCam
            genCam.Priority = 0;
        }
    }
}
