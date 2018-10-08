using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIPausePopup : UIScreen
{
    private float lastTime;

    public GameObject firstSelected;
    private EventSystem es;
    public InventoryManager im;

    public Text[] text;
    public GameObject[] collectibles;

    private void Start()
    {
        

    }


    private void Update()
    {
        OnContineGame();
        
    }

    public void OnContinueGameButton()
    {
        //AkSoundEngine.PostEvent("UI_Click_Default", gameObject);
        UIManager.instance.Show<UIGameScreen>();
        Time.timeScale = 1f;
    }

     void OnEnable()
    {

        es = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        es.SetSelectedGameObject(null);
        es.SetSelectedGameObject(firstSelected);

        if (im == null)
            im = InventoryManager.instance;

        updateData();

    }

    public void OnContineGame()
    {
        if(Input.GetButtonDown("Pause"))
        {
            UIManager.instance.Show<UIGameScreen>();
            Time.timeScale = 1;
        }  
    }

    public void updateData()
    {
        for(int i = 0; i < collectibles.Length; i++)
        {
            collectibles[i].SetActive(im.collectibles[i]);
        }
    }

    public void OnExitGameButton()
    {
       // AkSoundEngine.PostEvent("UI_Click_Default", gameObject);
        UIManager.instance.Show<UIMainMenu>();
        Time.timeScale = 1f;
        es.SetSelectedGameObject(UIManager.instance.MainMenuFirstButton);
/*        if (!UIManager.instance.healthUI.GetCurrentAnimatorStateInfo(2).IsName("HeartsRegular"))
            UIManager.instance.healthUI.SetTrigger("Revive");
            */
        SceneManager.LoadSceneAsync("SCENE_Main Menu");
    }

    public void OnChangeTab()
    {

    }
}
