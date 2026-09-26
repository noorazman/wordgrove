using System;
using System.Collections.Generic;
using System.Linq;

namespace Wordgrove;

/// <summary>
/// Data returned by <see cref="LevelGenerator.Generate"/>.
/// </summary>
public sealed class LevelData
{
    public string Theme { get; set; } = "";
    public string[] Letters { get; set; } = Array.Empty<string>();
    public string[] TargetWords { get; set; } = Array.Empty<string>();
    public string[] BonusWords { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Procedural level generator with scaling, theme rotation,
/// difficulty rhythm, and solvability guarantees.
/// </summary>
public static class LevelGenerator
{
    private static readonly Random Rng = new();

    // The last theme index used — ensures no two consecutive themes.
    private static int _lastThemeIndex = -1;

    // ── Difficulty rhythm state ────────────────────────────
    // Tracks lettersInPool of the last 3 levels for rhythm enforcement.
    private static readonly List<int> RecentLetterCounts = new();

    // Known-good fallback level (always solvable)
    private static readonly LevelData FallbackLevel = new()
    {
        Theme = "ANIMALS",
        Letters = new[] { "C", "A", "T" },
        TargetWords = new[] { "CAT" },
        BonusWords = Array.Empty<string>()
    };

    /// <summary>
    /// Generate a level for the given 1-indexed level number.
    /// Optionally pass the previous level's solve time in seconds
    /// for adaptive difficulty.
    /// </summary>
    public static LevelData Generate(int levelNumber, double previousSolveTime = -1)
    {
        // ── 1. Compute base scaling parameters ─────────────
        int lettersInPool = ComputeLetterCount(levelNumber);
        int targetWordCount = ComputeTargetWordCount(levelNumber);
        int minWordLength = ComputeMinWordLength(levelNumber);
        int maxWordLength = minWordLength + 2;

        // ── 2. Difficulty rhythm adjustments ───────────────
        ApplyRhythm(ref lettersInPool, ref targetWordCount, previousSolveTime);

        // Clamp after rhythm adjustments
        lettersInPool = Math.Clamp(lettersInPool, 3, 12);
        targetWordCount = Math.Clamp(targetWordCount, 1, 6);
        minWordLength = Math.Clamp(minWordLength, 3, 6);
        maxWordLength = Math.Clamp(minWordLength + 2, minWordLength, 8);

        // ── 3. Pick a theme (no repeats) ───────────────────
        string theme = PickTheme();

        // ── 4. Try to generate a solvable level ────────────
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var level = TryGenerate(theme, lettersInPool, targetWordCount,
                                    minWordLength, maxWordLength);
            if (level != null)
            {
                // Record for rhythm tracking
                RecordLetterCount(lettersInPool);
                return level;
            }
        }

        // ── 5. Fallback: guaranteed simple level ───────────
        RecordLetterCount(3);
        return new LevelData
        {
            Theme = FallbackLevel.Theme,
            Letters = (string[])FallbackLevel.Letters.Clone(),
            TargetWords = (string[])FallbackLevel.TargetWords.Clone(),
            BonusWords = Array.Empty<string>()
        };
    }

    // ──────────────────────────────────────────────────────────
    // Scaling formulas
    // ──────────────────────────────────────────────────────────
    private static int ComputeLetterCount(int level)
    {
        if (level <= 10) return Rng.Next(3, 5);       // 3-4
        if (level >= 101) return Rng.Next(10, 13);     // 10-12
        return Math.Clamp(3 + level / 5, 3, 12);
    }

    private static int ComputeTargetWordCount(int level)
    {
        if (level <= 10) return Rng.Next(1, 3);        // 1-2
        if (level >= 101) return Rng.Next(5, 7);       // 5-6
        return Math.Clamp(1 + level / 15, 1, 6);
    }

    private static int ComputeMinWordLength(int level)
    {
        if (level <= 10) return 3;
        if (level >= 101) return Rng.Next(6, 7);       // 6
        return Math.Clamp(3 + level / 40, 3, 6);
    }

    // ──────────────────────────────────────────────────────────
    // Difficulty rhythm
    // ──────────────────────────────────────────────────────────
    private static void ApplyRhythm(ref int lettersInPool, ref int targetWordCount,
                                     double previousSolveTime)
    {
        // Rule 1: if last 3 levels had the same lettersInPool, force ±1
        if (RecentLetterCounts.Count >= 3)
        {
            var last3 = RecentLetterCounts.Skip(RecentLetterCounts.Count - 3).ToList();
            if (last3.All(c => c == last3[0]) && lettersInPool == last3[0])
            {
                lettersInPool += Rng.Next(0, 2) == 0 ? 1 : -1;
            }
        }

        // Rule 2: adaptive based on solve time
        if (previousSolveTime > 60)
        {
            lettersInPool = Math.Max(3, lettersInPool - 1);
            targetWordCount = Math.Max(1, targetWordCount - 1);
        }
        else if (previousSolveTime >= 0 && previousSolveTime < 15)
        {
            lettersInPool = Math.Min(12, lettersInPool + 1);
        }
    }

    private static void RecordLetterCount(int count)
    {
        RecentLetterCounts.Add(count);
        // Keep only the last 5 for memory efficiency
        while (RecentLetterCounts.Count > 5)
            RecentLetterCounts.RemoveAt(0);
    }

    // ──────────────────────────────────────────────────────────
    // Theme rotation
    // ──────────────────────────────────────────────────────────
    private static string PickTheme()
    {
        int count = WordDatabase.ThemeNames.Length;
        int idx;
        do
        {
            idx = Rng.Next(count);
        } while (idx == _lastThemeIndex && count > 1);

        _lastThemeIndex = idx;
        return WordDatabase.ThemeNames[idx];
    }

    // ──────────────────────────────────────────────────────────
    // Core generation with solvability verification
    // ──────────────────────────────────────────────────────────
    private static LevelData TryGenerate(string theme, int letterPoolSize,
                                          int targetWordCount,
                                          int minWordLen, int maxWordLen)
    {
        if (!WordDatabase.ThemeWords.TryGetValue(theme, out var themeWordList))
            return null;

        // Filter words by length
        var candidates = themeWordList
            .Where(w => w.Length >= minWordLen && w.Length <= maxWordLen)
            .ToList();

        if (candidates.Count == 0) return null;

        // Shuffle candidates
        Shuffle(candidates);

        // Try to find a valid target-word set with a shared letter pool
        for (int poolAttempt = 0; poolAttempt < 20; poolAttempt++)
        {
            // Build a letter pool from the first few candidate words
            var selectedWords = new List<string>();
            var poolLetters = new HashSet<char>();

            // Start with one random word and its letters
            var seed = candidates[Rng.Next(candidates.Count)];
            selectedWords.Add(seed);
            foreach (char c in seed) poolLetters.Add(c);

            // Add more words that fit within expanding pool
            foreach (var word in candidates)
            {
                if (selectedWords.Count >= targetWordCount) break;
                if (selectedWords.Contains(word)) continue;

                var wordLetters = new HashSet<char>(word);
                var combined = new HashSet<char>(poolLetters);
                combined.UnionWith(wordLetters);

                if (combined.Count <= letterPoolSize)
                {
                    selectedWords.Add(word);
                    poolLetters = combined;
                }
            }

            if (selectedWords.Count < targetWordCount) continue;

            // Pad pool to requested size with random consonants/vowels
            var pool = new List<char>(poolLetters);
            while (pool.Count < letterPoolSize)
            {
                char extra = GetRandomLetter();
                if (!pool.Contains(extra))
                    pool.Add(extra);
            }

            // ── SOLVABILITY CHECK ──
            // Verify every selected word can be formed from the pool letters
            bool allSolvable = true;
            foreach (var word in selectedWords)
            {
                if (!CanFormWord(word, pool))
                {
                    allSolvable = false;
                    break;
                }
            }

            if (!allSolvable) continue;

            // Shuffle pool order
            var poolArray = pool.Select(c => c.ToString()).ToArray();
            ShuffleArray(poolArray);

            return new LevelData
            {
                Theme = theme,
                Letters = poolArray,
                TargetWords = selectedWords.ToArray(),
                BonusWords = Array.Empty<string>()
            };
        }

        return null;
    }

    /// <summary>
    /// Checks if a word can be formed using letters from the pool
    /// (each pool letter can be used once).
    /// </summary>
    public static bool CanFormWord(string word, IEnumerable<char> pool)
    {
        var available = new List<char>(pool);
        foreach (char c in word.ToUpperInvariant())
        {
            int idx = available.IndexOf(c);
            if (idx < 0) return false;
            available.RemoveAt(idx);
        }
        return true;
    }

    /// <summary>
    /// Checks if a word can be formed using letters (as strings) from the pool.
    /// </summary>
    public static bool CanFormWordFromStrings(string word, string[] pool)
    {
        return CanFormWord(word, pool.Select(s => s.Length > 0 ? s[0] : ' '));
    }

    private static char GetRandomLetter()
    {
        // Weighted towards common letters
        const string common = "AAABCDDEEEFGHIIIJKLMNNOOOPQRRSSSTTUUVWXYZ";
        return common[Rng.Next(common.Length)];
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static void ShuffleArray<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
}
