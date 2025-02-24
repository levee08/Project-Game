using Gravitons.UI.Modal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleExit : MonoBehaviour
{

    public void QuitGame()
    {
        Debug.Log("kattintva");
        ModalManager.Show("Kil�p�s",
            "Biztos beakarod fejezni a j�t�kot?",
              new[]
              {
                    new ModalButton() { Text = "Igen", Callback = BackToMainGame },
                    new ModalButton() { Text = "Nem" }
              }
          );
    }
    void BackToMainGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

}
