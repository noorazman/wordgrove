using Godot;
using System;
using System.Text.Json;

namespace Wordgrove;

/// <summary>
/// Persists and manages player progress: current level, coins,
/// daily streak, and bonus-word stats.
///
/// File: user://progress.json
/// </summary>
public static class ProgressManager
{
    private const string SavePath = "user://progress.json";

    // ── In-memory state ────────────────────────────────────
    public static int CurrentLevel { get; set; } = 1;
    public static int Coins { get; set; } = 0;
    public static int CurrentStreak { get; set; } = 0;
    public static string LastPlayedDate { get; set; } = "";
    public static int TotalBonusWords { get; set; } = 0;

    // ── Streak milestones ──────────────────────────────────
    private static readonly (int Day, int Reward)[] Milestones =
    {
        (3, 50),
        (7, 100),
        (30, 500)
    };

    /// <summary>
    /// Load progress from disk. Safe on missing / corrupt files.
    /// </summary>
    public static void Load()
    {
        try
        {
            if (!FileAccess.FileExists(SavePath))
            {
                GD.Print("ProgressManager: No save file, using defaults.");
                ResetDefaults();
                return;
            }

            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"ProgressManager: Cannot open {SavePath}");
                ResetDefaults();
                return;
            }

            string json = file.GetAsText();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            CurrentLevel = GetInt(root, "currentLevel", 1);
            Coins = GetInt(root, "coins", 0);
            CurrentStreak = GetInt(root, "currentStreak", 0);
            LastPlayedDate = GetString(root, "lastPlayedDate", "");
            TotalBonusWords = GetInt(root, "totalBonusWords", 0);

            GD.Print($"ProgressManager: Loaded — Level {CurrentLevel}, " +
                     $"Coins {Coins}, Streak {CurrentStreak}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"ProgressManager: Load error — {ex.Message}");
            ResetDefaults();
        }
    }

    /// <summary>
    /// Save current state to disk.
    /// </summary>
    public static void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(new
            {
                currentLevel = CurrentLevel,
                coins = Coins,
                currentStreak = CurrentStreak,
                lastPlayedDate = LastPlayedDate,
                totalBonusWords = TotalBonusWords
            }, new JsonSerializerOptions { WriteIndented = true });

            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                GD.PrintErr($"ProgressManager: Cannot write {SavePath}");
                return;
            }
            file.StoreString(json);
            GD.Print("ProgressManager: Saved.");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"ProgressManager: Save error — {ex.Message}");
        }
    }

    /// <summary>
    /// Call on game launch to update the daily streak.
    /// Returns the coin reward earned from milestones (0 if none).
    /// </summary>
    public static int UpdateStreak()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        if (LastPlayedDate == today)
        {
            // Already played today — streak continues, no reward
            return 0;
        }

        if (IsYesterday(LastPlayedDate))
        {
            CurrentStreak++;
        }
        else
        {
            // Streak broken or first ever play
            CurrentStreak = 1;
        }

        LastPlayedDate = today;

        // Check milestones
        int reward = 0;
        foreach (var (day, coins) in Milestones)
        {
            if (CurrentStreak == day)
            {
                reward = coins;
                break;
            }
        }

        if (reward > 0)
        {
            Coins += reward;
            GD.Print($"ProgressManager: Streak milestone day {CurrentStreak}! +{reward} coins");
        }

        Save();
        return reward;
    }

    public static void AddCoins(int amount)
    {
        Coins += amount;
        Save();
    }

    public static bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        Save();
        return true;
    }

    public static void AdvanceLevel()
    {
        CurrentLevel++;
        Save();
    }

    public static void RecordBonusWord()
    {
        TotalBonusWords++;
        Save();
    }

    // ── Helpers ─────────────────────────────────────────────
    private static void ResetDefaults()
    {
        CurrentLevel = 1;
        Coins = 0;
        CurrentStreak = 0;
        LastPlayedDate = "";
        TotalBonusWords = 0;
    }

    private static bool IsYesterday(string dateStr)
    {
        if (string.IsNullOrEmpty(dateStr)) return false;
        if (!DateTime.TryParse(dateStr, out var parsed)) return false;

        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        return parsed.Date == yesterday;
    }

    private static int GetInt(JsonElement el, string prop, int fallback)
    {
        if (el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number)
            return v.GetInt32();
        return fallback;
    }

    private static string GetString(JsonElement el, string prop, string fallback)
    {
        if (el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String)
            return v.GetString() ?? fallback;
        return fallback;
    }
}
