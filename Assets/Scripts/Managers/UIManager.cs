using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
            }

            return _instance;
        }
    }

    private static UIManager _instance;
    private Dictionary<Type, UIScreen> screens = new Dictionary<Type, UIScreen>();
    private int amountOfInstances;
    private UIScreen currentScreen;
    public UIPausePopup pauseScreen;
    public UIGameScreen gameScreen;

    public Animator healthUI;

    //Handle first button per menu
    public GameObject MainMenuFirstButton;
    public GameObject PauseMenuFirstButton;
    public Canvas mCanvas;
   
    private void Awake()
    {
        if (UIManager.instance.gameObject != gameObject)
            Destroy(gameObject);

        Cursor.visible = false;
    }

    private void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
        ++amountOfInstances;

        if (amountOfInstances > 1)
        {
            DestroyObject(FindObjectsOfType<UIManager>()[0]);
        }
            

        foreach (UIScreen screen in GetComponentsInChildren<UIScreen>(true))
        {
            screen.gameObject.SetActive(false);
            screens.Add(screen.GetType(), screen);
        }
        Show(typeof(UIGameScreen));
    }

    public void Show<T>()
    {
        Show(typeof(T));
    }

    void Show(Type screenType)
    {
        if(currentScreen != null)
        {
            currentScreen.gameObject.SetActive(false);
        }

        UIScreen newScreen = screens[screenType];
        newScreen.gameObject.SetActive(true);
        currentScreen = newScreen;
    }

    public void Hide()
    {
        if (currentScreen != null)
        {
            currentScreen.gameObject.SetActive(false);
        }
    }
}
