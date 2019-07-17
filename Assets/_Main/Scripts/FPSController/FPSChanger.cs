using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSChanger : MonoBehaviour
{
    public static float ACTUAL_DELTA_TIME = 0;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    private void Update()
    {
        ACTUAL_DELTA_TIME = Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Application.targetFrameRate = 30;
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            Application.targetFrameRate = 60;
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            Application.targetFrameRate = 90;
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            Application.targetFrameRate = 120;
        }
    }

}
