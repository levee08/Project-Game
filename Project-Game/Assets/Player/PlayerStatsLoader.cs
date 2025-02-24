using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class PlayerStatsLoader : MonoBehaviour
{
    public RectTransform playerStatsContent;
    public RectTransform rowPrefab;

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
            DisplayPlayerStats(playersDataString);
        }
    }

    void DisplayPlayerStats(string data)
    {
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

                if (!string.IsNullOrEmpty(playerName) && !string.IsNullOrEmpty(playerScore))
                {
                    // Sor létrehozása
                    GameObject newRow = Instantiate(rowPrefab.gameObject, playerStatsContent.transform);
                    newRow.SetActive(true);

                    // Szövegek frissítése
                    TextMeshProUGUI nameText = newRow.transform.Find("Canvas/Név").GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI scoreText = newRow.transform.Find("Canvas/Pontszám").GetComponent<TextMeshProUGUI>();

                    if (nameText != null) nameText.text = playerName;
                    if (scoreText != null) scoreText.text = playerScore;

                    // RowClickHandler inicializálása
                    RowClickHandler rowHandler = newRow.GetComponent<RowClickHandler>();
                    if (rowHandler != null)
                    {
                        rowHandler.Initialize(playerName, playerScore, "CustomID");
                    }
                }
            }
        }
    }
}
