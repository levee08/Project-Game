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
                $"Játék leirás, mit kell csinálni, mikor, pontozás, hány feledat. Stb.. Stb..",
                new[] { new ModalButton() { Text = "Ok" } }
            );
        }
       
    }
}
