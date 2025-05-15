// Assets/Scripts/LeaderboardManager.cs
using UnityEngine;       // ha ebbe a namespace-be tetted a PlayerRecord-et
using System.Linq;

public class LeaderboardManager : MonoBehaviour
{
    public GameObject rowPrefab;
    public Transform contentParent;

    void Start()
    {
        var players = DatabaseManager.Instance.GetAllPlayers();
        foreach (var p in players)
        {
            var go = Instantiate(rowPrefab, contentParent);
            var handler = go.GetComponent<RowClickHandler>();
            handler.Initialize(p.Name, p.TotalScore.ToString(), p.PlayerId.ToString());
        }
    }
}
