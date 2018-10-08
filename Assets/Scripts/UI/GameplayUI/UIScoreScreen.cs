using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIScoreScreen : UIScreen
{
    [Header("Fade In / Out Settings")]
    public Image fadeImage;
    public float fadeSpeed = 0.01f;

    [Header("Stats Options")]
    public GameObject statsContent;
    public Text scoreText;
    public Text floppyText;
    public Text hackText;
    public Text deadText;

    public GameObject buttonToContinue;

    internal bool isEnding;
    private bool isReady;
    private bool canContinue;
	
	// Update is called once per frame
	void Update ()
    {
 	}

    public void FadeIn()
    {
        fadeImage.color = Color.Lerp(fadeImage.color, new Color(1, 1, 1, 1.0f), fadeSpeed);
    }

    public void FadeOut()
    {
        fadeImage.color = Color.Lerp(fadeImage.color, new Color(1, 1, 1, 0.0f), fadeSpeed);
    }

    public void EndScene()
    {
        FadeIn();

        // If the screen is almost black...
        if (fadeImage.color.a >= 0.95f && !isReady)
        {
            isReady = true;
            isEnding = false;
        }
    }

    private void ShowStats()
    {
        //scoreText.text = ScoreManager.instance.score.ToString();
        //floppyText.text = ScoreManager.instance.totalFloppyDisks.ToString() + " / " + ScoreManager.instance.totalFloppyOnLevel.ToString();
        //hackText.text = ScoreManager.instance.hackedRobots.ToString();
       // deadText.text = ScoreManager.instance.deadTimes.ToString();

        statsContent.SetActive(true);
        WaitToShowStats();
    }

    private IEnumerator WaitToShowStats()
    {
        ShowStats();
        yield return new WaitForSeconds(0.5f);
        buttonToContinue.SetActive(true);
        canContinue = true;
    }
}
