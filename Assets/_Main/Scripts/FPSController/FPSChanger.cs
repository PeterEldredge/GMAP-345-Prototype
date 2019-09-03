using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FPSChanger : MonoBehaviour
{
    public static float ACTUAL_DELTA_TIME = 0;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        /*ACTUAL_DELTA_TIME = Time.deltaTime;

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
        }*/

        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            //Application.Quit();
        }
    }

}
