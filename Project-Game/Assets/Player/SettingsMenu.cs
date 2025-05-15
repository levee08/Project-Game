// SettingsMenu.cs
// Húzd rá a SettingsMenu panelre!

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CognitiveGames.Scoring;   // itt van a Difficulty és GameSettings

public class SettingsMenu : MonoBehaviour
{
    [Header("UI-hivatkozások")]
    [SerializeField] private GameObject mainMenuPanel;      // a Játék / Beállítások / Kilépés panel
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Button backButton;

    void Awake()
    {
        /*-------------------------------------------------
         * 2) Induláskor állítsuk a dropdown pozícióját arra,
         *    amit legutóbb kiválasztottak
         *------------------------------------------------*/
        difficultyDropdown.value = (int)GameSettings.CurrentDifficulty;

        /*-------------------------------------------------
         * 3) Event-feliratkozások
         *------------------------------------------------*/
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        backButton.onClick.AddListener(CloseSettings);
    }

    /*-------------------------------*/
    void OnDifficultyChanged(int idx)
    {
        GameSettings.CurrentDifficulty = (Difficulty)idx;
        Debug.Log($"Nehézség beállítva: {GameSettings.CurrentDifficulty}");
    }

    /*-------------------------------*/
    void CloseSettings()
    {
        // Panel-kapcsolgatás
        gameObject.SetActive(false);     // SettingsMenu OFF
        mainMenuPanel.SetActive(true);   // MainMenu     ON
    }
}
