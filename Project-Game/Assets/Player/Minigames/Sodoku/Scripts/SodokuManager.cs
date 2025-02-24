using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SudokuManager : MonoBehaviour
{
    public RectTransform sudokuGridParent;
    public GameObject cellPrefab;
    public TMP_Text errorCounterText; // Hibaszámláló megjelenítésére szolgáló UI elem

    private int[,] sudokuGrid = new int[9, 9];
    private bool[,] isGeneratedCell = new bool[9, 9]; // Az alapból generált mezők jelölésére
    private int errorCount = 0; // Hibák számlálója

    void Start()
    {
        GenerateSudoku();
        RemoveNumbers(30);
        CreateSudokuUI();
        UpdateErrorCounter(); // Kezdetben frissítjük a hibaszámláló UI-t
    }

    private void GenerateSudoku()
    {
        while (!FillGrid(0, 0))
        {
            Debug.LogWarning("Érvénytelen rács generálva, újrapróbálkozás...");
        }
    }

    private bool FillGrid(int row, int col)
    {
        if (row == 9) return true;

        int nextRow = col == 8 ? row + 1 : row;
        int nextCol = col == 8 ? 0 : col + 1;

        List<int> numbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Shuffle(numbers);

        foreach (int number in numbers)
        {
            if (IsSafeToPlace(sudokuGrid, row, col, number))
            {
                sudokuGrid[row, col] = number;

                if (FillGrid(nextRow, nextCol)) return true;

                sudokuGrid[row, col] = 0; // Backtrack
            }
        }

        return false;
    }

    private bool IsSafeToPlace(int[,] grid, int row, int col, int number)
    {
        for (int i = 0; i < 9; i++)
        {
            if (grid[row, i] == number || grid[i, col] == number)
                return false;
        }

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (grid[startRow + i, startCol + j] == number)
                    return false;
            }
        }

        return true;
    }

    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private void RemoveNumbers(int cellsToRemove)
    {
        int removed = 0;

        while (removed < cellsToRemove)
        {
            int row = Random.Range(0, 9);
            int col = Random.Range(0, 9);

            if (sudokuGrid[row, col] != 0)
            {
                sudokuGrid[row, col] = 0;
                isGeneratedCell[row, col] = false; // Ez a mező most már felhasználó által módosítható
                removed++;
            }
        }
    }

    private void CreateSudokuUI()
    {
        foreach (Transform child in sudokuGridParent)
        {
            Destroy(child.gameObject);
        }

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                GameObject cell = Instantiate(cellPrefab, sudokuGridParent);
                TMP_InputField inputField = cell.GetComponent<TMP_InputField>();

                if (inputField == null)
                {
                    Debug.LogError("A CellPrefab nem tartalmaz TMP_InputField komponenst!");
                    continue;
                }

                if (sudokuGrid[row, col] != 0)
                {
                    inputField.text = sudokuGrid[row, col].ToString();
                    inputField.interactable = false;
                    inputField.image.color = Color.white;
                    isGeneratedCell[row, col] = true; // Ez egy alapból generált mező
                }
                else
                {
                    inputField.text = "";
                    inputField.interactable = true;
                    isGeneratedCell[row, col] = false; // Ez egy szerkeszthető mező

                    int currentRow = row;
                    int currentCol = col;

                    inputField.onValueChanged.AddListener((value) =>
                    {
                        HandleValueChange(inputField, currentRow, currentCol, value);
                    });

                    inputField.onEndEdit.AddListener((value) =>
                    {
                        ValidateUserInput(inputField, currentRow, currentCol, value);
                    });
                }
            }
        }
    }

    private void HandleValueChange(TMP_InputField inputField, int row, int col, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            sudokuGrid[row, col] = 0;
            inputField.image.color = Color.white; // Fehérré tesszük, ha törölték
        }
    }

    private void ValidateUserInput(TMP_InputField inputField, int row, int col, string value)
    {
        if (int.TryParse(value, out int number) && number >= 1 && number <= 9)
        {
            if (IsSafeToPlace(sudokuGrid, row, col, number))
            {
                sudokuGrid[row, col] = number;
                inputField.image.color = Color.white; // Helyes input
            }
            else
            {
                inputField.image.color = Color.red; // Hibás input
                IncrementErrorCount(); // Növeljük a hibák számát
            }
        }
        else
        {
            inputField.image.color = Color.red; // Nem érvényes szám
            IncrementErrorCount(); // Növeljük a hibák számát
        }
    }

    private void IncrementErrorCount()
    {
        errorCount++; // Növeljük a hibák számát
        UpdateErrorCounter(); // Frissítjük az UI-t
    }

    private void UpdateErrorCounter()
    {
        errorCounterText.text = $"Hibák száma: {errorCount}"; // Hibaszám megjelenítése
    }
}
