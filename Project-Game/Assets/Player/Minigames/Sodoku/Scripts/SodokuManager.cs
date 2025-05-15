using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Gravitons.UI.Modal;
using CognitiveGames.Scoring;  // ▶ ScoreManager, Difficulty, GameMetrics

public class SudokuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] RectTransform sudokuGridParent;
    [SerializeField] GameObject cellPrefab;
    [SerializeField] TMP_Text errorCounterText;

    [Header("Gameplay")]
    [SerializeField] Difficulty difficulty;

    int[,] solutionGrid = new int[9, 9];
    bool[,] isGeneratedCell = new bool[9, 9];
    int errorCount;
    float startTime;

    int cluesToKeep;

    void Start()
    {
        startTime = Time.time;
        difficulty = GameSettings.CurrentDifficulty;
        ApplyDifficultySettings();

        GenerateFullGrid();
        RemoveCluesWithUniqueness(81 - cluesToKeep);
        CreateSudokuUI();
        UpdateErrorCounter();
    }

    void ApplyDifficultySettings()
    {
        switch (difficulty)
        {
            case Difficulty.Könnyű: cluesToKeep = 60; break;
            case Difficulty.Normál: cluesToKeep = 50; break;
            case Difficulty.Nehéz: cluesToKeep = 40; break;
            default: cluesToKeep = 30; break;
        }
    }

    void GenerateFullGrid()
    {
        Array.Clear(solutionGrid, 0, solutionGrid.Length);
        FillGrid(0, 0);
    }

    bool FillGrid(int row, int col)
    {
        if (row == 9) return true;
        int nr = (col == 8 ? row + 1 : row);
        int nc = (col == 8 ? 0 : col + 1);

        foreach (int n in Enumerable.Range(1, 9).OrderBy(_ => UnityEngine.Random.value))
        {
            if (IsSafeToPlace(solutionGrid, row, col, n))
            {
                solutionGrid[row, col] = n;
                if (FillGrid(nr, nc)) return true;
                solutionGrid[row, col] = 0;
            }
        }
        return false;
    }

    void RemoveCluesWithUniqueness(int toRemove)
    {
        var coords = Enumerable.Range(0, 9)
            .SelectMany(r => Enumerable.Range(0, 9).Select(c => (r, c)))
            .OrderBy(_ => UnityEngine.Random.value)
            .ToList();

        int removed = 0;
        foreach (var (r, c) in coords)
        {
            if (removed >= toRemove) break;
            int backup = solutionGrid[r, c];
            solutionGrid[r, c] = 0;

            int sols = CountSolutions(solutionGrid, 2);
            if (sols == 1)
                removed++;
            else
                solutionGrid[r, c] = backup;
        }
    }

    int CountSolutions(int[,] grid, int limit)
    {
        // klónozzuk a rácsot, hogy ne piszkáljuk a főgridet
        int[,] clone = (int[,])grid.Clone();
        return CountSolutionsRec(clone, limit);
    }

    int CountSolutionsRec(int[,] g, int limit)
    {
        // Keressünk egy üres cellát
        int er = -1, ec = -1;
        bool found = false;
        for (int r = 0; r < 9 && !found; r++)
            for (int c = 0; c < 9; c++)
                if (g[r, c] == 0)
                {
                    er = r; ec = c;
                    found = true;
                    break;
                }

        if (!found)
            return 1;  // pöccre megvan egy megoldás

        int total = 0;
        for (int n = 1; n <= 9; n++)
        {
            if (IsSafeToPlace(g, er, ec, n))
            {
                g[er, ec] = n;
                total += CountSolutionsRec(g, limit);
                g[er, ec] = 0;  // visszaállítás

                if (total >= limit)
                    return total;
            }
        }
        return total;
    }

    bool IsSafeToPlace(int[,] g, int rr, int cc, int num)
    {
        for (int i = 0; i < 9; i++)
            if ((i != cc && g[rr, i] == num) ||
                (i != rr && g[i, cc] == num))
                return false;

        int br = (rr / 3) * 3, bc = (cc / 3) * 3;
        for (int r = br; r < br + 3; r++)
            for (int c = bc; c < bc + 3; c++)
                if ((r != rr || c != cc) && g[r, c] == num)
                    return false;

        return true;
    }

    void CreateSudokuUI()
    {
        foreach (Transform ch in sudokuGridParent) Destroy(ch.gameObject);

        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                var go = Instantiate(cellPrefab, sudokuGridParent);
                var inp = go.GetComponent<TMP_InputField>();
                var img = go.GetComponent<Image>();

                if (solutionGrid[r, c] != 0)
                {
                    inp.text = solutionGrid[r, c].ToString();
                    inp.interactable = false;
                    img.color = Color.white;
                    isGeneratedCell[r, c] = true;
                }
                else
                {
                    inp.text = "";
                    inp.interactable = true;
                    isGeneratedCell[r, c] = false;
                    int rr = r, cc = c;
                    inp.onValueChanged.AddListener(_ => HandleValue(rr, cc, inp));
                    inp.onEndEdit.AddListener(_ => ValidateInput(rr, cc, inp));
                }
            }
    }

    void HandleValue(int r, int c, TMP_InputField f)
    {
        if (string.IsNullOrWhiteSpace(f.text))
        {
            solutionGrid[r, c] = 0;
            f.image.color = Color.white;
        }
    }

    void ValidateInput(int r, int c, TMP_InputField f)
    {
        if (int.TryParse(f.text, out int n) && n >= 1 && n <= 9
            && IsSafeToPlace(solutionGrid, r, c, n))
        {
            solutionGrid[r, c] = n;
            f.image.color = Color.white;
            CheckCompletion();
        }
        else
        {
            f.image.color = Color.red;
            errorCount++;
            UpdateErrorCounter();
        }
    }

    void UpdateErrorCounter() =>
        errorCounterText.text = $"Hibák száma: {errorCount}";

    void CheckCompletion()
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (solutionGrid[r, c] == 0
                 || !IsSafeToPlace(solutionGrid, r, c, solutionGrid[r, c]))
                    return;

        float elapsed = Time.time - startTime;
        var metrics = new GameMetrics(
            "Sudoku", elapsed, 81, errorCount, difficulty);
        ScoreManager.Instance?.OnGameFinished(metrics);
        DataManager.Instance.MarkFinished(MiniGameTrigger.CurrentMiniGameId);

        ModalManager.Show(
            "Gratulálok!",
            $"Megoldva {elapsed:F1} mp alatt\nHibák: {errorCount}",
            new[] { new ModalButton { Text = "OK", Callback = BackToMainGame } }
        );
    }

    void BackToMainGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
    }
}
