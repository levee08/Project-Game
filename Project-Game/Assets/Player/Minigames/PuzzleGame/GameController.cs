using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Gravitons.UI.Modal;
using CognitiveGames.Scoring;      // Difficulty, GameSettings, GameMetrics

public class PuzzleGameController : MonoBehaviour
{
    /*─────────────  INSPECTOR  ─────────────*/
    [Header("UI-Prefabs")]
    [SerializeField] private Sprite bgImage;        // hátlap
    [SerializeField] private Button cardPrefab;     // PuzzleButton prefab
    [SerializeField] private GridLayoutGroup grid;           // Puzzle Field

    [Header("Kártya-sprite-készlet (≥10)")]
    [SerializeField] private Sprite[] puzzles;

    /*─────────────  BELSŐ  ────────────────*/
    private readonly List<Button> btns = new();
    private readonly List<Sprite> gamePuzzles = new();

    private bool firstGuess, secondGuess;
    private int firstIdx, secondIdx;
    private string firstName, secondName;

    private int countGuesses, countCorrect;
    private int targetPairs;           // 6 / 8 / 10
    private int colsForDifficulty;     // 4 / 4 / 5
    private float revealTime;            // 1.2 / 1.0 / 0.8

    private float startTime;
    private Difficulty difficulty;

    /*──────────────  START  ───────────────*/
    private void Start()
    {
        difficulty = GameSettings.CurrentDifficulty;
        ApplyDifficulty();

        Debug.Log($"[Puzzle] Mode={difficulty} → pairs={targetPairs}, cols={colsForDifficulty}");

        BuildCardButtons();
        StartCoroutine(LayoutAfterCanvasScaler());

        AddListeners();
        FillPuzzleList();
        Shuffle(gamePuzzles);
        startTime = Time.time;
    }

    private IEnumerator LayoutAfterCanvasScaler()
    {
        // egy EndOfFrame után már a CanvasScaler is lefutott
        yield return new WaitForEndOfFrame();
        AdjustGridLayout();
       // grid.enabled = false;  // egyszer rendezi, utána nem piszkálja
    }

    /*──────────  NEHÉZSÉG  ˙─────────────*/
    private void ApplyDifficulty()
    {
        switch (difficulty)
        {
            case Difficulty.Könnyű:
                targetPairs = 6;
                colsForDifficulty = 4;
                revealTime = 1.2f;
                break;

            case Difficulty.Normál:
                targetPairs = 6;
                colsForDifficulty = 4;
                revealTime = 1.0f;
                break;

            case Difficulty.Nehéz:
                targetPairs = 10;
                colsForDifficulty = 5;  // ← Hard → 5 oszlop
                revealTime = 0.8f;
                break;
        }
    }

    /*──────────  KÁRTYA-ÉPÍTÉS  ───────────*/
    private void BuildCardButtons()
    {
        int need = targetPairs * 2;
        GetExistingButtons();

        // régi gombok hátlapját frissítjük
        foreach (var b in btns)
            b.image.sprite = bgImage;

        // ha kevés a gomb, újakat gyártunk
        while (btns.Count < need)
        {
            int idx = btns.Count;
            Button b = Instantiate(cardPrefab, grid.transform);
            b.name = idx.ToString();
            b.tag = "PuzzleButton";
            b.image.sprite = bgImage;
            btns.Add(b);
        }

        // ha túl sok a gomb, kikapcsoljuk a fölösleget
        for (int i = btns.Count - 1; i >= need; --i)
        {
            btns[i].gameObject.SetActive(false);
            btns.RemoveAt(i);
        }
    }

    private void GetExistingButtons()
    {
        btns.Clear();
        foreach (var go in GameObject.FindGameObjectsWithTag("PuzzleButton"))
        {
            if (go.activeSelf && go.TryGetComponent<Button>(out var b))
                btns.Add(b);
        }
    }

    /*──────────  RÁCS MÉRETEZÉSE  ──────────*/
    private void AdjustGridLayout()
    {
        int total = targetPairs * 2;
        int cols = colsForDifficulty;
        int rows = Mathf.CeilToInt((float)total / cols);

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;
        grid.childAlignment = TextAnchor.MiddleCenter;

        RectTransform rt = grid.GetComponent<RectTransform>();
        float availW = rt.rect.width    // pontosan a Canvas szélesség
             - grid.spacing.x * (cols - 1);

        float availH = rt.rect.height   // pontosan a Canvas magasság
             - grid.spacing.y * (rows - 1);

        const float aspect = 1f;  // magasság/szélesség arány (150/100)


        float cardW = Mathf.Min(availW / cols,
                                availH / rows / aspect);

        if (difficulty == Difficulty.Normál || difficulty == Difficulty.Könnyű)
        {
            grid.cellSize = new Vector2(cardW, cardW * aspect)*0.8f;
        }
        else
        {
            grid.cellSize = new Vector2(cardW, cardW * aspect)*0.8f;
        }
        

        Debug.Log($"[Puzzle] Grid: cols={cols}, rows={rows}, cell={grid.cellSize}");
    }

    /*──────────  LISTENERS  ─────────────*/
    private void AddListeners()
    {
        foreach (var b in btns)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => PickCard());
        }
    }

    /*──────────  SPRITE-LISTA  ───────────*/
    private void FillPuzzleList()
    {
        gamePuzzles.Clear();
        int half = targetPairs;
        int idx = 0;

        for (int i = 0; i < targetPairs * 2; i++)
        {
            if (idx == half) idx = 0;
            gamePuzzles.Add(puzzles[idx]);
            idx++;
        }
    }

    /*──────────  PICK/MATCH LOGIKA  ─────────*/
    public void PickCard()
    {
        int idx = int.Parse(EventSystem.current.currentSelectedGameObject.name);

        if (!firstGuess)
        {
            firstGuess = true;
            firstIdx = idx;
            firstName = gamePuzzles[idx].name;
            Reveal(idx, true);
        }
        else if (!secondGuess && idx != firstIdx)
        {
            secondGuess = true;
            secondIdx = idx;
            secondName = gamePuzzles[idx].name;
            Reveal(idx, true);
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {
        countGuesses++;
        yield return new WaitForSeconds(revealTime);

        if (firstName == secondName)
        {
            LockMatched(firstIdx);
            LockMatched(secondIdx);
            countCorrect++;
            if (countCorrect == targetPairs)
                EndGame();
        }
        else
        {
            Reveal(firstIdx, false);
            Reveal(secondIdx, false);
        }

        firstGuess = secondGuess = false;
    }

    private void Reveal(int idx, bool show)
    {
        btns[idx].image.sprite = show ? gamePuzzles[idx] : bgImage;
        btns[idx].interactable = !show;
    }

    private void LockMatched(int idx)
    {
        btns[idx].image.color = new Color(0, 0, 0, 0);
        btns[idx].interactable = false;
    }

    /*──────────  JÁTÉK VÉGE  ─────────────*/
    private void EndGame()
    {
        int mistakes = countGuesses - countCorrect;
        float elapsed = Time.time - startTime;

        var metrics = new GameMetrics("Puzzle", elapsed,
                                      countCorrect, mistakes, difficulty);
        
        ScoreManager.Instance?.OnGameFinished(metrics);
        DataManager.Instance.MarkFinished(MiniGameTrigger.CurrentMiniGameId);

        ModalManager.Show(
            "Gratulálok!",
            $"Próbálkozások: {countGuesses}",
            new[]
            {
                new ModalButton { Text = "OK", Callback = BackToMainGame }
            }
        );
    }

    /*──────────  HELPER  ───────────────────*/
    private void Shuffle(List<Sprite> list)
    {
        for (int i = list.Count - 1; i > 0; --i)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void BackToMainGame() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
}
