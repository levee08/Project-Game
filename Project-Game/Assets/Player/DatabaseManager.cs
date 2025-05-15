using System;
using System.IO;
using System.Linq;
using SQLite4Unity3d;
using UnityEngine;

/// <summary>
/// Singleton, amely a játékos-adatokat SQLite-ben tárolja.
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }
    SQLiteConnection _db;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Az adatbázis elérési útja
        var path = Path.Combine(Application.persistentDataPath, "game.db");
        _db = new SQLiteConnection(path);

        // Táblák létrehozása, ha még nem léteznek
        _db.CreateTable<PlayerRecord>();
        _db.CreateTable<GameResultRecord>();
    }

    /// <summary>
    /// Beszúrja vagy frissíti a PlayerRecordot.
    /// Ha már van ilyen PlayerId, UPDATE, különben INSERT.
    /// </summary>
    public void UpsertPlayer(PlayerRecord p)
    {
        // Megnézzük, létezik-e már rekord ezzel a PlayerId-vel
        var existing = GetPlayerByPlayerId(p.PlayerId);
        if (existing == null)
        {
            // INSERT az AutoIncrement kulcs most p.Id-ba kerül
            p.Id = _db.Insert(p);
        }
        else
        {
            // UPDATE örököljük az adatbázisbeli Id-t
            p.Id = existing.Id;
            _db.Update(p);
        }
    }

    /// <summary>
    /// Visszaadja a PlayerRecordot a kapott playerId alapján, vagy null-t, ha nincs ilyen.
    /// </summary>
    public PlayerRecord GetPlayerByPlayerId(int playerId)
    {
        return _db.Table<PlayerRecord>()
                  .FirstOrDefault(x => x.PlayerId == playerId);
    }

    public GameResultRecord[] GetResultsForPlayerRecordId(int playerRecordId)
    {
        return _db.Table<GameResultRecord>()
                  .Where(x => x.PlayerRecordId == playerRecordId)
                  .OrderByDescending(x => x.PlayedAt)
                  .ToArray();
    }

    /// <summary>
    /// Az összes játékos rekordot visszaadja tömbben.
    /// </summary>
    public PlayerRecord[] GetAllPlayers()
    {
        return _db.Table<PlayerRecord>().ToArray();
    }

    /// <summary>
    /// Új GameResultRecord beszúrása (minden egyes játék után).
    /// </summary>
    public void SaveGameResult(GameResultRecord r)
    {
        _db.Insert(r);
    }

    /// <summary>
    /// Egy adott játékoshoz tartozó eredmények lekérdezése,
    /// dátum szerint csökkenő sorrendben.
    /// </summary>
    public GameResultRecord[] GetResultsForPlayer(int playerRecordId)
    {
        return _db.Table<GameResultRecord>()
                  .Where(x => x.PlayerRecordId == playerRecordId)
                  .OrderByDescending(x => x.PlayedAt)
                  .ToArray();
    }
}

/// <summary>
/// A PlayerRecord osztály, amelyet SQLite-ben tárolunk.
/// </summary>
[Table("Players")]
public class PlayerRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed(Unique = true)]
    public int PlayerId { get; set; }

    public string Name { get; set; }
    public int Age { get; set; }

    public int AttentionScore { get; set; }
    public int LogicScore { get; set; }
    public int VisualScore { get; set; }
    public int ProblemSolvingScore { get; set; }

    public float TotalScore { get; set; }
    public DateTime LastPlayed { get; set; }
}
[Table("GameResults")]
    public class GameResultRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int PlayerRecordId { get; set; }  // FK a Players.Id

        public string GameName { get; set; }
        public double Score { get; set; }
        public int Mistakes { get; set; }
        public double Duration { get; set; }
        public string Difficulty { get; set; }
        public DateTime PlayedAt { get; set; }
    }
