using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

  public static PauseMenu Instance;
	public bool GameIsPaused = false;
	public GameObject pauseMenuUI;

  private void Awake()
  {
    if(!Instance)
    {
      Instance = this;
    }
  }

  // Update is called once per frame
  void Update()
  {
      if (Input.GetKeyDown(KeyCode.Escape))
      {
        if (GameIsPaused)
        {
          Resume();
        }
        else
        {
          Pause();
        }
      }
  }

  public void Resume()
  {
    pauseMenuUI.SetActive(false);
    Time.timeScale = 1f;
    Instance.GameIsPaused = false;
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
  }

  void Pause()
  {
    pauseMenuUI.SetActive(true);
    Time.timeScale = 0f;
    Instance.GameIsPaused = true;
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }

  public void LoadMenu()
  {
    Resume();
    SceneManager.LoadScene("MainMenu");
  }

  public void QuitGame()
  {
    Application.Quit();
  }
}
