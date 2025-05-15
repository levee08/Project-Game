using CognitiveGames.Scoring;
using System;
using System.Collections.Generic;
using static CognitiveGames.Scoring.ScoreManager;

[Serializable]
public class PlayerData
{
    public int playerId;             // Egyedi azonosító (ha van)
    public string playerName;
    public int playerAge;
    public SkillScores skills = new SkillScores();

    public HashSet<string> finishedMiniGames = new HashSet<string>();
    // Kognitív képességek pontszámai
    public int attentionScore;
    public int logicScore;
    public int visualScore;
    public int problemSolvingScore;

    public float totalScore;

    // Játékonkénti eredmények
    public List<GameResult> gameResults = new List<GameResult>();

    public DateTime lastPlayed; // Mikor játszott utoljára

    public void RefreshTotal() => totalScore = skills.Total;
    public void CalculateTotalScore()
    {
        totalScore = (attentionScore + logicScore + visualScore + problemSolvingScore) / 4f;
    }
}

[Serializable]
public class GameResult
{
    public string gameName;   // pl. "Double Decision"
    public int score;         // elért pontszám (0-100)
    public float timeTaken;   // másodpercben eltelt idő
    public int mistakes;      // hibák száma (ha van)

    public DateTime playedAt; // játszott időpont
}
