using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerInfoInput : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField ageInput;
    public TMP_InputField customInput; // ezt használod playerId-ként

    public void SavePlayerInfo()
    {
        int pid = int.Parse(customInput.text);
        var existing = DatabaseManager.Instance.GetPlayerByPlayerId(pid);

        PlayerRecord rec;
        if (existing != null)
        {
            rec = existing;
            rec.Name = nameInput.text;
            rec.Age = int.Parse(ageInput.text);
        }
        else
        {
            rec = new PlayerRecord
            {
                PlayerId = pid,
                Name = nameInput.text,
                Age = int.Parse(ageInput.text)
            };
        }

        DatabaseManager.Instance.UpsertPlayer(rec);

        // Memóriába is mentsd, hogy a játék tudja, kivel megy tovább:
        DataManager.Instance.playerData.playerId = rec.PlayerId;
        DataManager.Instance.playerData.playerName = rec.Name;
        DataManager.Instance.playerData.playerAge = rec.Age;

        // És indítsd a játékot...
    }
    /// <summary>
    /// Ezt a gombnyomás‐callbacket hívd meg, amikor a játékos rányom a „Játék indítása” gombra.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("MainGame scene called.");

        // Stopper indítása, ha van
        var stopper = FindObjectOfType<Stopper>();
        if (stopper != null)
            stopper.StartStopper();
        else
            Debug.LogWarning("Stopper nem található!");

        // Következő jelenet
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
