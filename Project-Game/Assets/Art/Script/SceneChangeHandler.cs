using UnityEngine;
using UnityEngine.SceneManagement;
using Gravitons.UI.Modal;

public class SceneChangeHandler : MonoBehaviour
{
    public static bool ishown;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; 
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Jelenet betöltve: {scene.name}");
        if (scene.name == "MainGame"&&!ishown)
        {ishown = true;
            // Modal megjelenítése
            ModalManager.Show(
                "Játék Ismertető",
                $"Üdvözöllek a játékban! A feladatod az lesz, hogy a nyilak segítségével navigálj oda, az egyes karakterekhez. A kezdési pozíciódtól fentre indulj el, majd menj körbe minden egyes karakteren. A játékot a tőled jobbra található karakterhez sétálva tudod befejezni.",
                new[] { new ModalButton() { Text = "Ok" } }
            );
        }
       
    }
}
