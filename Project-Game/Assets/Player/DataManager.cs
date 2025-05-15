using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public PlayerData playerData;                 // az egyetlen profil

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            playerData = new PlayerData();        // új, üres adat
        }
        else Destroy(gameObject);
    }

    /*────  mini-játék státusz  ────*/
    public bool HasFinished(string gameId) =>
        playerData.finishedMiniGames.Contains(gameId);

    public void MarkFinished(string gameId)
    {
        if (!playerData.finishedMiniGames.Contains(gameId))
            playerData.finishedMiniGames.Add(gameId);
        // Nincs fájl- vagy DB-mentés, csak memóriában jegyezzük meg
    }
}
