using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    string savePath;
    [SerializeField] private Transform LoadButton;

    public void Awake()
    {
        savePath = Application.persistentDataPath + "/save.json";

        if (System.IO.File.Exists(savePath))
        {
            LoadButton.gameObject.SetActive(true);
        }
        else
        {
            LoadButton.gameObject.SetActive(false);
        }
    }    

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void LoadGame()
    {                
        PlayerPrefs.SetInt("Load", 1); 
        StartGame();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
