// SettingsMenu.cs
// H�zd r� a SettingsMenu panelre!

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CognitiveGames.Scoring;   // itt van a Difficulty �s GameSettings

public class SettingsMenu : MonoBehaviour
{
    [Header("UI-hivatkoz�sok")]
    [SerializeField] private GameObject mainMenuPanel;      // a J�t�k / Be�ll�t�sok / Kil�p�s panel
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Button backButton;

    void Awake()
    {
        /*-------------------------------------------------
         * 2) Indul�skor �ll�tsuk a dropdown poz�ci�j�t arra,
         *    amit legut�bb kiv�lasztottak
         *------------------------------------------------*/
        difficultyDropdown.value = (int)GameSettings.CurrentDifficulty;

        /*-------------------------------------------------
         * 3) Event-feliratkoz�sok
         *------------------------------------------------*/
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        backButton.onClick.AddListener(CloseSettings);
    }

    /*-------------------------------*/
    void OnDifficultyChanged(int idx)
    {
        GameSettings.CurrentDifficulty = (Difficulty)idx;
        Debug.Log($"Neh�zs�g be�ll�tva: {GameSettings.CurrentDifficulty}");
    }

    /*-------------------------------*/
    void CloseSettings()
    {
        // Panel-kapcsolgat�s
        gameObject.SetActive(false);     // SettingsMenu OFF
        mainMenuPanel.SetActive(true);   // MainMenu     ON
    }
}
