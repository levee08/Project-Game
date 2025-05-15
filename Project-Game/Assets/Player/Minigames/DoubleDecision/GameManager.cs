using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Gravitons.UI.Modal;
using CognitiveGames.Scoring;  // ▶ ScoreManager, Difficulty, GameMetrics

public class DoubleDecisionGameManager : MonoBehaviour
{
    // ---------------- Inspector ----------------
    [Header("Stimulus képek")]
    [SerializeField] Image centerImage;
    [SerializeField] Image peripheralImage;

    [Header("UI panelek")]
    [SerializeField] GameObject answerButtonsPanel;
    [SerializeField] GameObject peripheralSelectorPanel;

    [Header("Sprite-k")]
    [SerializeField] Sprite[] centerSprites;
    [SerializeField] Sprite[] peripheralSprites;

    [Header("Nehézség")]
    [SerializeField] CognitiveGames.Scoring.Difficulty difficulty;

    // ---------------- Belső állapot ----------------
    string currentCenterName;
    string currentPeripheralDirection;

    float stimulusTime;                // ← most már változik

    string playerCenterAnswer;
    string playerPeripheralAnswer;

    int correctAnswersInTotal;
    int totalRounds;
    public int targetCorrectAnswers = 3;

    bool gameEnded;
    float startTime;                   // stopper

    private void Start()
    {
        // --- 0) Gombpanelek alapból kikapcs
        answerButtonsPanel.SetActive(false);
        peripheralSelectorPanel.SetActive(false);

        // --- 1) Nehézség beolvasása
        difficulty = GameSettings.CurrentDifficulty ;
        ApplyDifficulty();

        // --- 2) Indítás
        startTime = Time.time;
        StartCoroutine(RunTrial());
    }


    IEnumerator RunTrial()
    {
        yield return new WaitForSeconds(1f);

        //  Képek kisorsolása
        int centralIdx = Random.Range(0, centerSprites.Length);
        currentCenterName = centerSprites[centralIdx].name;
        centerImage.sprite = centerSprites[centralIdx];

        int peripheralIdx = Random.Range(0, peripheralSprites.Length);
        currentPeripheralDirection = GetRandomDirection();
        peripheralImage.sprite = peripheralSprites[peripheralIdx];
        SetPeripheralPosition(currentPeripheralDirection);

        //  Stimulus megjelenítése
        centerImage.enabled = true;
        peripheralImage.enabled = true;
        yield return new WaitForSeconds(stimulusTime);

        // Stimulus eltűnik, UI válaszpanelen
        centerImage.enabled = false;
        peripheralImage.enabled = false;

        answerButtonsPanel.SetActive(true);
        peripheralSelectorPanel.SetActive(true);
    }

    public void OnCenterChoice(string choice)
    {
        playerCenterAnswer = choice;
        answerButtonsPanel.SetActive(false);
        CheckIfBothAnswered();
    }

    public void OnPeripheralChoice(string dir)
    {
        playerPeripheralAnswer = dir;
        peripheralSelectorPanel.SetActive(false);
        CheckIfBothAnswered();
    }

    void CheckIfBothAnswered()
    {
        if (playerCenterAnswer == null || playerPeripheralAnswer == null) return;

        totalRounds++;
        bool okCenter = playerCenterAnswer == currentCenterName;
        bool okPeripheral = playerPeripheralAnswer == currentPeripheralDirection;

        if (okCenter && okPeripheral) correctAnswersInTotal++;

        if (correctAnswersInTotal >= targetCorrectAnswers)
            EndGame();
        else
            StartCoroutine(NextRound());
    }

    IEnumerator NextRound()
    {
        if (gameEnded) yield break;
        playerCenterAnswer = playerPeripheralAnswer = null;
        yield return new WaitForSeconds(1f);
        StartCoroutine(RunTrial());
    }

    void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        answerButtonsPanel.SetActive(false);
        peripheralSelectorPanel.SetActive(false);

        float elapsed = Time.time - startTime;
        int mistakes = totalRounds - correctAnswersInTotal;

        var metrics = new GameMetrics(
                        "DoubleDecision",
                        elapsed,
                        correctAnswersInTotal,
                        mistakes,
                        difficulty);

        ScoreManager.Instance?.OnGameFinished(metrics);
        DataManager.Instance.MarkFinished(MiniGameTrigger.CurrentMiniGameId);

        ModalManager.Show(
            "Gratulálok!",
            $"Mind a {targetCorrectAnswers} helyes válasz!\nPróbálkozások: {totalRounds}",
            new[] { new ModalButton { Text = "OK", Callback = BackToMainGame } }
        );

        Debug.Log($"Double Decision kész ▶ Time={elapsed:F1}s  Correct={correctAnswersInTotal}/{totalRounds}");
    }

    void BackToMainGame() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 4);

    static string GetRandomDirection()
    {
        string[] dir = { "Left", "Right", "Up", "Down" };
        return dir[Random.Range(0, dir.Length)];
    }

    void SetPeripheralPosition(string direction)
    {
        RectTransform rt = peripheralImage.rectTransform;
        rt.anchoredPosition = direction switch
        {
            "Left" => new Vector2(-400, 0),
            "Right" => new Vector2(400, 0),
            "Up" => new Vector2(0, 200),
            "Down" => new Vector2(0, -200),
            _ => Vector2.zero
        };
    }

    // ── Itt adjuk hozzá a stimulusTime változtatást ──
    void ApplyDifficulty()
    {
        switch (difficulty)
        {
            case Difficulty.Könnyű:
                stimulusTime = 1.5f;
                break;
            case Difficulty.Normál:
                stimulusTime = 1.0f;
                break;
            case Difficulty.Nehéz:
                stimulusTime = 0.7f;
                break;
        }
    }
}
