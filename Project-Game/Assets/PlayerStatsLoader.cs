using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerStatsLoader : MonoBehaviour
{
    public GameObject playerStatsContent;
    public GameObject rowPrefab; 

    void Start()
    {
        StartCoroutine(LoadPlayerStats());
    }

    IEnumerator LoadPlayerStats()
    {
        UnityWebRequest playersDataRequest = UnityWebRequest.Get("http://localhost/ProjectGame/playerstats.php");
        yield return playersDataRequest.SendWebRequest();

        if (playersDataRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load player stats: " + playersDataRequest.error);
        }
        else
        {
            string playersDataString = playersDataRequest.downloadHandler.text;
            Debug.Log("Received Data: " + playersDataString);
            DisplayPlayerStats(playersDataString);
        }
    }

    void DisplayPlayerStats(string data)
    {
        Debug.Log("Displaying Player Stats");
        string[] players = data.Split(';');
        foreach (string player in players)
        {
            if (!string.IsNullOrEmpty(player))
            {
                string[] playerInfo = player.Split('|');
                string playerName = string.Empty;
                string playerScore = string.Empty;

                foreach (string info in playerInfo)
                {
                    if (info.StartsWith("Név:"))
                    {
                        playerName = info.Substring(info.IndexOf("Név:") + "Név:".Length).Trim();
                    }
                    else if (info.StartsWith("Eredmény:"))
                    {
                        playerScore = info.Substring(info.IndexOf("Eredmény:") + "Eredmény:".Length).Trim();
                    }
                }

                Debug.Log("Player Name: " + playerName + ", Player Score: " + playerScore);

                if (!string.IsNullOrEmpty(playerName) && !string.IsNullOrEmpty(playerScore))
                {
                    GameObject newRow = Instantiate(rowPrefab, playerStatsContent.transform);
                    Debug.Log("Instantiated new row prefab: " + newRow.name);
                    newRow.SetActive(true); // Győződj meg róla, hogy az új prefab aktív
                    Text[] texts = newRow.GetComponentsInChildren<Text>();
                    Debug.Log("Found " + texts.Length + " Text components in new row");
                    foreach (Text text in texts)
                    {
                        Debug.Log("Found Text Component: " + text.gameObject.name);
                        if (text.gameObject.name == "Név")
                        {
                            text.text = playerName;
                            Debug.Log("Set Name Text: " + playerName + ", Component Text: " + text.text);
                        }
                        else if (text.gameObject.name == "Pontszám")
                        {
                            text.text = playerScore;
                            Debug.Log("Set Score Text: " + playerScore + ", Component Text: " + text.text);
                        }
                    }
                }
            }
        }
    }
}
