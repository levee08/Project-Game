using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CognitiveGames.Scoring
{
    public enum Difficulty
    {
        Könnyű = 0,
        Normál = 1,
        Nehéz = 2
    }

    public static class GameSettings
    {
        // ugyanazt az enum-típust használod
        public static Difficulty CurrentDifficulty = Difficulty.Normál;
    }

    [Serializable]                      // ➊  egy játék-nehézséghez tartozó konstansok
    public class DifficultyParams
    {
        public float maxTime = 180f;  // mp
        [Range(0f, 1f)] public float accuracyWeight = 0.8f;
        [Range(0f, 1f)] public float speedWeight = 0.2f;
    }

    // ------------------ változatlan adatmodellek + új mező ------------------

    [Serializable]
    public class GameMetrics
    {
        public string GameName;
        public float Time;
        public int Correct;
        public int Mistakes;
        public Difficulty DifficultyLevel;     // ◀ új
        public float TargetTime;          // opcionális, felülírja a táblázatot

        public GameMetrics(string game, float t, int ok, int err,
                           Difficulty diff, float targetTime = -1f)
        {
            GameName = game;
            Time = t;
            Correct = ok;
            Mistakes = err;
            DifficultyLevel = diff;
            TargetTime = targetTime;       // −1  → “használd a táblázatot”
        }
    }

    // ------------------------  pontozó singleton  ---------------------------

    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public SkillScores PlayerProfile = new SkillScores();
        public PlayerData CurrentPlayer;   // ezt a DataManagerból kapod Awake-ben

        [Serializable]
        class GameDifficultyPair
        {
            public string gameName;
            public Difficulty difficulty;
            public DifficultyParams parameters;
        }
        [SerializeField] List<GameDifficultyPair> settings = new();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Betöltjük a memóriába a PlayerData-t
                CurrentPlayer = DataManager.Instance.playerData;
            }
            else Destroy(gameObject);
        }

        /// <summary>
        /// Ezt hívd meg minden minijáték befejezésekor
        /// </summary>
        public void OnGameFinished(GameMetrics g)
        {
            // ---- meglévő: kiszámolja és alkalmazza a score-okat memória szinten ----
            int score = CalculateScore(g);
            ApplyGameScore(g.GameName, score);

            // --- PlayerRecord előkészítése / frissítése ---
            var pd = DataManager.Instance.playerData;
            var rec = DatabaseManager.Instance.GetPlayerByPlayerId(pd.playerId)
                      ?? new PlayerRecord { PlayerId = pd.playerId };

            rec.Name = pd.playerName;
            rec.Age = pd.playerAge;
            rec.AttentionScore = pd.skills.Attention;
            rec.LogicScore = pd.skills.Logic;
            rec.VisualScore = pd.skills.Visual;
            rec.ProblemSolvingScore = pd.skills.ProblemSolving;
            rec.TotalScore = pd.totalScore;        // itt már lesz értéke!
            rec.LastPlayed = DateTime.Now;

            DatabaseManager.Instance.UpsertPlayer(rec);
            DatabaseManager.Instance.UpsertPlayer(rec);

            // 2) Minden egyes futás eredményét külön táblába mentjük:
            var result = new GameResultRecord
            {
                PlayerRecordId = rec.Id,
                GameName = g.GameName,
                Score = score,
                Mistakes = g.Mistakes,
                Duration = g.Time,
                Difficulty = g.DifficultyLevel.ToString(),
                PlayedAt = DateTime.Now
            };
            DatabaseManager.Instance.SaveGameResult(result);
        }


        int CalculateScore(GameMetrics g) =>
            g.GameName switch
            {
                "Sudoku" => ScoreSudoku(g),
                "DoubleDecision" => ScoreDoubleDecision(g),
                "Nonogram" => ScoreNonogram(g),
                "Puzzle" => ScorePuzzle(g),
                _ => 0
            };

        DifficultyParams GetParams(string game, Difficulty diff)
        {
            var row = settings.FirstOrDefault(x => x.gameName == game && x.difficulty == diff);
            return row != null ? row.parameters : new DifficultyParams();
        }

        int ScoreSudoku(GameMetrics g)
        {
            var p = GetParams(g.GameName, g.DifficultyLevel);
            float maxT = g.TargetTime > 0 ? g.TargetTime : p.maxTime;

            float accuracy = g.Mistakes == 0
                ? 1f
                : Mathf.Clamp01(1f - g.Mistakes / 5f);
            float speed = Mathf.Clamp01(1f - g.Time / maxT);

            float raw = accuracy * p.accuracyWeight + speed * p.speedWeight;
            return Mathf.RoundToInt(raw * 100f);
        }

        int ScoreDoubleDecision(GameMetrics g)
        {
            const float targetTotalTime = 30f;
            const int maxRoundsToZero = 10;

            int totalRounds = g.Correct + g.Mistakes;
            float accuracy = totalRounds == 0
                ? 0
                : g.Correct / (float)totalRounds;
            accuracy *= 1f - Mathf.Clamp01((totalRounds - g.Correct) / (float)maxRoundsToZero);
            float speed = 1f - Mathf.Clamp01(g.Time / targetTotalTime);

            float raw = accuracy * 0.7f + speed * 0.3f;
            return Mathf.RoundToInt(raw * 100f);
        }

        int ScoreNonogram(GameMetrics g)
        {
            const float maxTimeEasy = 240f;
            const float maxTimeHard = 360f;
            const int maxMistakes = 30;

            float maxT = g.DifficultyLevel == Difficulty.Nehéz ? maxTimeHard : maxTimeEasy;
            float accuracy = 1f - Mathf.Clamp01(g.Mistakes / (float)maxMistakes);
            float speed = 1f - Mathf.Clamp01(g.Time / maxT);

            float raw = accuracy * 0.6f + speed * 0.4f;
            return Mathf.RoundToInt(raw * 100f);
        }

        int ScorePuzzle(GameMetrics g)
        {
            const float maxTime = 180f;
            const int maxMistakes = 20;

            float accuracy = 1f - Mathf.Clamp01(g.Mistakes / (float)maxMistakes);
            float speed = 1f - Mathf.Clamp01(g.Time / maxTime);

            float raw = (accuracy + speed) * 0.5f;
            return Mathf.RoundToInt(raw * 100f);
        }

        void ApplyGameScore(string game, int score)
        {
            // 1) az in-memory playerData.skills frissítése
            switch (game)
            {
                case "Sudoku":
                    CurrentPlayer.skills.Logic = Mathf.Max(CurrentPlayer.skills.Logic, score);
                    break;
                case "DoubleDecision":
                    CurrentPlayer.skills.Attention = Mathf.Max(CurrentPlayer.skills.Attention, score);
                    break;
                case "Nonogram":
                    CurrentPlayer.skills.Visual = Mathf.Max(CurrentPlayer.skills.Visual, score);
                    break;
                case "Puzzle":
                    CurrentPlayer.skills.ProblemSolving = Mathf.Max(CurrentPlayer.skills.ProblemSolving, score);
                    break;
            }

            // 2) totalScore újraszámolása a playerData-ban
            CurrentPlayer.RefreshTotal();
        }
        /// <summary>
        /// 0–100 skálán mért kognitív részpontok és egy
        /// kiszámított összesített átlag.
        /// </summary>
        [Serializable]                     // kell, hogy Unity Inspector-ban látszódjon
        public class SkillScores
        {
            [Range(0, 100)] public int Attention;       // Double Decision
            [Range(0, 100)] public int Logic;           // Sudoku
            [Range(0, 100)] public int Visual;          // Nonogram
            [Range(0, 100)] public int ProblemSolving;  // Puzzle

            /// <summary>Összesített pont (egyszerű átlag).</summary>
            public float Total =>
                (Attention + Logic + Visual + ProblemSolving) / 4f;

            /// <summary>Visszaállít mindent 0-ra (pl. új játékosnál).</summary>
            public void Reset()
            {
                Attention = Logic = Visual = ProblemSolving = 0;
            }
        }

    }
}
