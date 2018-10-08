using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class UIMainMenu : UIScreen
{
    private EventSystem es;
    private bool canStartGame = true;
    //private PlayerMainMenu modelRef;

    public GameObject playBtn;
    public GameObject exitBtn;

    private void Start()
    {
        Time.timeScale = 1f;
        es = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        es.SetSelectedGameObject(null);
        es.SetSelectedGameObject(UIManager.instance.MainMenuFirstButton);
        //modelRef = FindObjectOfType<PlayerMainMenu>();
    }

    private void Update()
    {
        es.SetSelectedGameObject(UIManager.instance.MainMenuFirstButton);
    }

    public void OnStartGameButton()
    {
        if(canStartGame)
        {
           // AkSoundEngine.PostEvent("D_Wender_Yea", gameObject);
            //AkSoundEngine.PostEvent("UI_Ball_Throw", gameObject);
            //LevelToLoad.instance.levelToLoad = 5;
            //modelRef.modelAnimator.SetTrigger("isStarting");
            StartCoroutine(WaitForAnim());
            playBtn.SetActive(false);
            exitBtn.SetActive(false);
            canStartGame = false;
        }
    }

    public void OnExitGameButton()
    {
        Application.Quit();
    }

    private IEnumerator WaitForAnim()
    {       
        yield return new WaitForSeconds(2.5f);
        UIManager.instance.Hide();
        es.SetSelectedGameObject(null);
        canStartGame = true;
        playBtn.SetActive(true);
        exitBtn.SetActive(true);
        SceneManager.LoadSceneAsync("SCENE_Loading");
    }
}

