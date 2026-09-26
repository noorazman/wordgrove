using Godot;
using System;

namespace Wordgrove;

/// <summary>
/// Stub ad manager. Replace the body of ShowRewardedAd with your
/// real ad-network SDK calls (AdMob, Unity Ads, etc.).
/// </summary>
public static class AdManager
{
    /// <summary>
    /// Show a rewarded ad.  On success call <paramref name="onReward"/>;
    /// on failure (no fill, user cancel, SDK error) call <paramref name="onFail"/>.
    /// </summary>
    public static void ShowRewardedAd(Action onReward, Action onFail)
    {
        // ── STUB ──────────────────────────────────────────────
        // In production, replace this with actual ad SDK integration.
        // For now we simulate "no ad available" so the toast path
        // is exercised during development.
        GD.Print("AdManager: ShowRewardedAd called (stub — simulating failure).");
        onFail?.Invoke();
    }
}
