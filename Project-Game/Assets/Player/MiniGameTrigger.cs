using UnityEngine;
using UnityEngine.SceneManagement;
using Gravitons.UI.Modal;

/// <summary>
/// Az NPC-re lépve egyszeri minijáték-belépést enged.
/// Ha az NPC neve "NPC-End", akkor visszalép a főmenübe.
/// A GameObject nevének végén lévő szám (pl. "NPC2" → 2) adja a
/// Scene-offsetet. Ha a játékos már végigjátszotta, nem engedi újra.
/// </summary>
public class MiniGameTrigger : MonoBehaviour
{
    public static string CurrentMiniGameId { get; private set; }
    bool introShown;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player") || introShown)
            return;

        introShown = true;
        string npcId = gameObject.name; // pl. "NPC1", "NPC2", ..., "NPC-End"

        // ha ez az NPC-End, akkor visszalépünk a főmenübe
        if (npcId.Equals("NPC-End", System.StringComparison.OrdinalIgnoreCase))
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        // ha már kész → csak üzenetet mutat
        if (DataManager.Instance.HasFinished(npcId))
        {
            ModalManager.Show(
                "Már teljesítve",
                "Ezt a mini-játékot már megcsináltad!",
                new[] { new ModalButton { Text = "OK" } }
            );
            return;
        }

        // === itt állítjuk össze az NPC-specifikus ismertetőt ===
        string title, message;
        switch (npcId)
        {
            case "NPC1":
                title = "Puzzle";
                message = "Találd meg az azonos képek párosait! Kattints a kártyákra, és ha nem pár, újra fordulnak.";
                break;
            case "NPC2":
                title = "Sudoku";
                message = "Töltsd ki a 9×9-es rácsot úgy, hogy minden sorban, oszlopban és 3×3-as blokkban egyszer szerepeljen az 1–9 szám!";
                break;
            case "NPC3":
                title = "Nonogram";
                message = "Kövesd a sor- és oszlopszámokat! Fekete cellákból kirajzolódik a rejtett kép. A négyzetekre kattintva szineződik. És az adott számú fekete négyzetnek kell lennie egy sorban/oszlopban.";
                break;
            case "NPC4":
                title = "Double Decision";
                message = "Először a középen lévő képet kell felismerned, majd irányt (fel, le, jobbra, balra) is választanod.";
                break;
            case "NPC-End":
                title = "Vége";
                message = "Gratulálok végig vitted az összes játékot!";
                break;
            default:
                title = "Mini-játék";
                message = "Készen állsz? Kattints az OK-ra a kezdéshez!";
                break;
        }
        // =============================================================

        ModalManager.Show(
            title,
            message,
            new[]
            {
                new ModalButton
                {
                    Text = "OK",
                    Callback = () => StartMiniGame(npcId)
                }
            }
        );
    }

    /// <summary>
    /// Betölti a mini-játékot a jelenlegi scene buildIndex + idx alapján.
    /// </summary>
    void StartMiniGame(string npcId)
    {
        CurrentMiniGameId = npcId;
        char last = npcId[npcId.Length - 1];
        if (!char.IsDigit(last))
        {
            Debug.LogError($"MiniGameTrigger: váratlan NPC-név: {npcId}");
            return;
        }
        int idx = int.Parse(last.ToString());
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + idx);
    }
}
