using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Hivatkozások")]
    [SerializeField] GameObject settingsOverlay;   // a SettingsOverlay panel (Dimmer + Backdrop)

    // -------------------------------

    public void PlayGame() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    public void OpenSettings() =>
        settingsOverlay.SetActive(true);           // csak megjelenítjük az overlay-t

    public void QuitGame() =>
        Application.Quit();
}
