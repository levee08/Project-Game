using Gravitons.UI.Modal;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NonogramManager : MonoBehaviour
{
    public int gridSize = 5; // A játék tényleges mérete
    public Transform gridParent; // A GridPanel objektum
    public GameObject cellPrefab; // Egy kattintható cella prefab
    public GameObject numberTextPrefab; // Szám megjelenítésére szolgáló prefab

    private int[,] solutionGrid; // Generált megoldás rácsa
    private int[,] playerGrid; // Játékos aktuális megoldása
    private List<List<int>> rowClues; // Sorok szabályai
    private List<List<int>> columnClues; // Oszlopok szabályai

    private float startTime; // A játék indításának időpontja
    private bool isGameOver = false; // Ellenőrzi, hogy vége van-e a játéknak


    void Start()
    {
        GenerateRandomSolution(); // Véletlenszerű rács generálása
        GenerateClues(); // Szabályok generálása
        CreateGridUI(); // A teljes rács UI létrehozása
    }

    /// <summary>
    /// Véletlenszerű rács generálása
    /// </summary>
    void GenerateRandomSolution()
    {
        solutionGrid = new int[gridSize, gridSize];
        playerGrid = new int[gridSize, gridSize];

        // Random rács generálása
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                // Véletlenszerű 1 vagy 0
                solutionGrid[row, col] = Random.Range(0, 2); // 0 vagy 1
            }
        }
    }

    /// <summary>
    /// Szabályok generálása a megoldás rácsa alapján
    /// </summary>
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

    /// <summary>
    /// Sor/oszlop szabályainak kiszámítása
    /// </summary>
    List<int> ExtractClues(int[,] grid, int index, bool isRow)
    {
        List<int> clues = new List<int>();
        int count = 0;

        for (int i = 0; i < gridSize; i++)
        {
            int value = isRow ? grid[index, i] : grid[i, index];
            if (value == 1)
            {
                count++;
            }
            else if (count > 0)
            {
                clues.Add(count);
                count = 0;
            }
        }

        if (count > 0)
            clues.Add(count);

        return clues.Count > 0 ? clues : new List<int> { 0 };
    }

    /// <summary>
    /// A teljes rács UI létrehozása (számokkal és kattintható cellákkal)
    /// </summary>
    void CreateGridUI()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        for (int row = 0; row <= gridSize; row++)
        {
            for (int col = 0; col <= gridSize; col++)
            {
                if (row == 0 && col == 0)
                {
                    CreateEmptyCell();
                }
                else if (row == 0)
                {
                    CreateNumberCell(columnClues[col - 1]);
                }
                else if (col == 0)
                {
                    CreateNumberCell(rowClues[row - 1]);
                }
                else
                {
                    CreateClickableCell(row - 1, col - 1);
                }
            }
        }
    }

    void CreateNumberCell(List<int> clues)
    {
        GameObject numberCell = Instantiate(numberTextPrefab, gridParent);
        TMP_Text textComponent = numberCell.GetComponentInChildren<TMP_Text>();
        textComponent.text = string.Join(" ", clues);
    }

    void CreateClickableCell(int row, int col)
    {
        GameObject cell = Instantiate(cellPrefab, gridParent);
        cell.GetComponent<Button>().onClick.AddListener(() =>
        {
            ToggleCell(row, col, cell);
        });
    }

    void CreateEmptyCell()
    {
        GameObject emptyCell = Instantiate(numberTextPrefab, gridParent);
        TMP_Text textComponent = emptyCell.GetComponentInChildren<TMP_Text>();
        textComponent.text = "";
    }

    void ToggleCell(int row, int col, GameObject cell)
    {
        if (isGameOver) return;
        playerGrid[row, col] = playerGrid[row, col] == 1 ? 0 : 1;
        cell.GetComponent<Image>().color = playerGrid[row, col] == 1 ? Color.black : Color.white;

        if (ValidateSolution())
        {
            isGameOver = true; // Játék vége
            float elapsedTime = Time.time - startTime;
            int minutes = (int)(elapsedTime / 60); // Percek
            int seconds = (int)(elapsedTime % 60); // Másodpercek
            ModalManager.Show("Végeztél",
               $"{minutes} perc és {seconds} másodperc idő alatt",
               new[]
               {
                    new ModalButton() { Text = "OK", Callback = BackToMainGame }
               }
           );
            Debug.Log("Helyes megoldás!");
        }
    }

    bool ValidateSolution()
    {
        for (int row = 0; row < gridSize; row++)
        {
            if (!ValidateLine(playerGrid, row, rowClues[row], true))
                return false;
        }

        for (int col = 0; col < gridSize; col++)
        {
            if (!ValidateLine(playerGrid, col, columnClues[col], false))
                return false;
        }

        return true;
    }

    bool ValidateLine(int[,] grid, int index, List<int> clues, bool isRow)
    {
        List<int> extracted = ExtractClues(grid, index, isRow);
        return extracted.SequenceEqual(clues);
    }
    void BackToMainGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 3);
    }
}
