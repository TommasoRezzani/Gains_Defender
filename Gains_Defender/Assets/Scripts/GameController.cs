using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private bool bIsGameActive;
    private Coroutine cHideTextCoroutine;
    [SerializeField] private GameObject gBackgroundPanel;
    [SerializeField] private GameObject gTitleText;
    [SerializeField] private GameObject gStartButton;
    [SerializeField] private GameObject gInstructionText;
    [SerializeField] private float fInstructionDuration = 5f;

    protected void InitController()
    {
        bIsGameActive = false;

        if (gBackgroundPanel != null) gBackgroundPanel.SetActive(true);
        if (gTitleText != null) gTitleText.SetActive(true);
        if (gStartButton != null) gStartButton.SetActive(true);
        if (gInstructionText != null) gInstructionText.SetActive(false);
    }

    void Start()
    {
        InitController();
    }

    public void StartGameSequence()
    {
        bIsGameActive = true;

        if (gBackgroundPanel != null) gBackgroundPanel.SetActive(false);
        if (gTitleText != null) gTitleText.SetActive(false);
        if (gStartButton != null) gStartButton.SetActive(false);
        
        // Mostra le istruzioni e avvia il timer
        if (gInstructionText != null) 
        {
            gInstructionText.SetActive(true);
            cHideTextCoroutine = StartCoroutine(HideInstructionsRoutine());
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