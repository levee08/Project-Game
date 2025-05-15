using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Gravitons.UI.Modal;
using CognitiveGames.Scoring;  // ▶ ScoreManager, Difficulty, GameMetrics

public class NonogramManager : MonoBehaviour
{
    // ----------------- Inspector -----------------
    [Header("Beállítások")]
    [SerializeField] int gridSize = 5;
    [SerializeField] CognitiveGames.Scoring.Difficulty difficulty;

    [Header("Prefabok / Szülő")]
    [SerializeField] Transform gridParent;
    [SerializeField] GameObject cellPrefab;
    [SerializeField] GameObject numberTextPrefab;

    // ----------------- Belső mezők -----------------
    int[,] solutionGrid;
    int[,] playerGrid;
    List<List<int>> rowClues;
    List<List<int>> columnClues;

    float startTime;
    bool isGameOver;
    int mistakeCount;

    // időkorlát és hibakorlát nehézség szerint
    float timeLimit;
    int maxMistakesAllowed;

    void Start()
    {
        difficulty = GameSettings.CurrentDifficulty;
        ApplyDifficultySettings();

        startTime = Time.time;
        mistakeCount = 0;
        solutionGrid = new int[gridSize, gridSize];
        playerGrid = new int[gridSize, gridSize];

        GenerateRandomSolution();
        GenerateClues();
        CreateGridUI();
    }

    void Update()
    {
        if (!isGameOver && Time.time - startTime > timeLimit)
        {
            FailGame("Idő lejárt", $"A rendelkezésre álló {timeLimit:F0} mp lejárt.");
        }
    }

    // =======================================================================
    //  Nehézség-specifikus beállítások: időkorlát és megengedett hibaszám
    // =======================================================================
    void ApplyDifficultySettings()
    {
        switch (difficulty)
        {
            case Difficulty.Könnyű:
                timeLimit = 600f;    // 10 perc
                maxMistakesAllowed = int.MaxValue;
                break;
            case Difficulty.Normál:
                timeLimit = 300f;    // 5 perc
                maxMistakesAllowed = 10;
                break;
            case Difficulty.Nehéz:
                timeLimit = 120f;    // 2 perc
                maxMistakesAllowed = 5;
                break;
            default:
                timeLimit = 300f;
                maxMistakesAllowed = 10;
                break;
        }

        Debug.Log($"[Nonogram] Difficulty={difficulty}, timeLimit={timeLimit}s, maxMistakes={maxMistakesAllowed}");
    }

    // =======================================================================
    //  Pályagenerálás
    // =======================================================================
    void GenerateRandomSolution()
    {
        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
                solutionGrid[r, c] = Random.Range(0, 2);
    }

    void GenerateClues()
    {
        rowClues = new List<List<int>>();
        columnClues = new List<List<int>>();

        for (int i = 0; i < gridSize; i++)
        {
            rowClues.Add(ExtractClues(solutionGrid, i, true));
            columnClues.Add(ExtractClues(solutionGrid, i, false));
        }
    }

    List<int> ExtractClues(int[,] grid, int index, bool isRow)
    {
        var clues = new List<int>();
        int count = 0;
        for (int i = 0; i < gridSize; i++)
        {
            int val = isRow ? grid[index, i] : grid[i, index];
            if (val == 1) count++;
            else if (count > 0) { clues.Add(count); count = 0; }
        }
        if (count > 0) clues.Add(count);
        return clues.Count > 0 ? clues : new List<int> { 0 };
    }

    // =======================================================================
    //  UI-építés
    // =======================================================================
    void CreateGridUI()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        for (int r = 0; r <= gridSize; r++)
        {
            for (int c = 0; c <= gridSize; c++)
            {
                if (r == 0 && c == 0) CreateEmptyCorner();
                else if (r == 0) CreateNumberCell(columnClues[c - 1]);
                else if (c == 0) CreateNumberCell(rowClues[r - 1]);
                else CreateClickableCell(r - 1, c - 1);
            }
        }
    }

    void CreateNumberCell(List<int> clues)
    {
        var obj = Instantiate(numberTextPrefab, gridParent);
        obj.GetComponentInChildren<TMP_Text>().text = string.Join(" ", clues);
    }

    void CreateEmptyCorner()
    {
        var obj = Instantiate(numberTextPrefab, gridParent);
        obj.GetComponentInChildren<TMP_Text>().text = "";
    }

    void CreateClickableCell(int row, int col)
    {
        GameObject cell = Instantiate(cellPrefab, gridParent);
        cell.GetComponent<Button>().onClick.AddListener(() => ToggleCell(row, col, cell));
    }

    // =======================================================================
    //  Játéklogika: kattintás, hibakorlát és megoldás-ellenőrzés
    // =======================================================================
    void ToggleCell(int row, int col, GameObject cell)
    {
        if (isGameOver) return;

        // állapotváltás
        playerGrid[row, col] = 1 - playerGrid[row, col];
        cell.GetComponent<Image>().color =
            playerGrid[row, col] == 1 ? Color.black : Color.white;

        // hibaszám növelése, ha tévedés
        if (playerGrid[row, col] != solutionGrid[row, col])
        {
            mistakeCount++;

            if (mistakeCount > maxMistakesAllowed)
            {
                isGameOver = true;

                // ── Pontszámítás hibás végződés esetén ──
                float elapsed = Time.time - startTime;
                int correctCount = playerGrid.Cast<int>().Count(v => v == 1 && /* helyes */ true);
                var metrics = new GameMetrics(
                    "Nonogram",
                    elapsed,
                    correctCount,
                    mistakeCount,
                    difficulty
                );
                ScoreManager.Instance?.OnGameFinished(metrics);
                DataManager.Instance.MarkFinished(MiniGameTrigger.CurrentMiniGameId);

                ModalManager.Show(
                    "Túl sok hiba",
                    $"Több mint {maxMistakesAllowed} hibát követtél el.\nHelyes mezők: {correctCount}",
                    new[]
                    {
                    new ModalButton { Text = "OK", Callback = BackToMainGame }
                    }
                );
                return;
            }
        }

        // ha kész a megoldás
        if (ValidateSolution())
            FinishGame();
    }


    bool ValidateSolution()
    {
        for (int r = 0; r < gridSize; r++)
            if (!ExtractClues(playerGrid, r, true).SequenceEqual(rowClues[r]))
                return false;

        for (int c = 0; c < gridSize; c++)
            if (!ExtractClues(playerGrid, c, false).SequenceEqual(columnClues[c]))
                return false;

        return true;
    }

    // =======================================================================
    //  Befejezés: sikeres vagy sikertelen
    // =======================================================================
    void FinishGame() => EndGame(success: true, title: "Gratulálok!", body: $"Megoldva {(int)((Time.time - startTime) / 60)}p {(int)((Time.time - startTime) % 60)}s alatt\nHibák: {mistakeCount}");

    void FailGame(string title, string body) => EndGame(success: false, title: title, body: body);

    void EndGame(bool success, string title, string body)
    {
        isGameOver = true;
        float elapsed = Time.time - startTime;

        int correctCount = success ? playerGrid.Cast<int>().Count(v => v == 1) : 0;
        int mistakes = mistakeCount;

        // Pontszámítás
        var metrics = new GameMetrics(
            "Nonogram",
            elapsed,
            correctCount,
            mistakes,
            difficulty);

        ScoreManager.Instance?.OnGameFinished(metrics);
        DataManager.Instance.MarkFinished(MiniGameTrigger.CurrentMiniGameId);

        // Modal
        ModalManager.Show(
            title,
            body,
            new[] { new ModalButton { Text = "OK", Callback = BackToMainGame } }
        );

        Debug.Log($"Nonogram vége ▶ success={success}, time={elapsed:F1}s, mistakes={mistakes}");
    }

    void BackToMainGame() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 3);
}
