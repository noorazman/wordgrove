using System.Collections.Generic;

namespace Wordgrove;

/// <summary>
/// Runtime manager that tracks solve times for difficulty rhythm
/// and acts as a central coordinator for level progression.
/// </summary>
public static class GameManager
{
    // Circular buffer of recent solve times (seconds)
    private static readonly List<double> SolveTimes = new();

    /// <summary>
    /// Record the time the player took to solve a level.
    /// </summary>
    public static void RecordSolveTime(double seconds)
    {
        SolveTimes.Add(seconds);
        // Keep only the last 10 entries
        while (SolveTimes.Count > 10)
            SolveTimes.RemoveAt(0);
    }

    /// <summary>
    /// Returns the most recent solve time, or -1 if none recorded.
    /// Used by LevelGenerator for adaptive difficulty.
    /// </summary>
    public static double LastSolveTime()
    {
        if (SolveTimes.Count == 0) return -1;
        return SolveTimes[^1];
    }

    /// <summary>
    /// Clear all recorded times (e.g. on fresh start).
    /// </summary>
    public static void Reset()
    {
        SolveTimes.Clear();
    }
}
