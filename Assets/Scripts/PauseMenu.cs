using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance {get; private set;}
    public static bool isPaused = false;

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject popup;
    [SerializeField] private TextMeshProUGUI popupText;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Click any to continue
        if (Input.anyKeyDown)
        {
            if (popup.activeInHierarchy)
            {
                ClosePopup();

                // If popup was the losing state prompt
                if (GameManager.Instance.gameState == GameState.Lose)
                {                    
                    GridManager.Instance.GenerateGrid(restart:true);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {        
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        SoundManager.Instance.PauseBGM();
        isPaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        SoundManager.Instance.UnPauseBGM();
        isPaused = false;
    }

    public void Save()
    {
        if (GameManager.Instance.SaveGame())
        {                        
            popupText.text = "SAVED!";
        }
        else
        {
            popupText.text = "ERROR!";
        }
        
        OpenPopup();
    }

    public void ChangePromptText(string str)
    {
        popupText.text = str;
    }

    public void OpenPopup()
    {        
        popup.SetActive(true);
    }

    public void ClosePopup()
    {        
        popup.SetActive(false);
    }

    public void LoadMenu()
    {
        // Destroy all on going animation
        DOTween.KillAll();
        
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("Menu");
    }
}
