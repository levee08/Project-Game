using Assets.Player;
using Gravitons.UI.Modal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RowClickHandler : MonoBehaviour
{
    private string playerName;
    private string playerScore;
    private string playerCustomID;
   

    [SerializeField] private TextMeshProUGUI nameText; // Inspectorban állítsd be
    [SerializeField] private TextMeshProUGUI scoreText; // Inspectorban állítsd be

    public void Initialize(string name, string score, string customID)
    {
        Debug.Log($"Initialize hívva. Adatok: Név: {name}, Pontszám: {score}, Egyedi azonosító: {customID}");

        // Adatok mentése
        playerName = name;
        playerScore = score;
        playerCustomID = customID;

        // Szövegek frissítése
        if (nameText != null) nameText.text = name;
        if (scoreText != null) scoreText.text = score;

        // Gomb esemény hozzárendelése
        Button button = GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners(); // Előző események törlése
            button.onClick.AddListener(OnRowClicked); // Új esemény hozzáadása
            Debug.Log("Button esemény sikeresen hozzárendelve.");
        }
        else
        {
            Debug.LogError("Hiba: Button komponens nem található!");
        }
    }

    private void OnRowClicked()
    {
        if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(playerScore) || string.IsNullOrEmpty(playerCustomID))
        {
            Debug.LogError("Hiba: Az adatok még nincsenek inicializálva!");
            return; // Ne folytasd, ha nincsenek adatok
        }
        SelectedPlayerData.PlayerName = playerName;
        SelectedPlayerData.PlayerScore = playerScore;
        SelectedPlayerData.PlayerCustomID = playerCustomID;

        Debug.Log($"Gombra kattintottak! Név: {playerName}, Pontszám: {playerScore}, Egyedi azonosító: {playerCustomID}");

        ModalManager.Show("Játékos adatai", $"Név: {playerName}\nPontszám: {playerScore}\nAzonosító: {playerCustomID}",
            new[] { new ModalButton() { Text = "Ok" } });
    }
}
