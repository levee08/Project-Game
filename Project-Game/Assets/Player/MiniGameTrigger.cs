using UnityEngine;
using Gravitons.UI.Modal; // ModalManager használatához
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

public class MiniGameTrigger : MonoBehaviour
{
    private bool isIntroShown = false;
    public string aktNPC = "";
    char idxchar = ' ';
    int idx = 0;
    private void OnTriggerEnter2D(Collider2D other)
    {
       aktNPC = gameObject.name;
       idxchar = aktNPC.ToCharArray()[aktNPC.Length-1];
         idx = int.Parse(idxchar.ToString());
        
        if (other.CompareTag("Player") && !isIntroShown)
        {
            Debug.Log("triggered");
            isIntroShown = true; // Csak egyszer jelenjen meg

            // Bevezető szöveg megjelenítése modalként
            ModalManager.Show(
                "Mini-játék kezdete",
                "Üdvözlünk! Ez egy mini-játék bevezető szövege. Kattints az 'OK'-ra a folytatáshoz.",
                new[]
                {
                    new ModalButton() { Text = "OK", Callback = StartMiniGame }
                }
            );
        }
    }

    private void StartMiniGame()
    {
        Debug.Log("Mini-játék elindítva!" + idx);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + idx);
    }
}
