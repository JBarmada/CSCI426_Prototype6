using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class HighScoreEntry
{
    public int score;
    public string date;
}

[Serializable]
public class HighScoreData
{
    public List<HighScoreEntry> entries = new List<HighScoreEntry>();
}

public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager Instance { get; private set; }

    private const string FileName = "highscores.json";

    private string FilePath => Path.Combine(Application.persistentDataPath, FileName);
    private HighScoreData data = new HighScoreData();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFromDisk();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Saves a new score entry to disk with the current date.</summary>
    public void SaveScore(int score)
    {
        var entry = new HighScoreEntry
        {
            score = score,
            date = DateTime.Now.ToString("yyyy-MM-dd")
        };

        data.entries.Add(entry);
        data.entries.Sort((a, b) => b.score.CompareTo(a.score));
        WriteToDisk();
    }

    /// <summary>Returns up to <paramref name="count"/> top entries sorted descending by score.</summary>
    public List<HighScoreEntry> GetTopScores(int count = 5)
    {
        int take = Mathf.Min(count, data.entries.Count);
        return data.entries.GetRange(0, take);
    }

    /// <summary>Deletes all saved high scores from memory and disk.</summary>
    public void ClearScores()
    {
        data.entries.Clear();
        WriteToDisk();
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    private void LoadFromDisk()
    {
        if (!File.Exists(FilePath))
        {
            data = new HighScoreData();
            return;
        }

        try
        {
            string json = File.ReadAllText(FilePath);
            data = JsonUtility.FromJson<HighScoreData>(json) ?? new HighScoreData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[HighScoreManager] Failed to load scores: {e.Message}");
            data = new HighScoreData();
        }
    }

    private void WriteToDisk()
    {
        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[HighScoreManager] Failed to save scores: {e.Message}");
        }
    }
}
