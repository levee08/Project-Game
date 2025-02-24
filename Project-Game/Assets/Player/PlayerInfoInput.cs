using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerInfoInput : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField ageInput;
    public TMP_InputField customInput;

    public void SavePlayerInfo()
    {
        DataManager.Instance.playerData.PlayerName = nameInput.text;
        DataManager.Instance.playerData.PlayerAge = int.Parse(ageInput.text);
        DataManager.Instance.playerData.CustomID = int.Parse(customInput.text);

        Debug.Log("Player Info Saved: " + DataManager.Instance.playerData.PlayerName + ", " + DataManager.Instance.playerData.PlayerAge + ", " + DataManager.Instance.playerData.CustomID);
    }
    public void StartGame()
    {
        Debug.Log("MainGame scene called.");

        // Stopper indítása
        Stopper stopper = FindObjectOfType<Stopper>();
        if (stopper != null)
        {
            stopper.StartStopper();
        }
        else
        {
            Debug.LogError("Stopper nem található!");
        }

        // Jelenet betöltése
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}


    // Adatok mentése az adatbázisba
    //public void SaveToDatabase()
    //{
    //    Debug.Log("SaveToDatabase called!");
    //    StartCoroutine(SendDataToDatabase());
    //}

    //private IEnumerator SendDataToDatabase()
    //{
    //    // Input mezők adatai
    //    string playerName = nameInput.text;
    //    string playerAge = ageInput.text;
    //    string customID = customInput.text;

    //    // POST kérés előkészítése
    //    WWWForm form = new WWWForm();
    //    form.AddField("name", playerName);
    //    form.AddField("age", playerAge);
    //    form.AddField("custom_id", customID);

    //    // HTTP kérés küldése a szerverre
    //    using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/ProjectGame/InsertPlayer.php", form))
    //    {
    //        yield return www.SendWebRequest();

    //        if (www.result != UnityWebRequest.Result.Success)
    //        {
    //            Debug.LogError("Failed to save data to database: " + www.error);
    //        }
    //        else
    //        {
    //            Debug.Log("Player Info Saved to Database: " + playerName + ", " + playerAge + ", " + customID);
    //        }
    //    }
    //}

    //public void StartGame()
    //{
    //    Debug.Log("MainGame scene called.");
    //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    //}

