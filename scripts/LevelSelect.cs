using Godot;

namespace Wordgrove;

/// <summary>
/// Home screen: shows streak, coin balance, and a big PLAY button
/// that continues the infinite ladder at the player's current level.
/// </summary>
public partial class LevelSelect : Control
{
    private Label _titleLabel;
    private Label _streakLabel;
    private Label _coinLabel;
    private Label _levelLabel;
    private Label _bonusLabel;
    private Button _playButton;
    private Label _rewardToast;

    public override void _Ready()
    {
        _titleLabel = GetNode<Label>("VBox/TitleLabel");
        _streakLabel = GetNode<Label>("VBox/StreakLabel");
        _coinLabel = GetNode<Label>("VBox/CoinLabel");
        _levelLabel = GetNode<Label>("VBox/LevelLabel");
        _bonusLabel = GetNode<Label>("VBox/BonusLabel");
        _playButton = GetNode<Button>("VBox/PlayButton");
        _rewardToast = GetNode<Label>("RewardToast");

        _playButton.Pressed += OnPlayPressed;

        // Load progress
        ProgressManager.Load();

        // Update daily streak
        int reward = ProgressManager.UpdateStreak();

        // Show UI
        UpdateDisplay();

        // Show milestone reward if any
        if (reward > 0)
        {
            _rewardToast.Text = $"🎉 Streak milestone! +{reward} coins!";
            _rewardToast.Visible = true;

            var tween = CreateTween();
            tween.TweenInterval(3.0);
            tween.TweenCallback(Callable.From(() =>
            {
                _rewardToast.Visible = false;
            }));
        }
        else
        {
            _rewardToast.Visible = false;
        }
    }

    private void UpdateDisplay()
    {
        _titleLabel.Text = "WORDGROVE";
        _titleLabel.AddThemeFontSizeOverride("font_size", 72);
        _titleLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.76f, 0.22f));

        _streakLabel.Text = ProgressManager.CurrentStreak > 0
            ? $"🔥 Streak: {ProgressManager.CurrentStreak} days"
            : "Start your streak today!";
        _streakLabel.AddThemeFontSizeOverride("font_size", 32);
        _streakLabel.AddThemeColorOverride("font_color", new Color(1f, 0.6f, 0.2f));

        _coinLabel.Text = $"🪙 {ProgressManager.Coins} coins";
        _coinLabel.AddThemeFontSizeOverride("font_size", 28);
        _coinLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));

        _levelLabel.Text = $"Level {ProgressManager.CurrentLevel}";
        _levelLabel.AddThemeFontSizeOverride("font_size", 28);
        _levelLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));

        _bonusLabel.Text = ProgressManager.TotalBonusWords > 0
            ? $"Total bonus words: {ProgressManager.TotalBonusWords}"
            : "";
        _bonusLabel.AddThemeFontSizeOverride("font_size", 22);
        _bonusLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.8f));

        _playButton.Text = "▶ PLAY";
        _playButton.AddThemeFontSizeOverride("font_size", 48);
        _playButton.CustomMinimumSize = new Vector2(400, 120);
    }

    private void OnPlayPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }
}
