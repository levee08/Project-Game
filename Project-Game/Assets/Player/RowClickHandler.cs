// RowClickHandler.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gravitons.UI.Modal;
using CognitiveGames.Scoring;
using System.Linq;

public class RowClickHandler : MonoBehaviour
{
    int playerCustomID;
    string playerName;
    string playerScore;

    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text scoreText;

    public void Initialize(string name, string score, string customID)
    {
        playerName = name;
        playerScore = score;
        playerCustomID = int.Parse(customID);

        nameText.text = name;
        scoreText.text = score;

        var btn = GetComponentInChildren<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnRowClicked);
    }

    void OnRowClicked()
    {
        // 1) A rekord lekérése DB-ből
        var record = DatabaseManager.Instance.GetPlayerByPlayerId(playerCustomID);
        if (record == null)
        {
            Debug.LogError($"PlayerRecord not found for playerId={playerCustomID}");
            return;
        }
        // 2) Átmásoljuk in-memory PlayerData-ba
        var pd = DataManager.Instance.playerData;
        pd.playerId = record.PlayerId;
        pd.playerName = record.Name;
        pd.playerAge = record.Age;
        pd.attentionScore = record.AttentionScore;
        pd.logicScore = record.LogicScore;
        pd.visualScore = record.VisualScore;
        pd.problemSolvingScore = record.ProblemSolvingScore;
        pd.totalScore = record.TotalScore;
        pd.lastPlayed = record.LastPlayed;
        // ha voltak korábbi eredmények:
        pd.gameResults.Clear();

        // 3) Frissítjük a ScoreManager referenciáját is
        ScoreManager.Instance.CurrentPlayer = pd;
        var results = DatabaseManager.Instance
                      .GetResultsForPlayerRecordId(record.Id);
        if (results.Length == 0)
        {
            ModalManager.Show("Eredmények", "Ehhez a játékoshoz még nincs mentett eredmény.",
                new[] { new ModalButton { Text = "OK" } });
        }
        else
        {
            // összeállítjuk a szöveget
            var lines = results.Select(r =>
                $"{r.PlayedAt:yyyy.MM.dd} — {r.GameName}: {r.Score} pont, " +
                $"{r.Mistakes} hiba, {r.Duration:F1}s");
            string body = string.Join("\n", lines);

            ModalManager.Show("Játékeredmények", body,
                new[] { new ModalButton { Text = "OK" } });
        }
    }
}
