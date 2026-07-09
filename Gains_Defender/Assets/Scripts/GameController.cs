using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private bool bIsGameActive;
    private bool bIsGameOver;
    private int iScore;
    private Coroutine cHideTextCoroutine;
    [SerializeField] private GameObject gBackgroundPanel;
    [SerializeField] private GameObject gTitleText;
    [SerializeField] private GameObject gStartButton;
    [SerializeField] private GameObject gInstructionText;
    [SerializeField] private float fInstructionDuration = 5f;
    [SerializeField] private GameObject gScoreText;

    protected void InitController()
    {
        bIsGameActive = false;
        bIsGameOver = false;
        iScore = 0;
        
        if (gBackgroundPanel != null) gBackgroundPanel.SetActive(true);
        if (gTitleText != null) 
        {
            gTitleText.SetActive(true);
            Text tTextComponent = gTitleText.GetComponent<Text>();
            if (tTextComponent != null) tTextComponent.text = "GAINS DEFENDER";
        }
        if (gStartButton != null) gStartButton.SetActive(true);
        
        if (gInstructionText != null) gInstructionText.SetActive(false);
        if (gScoreText != null) gScoreText.SetActive(false);
        
        UpdateScoreUI();
    }

    public EnemySpawner eSpawner;

    void Start()
    {
        InitController();
    }

    public void StartGameSequence()
    {
        bIsGameActive = true;
        bIsGameOver = false;
        iScore = 0;
        
        if (gBackgroundPanel != null) gBackgroundPanel.SetActive(false);
        if (gTitleText != null) gTitleText.SetActive(false);
        if (gStartButton != null) gStartButton.SetActive(false);
        
        if (gInstructionText != null) 
        {
            gInstructionText.SetActive(true);
            cHideTextCoroutine = StartCoroutine(HideInstructionsRoutine());
        }

        if (gScoreText != null) gScoreText.SetActive(true);
        UpdateScoreUI();

        if (eSpawner != null)
        {
            eSpawner.StartSpawning();
        }
    }

    public void AddScore(int iAmount)
    {
        if (!bIsGameActive || bIsGameOver) return;
        
        iScore += iAmount;
        UpdateScoreUI();
    }

    public void TriggerGameOver()
    {
        if (bIsGameOver) return;
        
        bIsGameOver = true;
        bIsGameActive = false;
        
        if (eSpawner != null)
        {
            eSpawner.StopSpawning();
        }

        if (gBackgroundPanel != null) gBackgroundPanel.SetActive(true);
        if (gStartButton != null) gStartButton.SetActive(true);
        if (gScoreText != null) gScoreText.SetActive(false);
        if (gTitleText != null) 
        {
            gTitleText.SetActive(true);
            Text tTextComponent = gTitleText.GetComponent<Text>();
            if (tTextComponent != null)
            {
                tTextComponent.text = "GAME OVER\nScore: " + iScore;
            }
        }
        
        Debug.Log("Game Over! Punteggio finale: " + iScore);
    }

    private void UpdateScoreUI()
    {
        if (gScoreText != null)
        {
            Text tTextComponent = gScoreText.GetComponent<Text>();
            if (tTextComponent != null)
            {
                tTextComponent.text = "Score: " + iScore;
            }
        }
    }

    private IEnumerator HideInstructionsRoutine()
    {
        yield return new WaitForSeconds(fInstructionDuration);
        
        if (gInstructionText != null) 
        {
            gInstructionText.SetActive(false);
        }
    }
}