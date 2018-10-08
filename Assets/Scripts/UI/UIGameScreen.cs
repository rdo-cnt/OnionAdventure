using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIGameScreen : UIScreen
{
    //private PlayerInventory playerInventory;
    //private PlayerHealth playerHealth;

    public GameObject HealthUI; //Esto manejara la UI de la vida

    public GameObject UI;
    public GameObject spawnScorePrefab;
    public GameObject spawnScorePos;
    public GameObject teleportTimerUI;
    public GameObject floppyPrefab;

    //internal UIBaseInfo currentInfo;
    //public UIBaseInfo hackingInfo;
    //public UIBaseInfo hidingInfo;
    public GameObject skipInfoUI;

    //public GameObject SwapInfo;

    private void Update()
    {
        if (GameManager.instance == null)
            return;

        //SwapInfo.SetActive(true);

        OnPauseGame();
    }

    public void OnPauseGame()
    {
        if (Input.GetButtonDown("Pause") && Time.timeScale == 0.0f)
        {
            UIManager.instance.Show<UIGameScreen>();
            Time.timeScale = 1f;
        }

        if (Input.GetButtonDown("Pause") && Time.timeScale > 0.5f)
        {
            UIManager.instance.Show<UIPausePopup>();
            Time.timeScale = 0.0f;
        }
    }

    /*
    public void OnInfoChange(UIBaseInfo infoToShow)
    {
        if(currentInfo != null)
        {
            currentInfo = null;
        }

        currentInfo = infoToShow;
        currentInfo.gameObject.SetActive(true);
    }

    public void DisableCurrentInfo()
    {
        if (currentInfo != null)
            currentInfo.gameObject.SetActive(false);
    }

    

    public void GainCoin(GameObject a)
    {
        Animator b = a.GetComponent<Animator>();

        b.SetTrigger("Collect");

    }

    public void GainCoin()
    {
        Animator b = floopyDiskUI.GetComponent<Animator>();

        b.SetTrigger("Collect");

    }

    public void GainScore()
    {
        Animator b = scoreUI.GetComponentInParent<Animator>();

        b.SetTrigger("Collect");

    }

    public void LoseScore()
    {
        Animator b = scoreUI.GetComponentInParent<Animator>();

        b.SetTrigger("Substract");

    }

    
    public void SpawnFloppyUI()
    {
        Instantiate(floppyPrefab, transform);
    }


    public void SpawnScoreUI(int amount)
    {
        GameObject m = (GameObject)Instantiate(spawnScorePrefab, spawnScorePos.transform);
        //m.GetComponent<ScoreAdditionWait>().amount = amount;

    }
    */
}
