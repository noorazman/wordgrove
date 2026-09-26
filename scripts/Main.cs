using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wordgrove;

public partial class Main : Control
{
    // ── Level state (generated at runtime) ──────────────────
    private LevelData _levelData;
    private int _currentLevelNumber;

    // ── Node references ────────────────────────────────────
    private Label _themeLabel;
    private Label _coinLabel;
    private VBoxContainer _gridArea;
    private LetterWheel _letterWheel;
    private ColorRect _completionOverlay;
    private Label _completeLabel;

    // ── Power-up buttons ───────────────────────────────────
    private Button _hintButton;
    private Button _shuffleButton;
    private Button _revealButton;

    // ── Toast label for bonus/insufficient coins ───────────
    private Label _toastLabel;

    // ── State ──────────────────────────────────────────────
    private readonly Dictionary<string, List<Label>> _wordSlots = new();
    private readonly HashSet<string> _foundWords = new();
    private readonly List<string> _foundBonusWords = new();
    private double _levelStartTime;
    private bool _levelActive;

    // ── Styling constants ──────────────────────────────────
    private const int SlotSize = 64;
    private const int SlotGap = 8;
    private const int RowGap = 12;
    private static readonly Color SlotEmpty = new(0.22f, 0.28f, 0.34f);
    private static readonly Color SlotFilled = new(0.18f, 0.56f, 0.34f);
    private static readonly Color SlotText = new(1f, 1f, 1f);
    private static readonly Color ThemeColor = new(0.95f, 0.76f, 0.22f);
    private static readonly Color BonusFlashColor = new(0.55f, 0.27f, 0.95f);

    // ── Power-up costs ─────────────────────────────────────
    private const int HintCost = 10;
    private const int ShuffleCost = 5;
    private const int RevealCost = 30;

    // ── Coin awards ────────────────────────────────────────
    private const int BonusWordCoins = 1;
    private const int LevelCompleteCoins = 5;

    public override void _Ready()
    {
        _themeLabel = GetNode<Label>("TopBar/ThemeLabel");
        _coinLabel = GetNode<Label>("TopBar/CoinLabel");
        _gridArea = GetNode<VBoxContainer>("GridArea");
        _letterWheel = GetNode<LetterWheel>("WheelArea/LetterWheel");
        _completionOverlay = GetNode<ColorRect>("CompletionOverlay");
        _completeLabel = _completionOverlay.GetNode<Label>("VBox/CompleteLabel");

        // Power-up bar
        _hintButton = GetNode<Button>("PowerUpBar/HintButton");
        _shuffleButton = GetNode<Button>("PowerUpBar/ShuffleButton");
        _revealButton = GetNode<Button>("PowerUpBar/RevealButton");

        // Toast
        _toastLabel = GetNode<Label>("ToastLabel");
        _toastLabel.Visible = false;

        // Connect signals
        _letterWheel.WordSubmitted += OnWordSubmitted;

        // Completion overlay buttons
        var nextBtn = _completionOverlay.GetNode<Button>("VBox/ButtonRow/NextButton");
        var menuBtn = _completionOverlay.GetNode<Button>("VBox/ButtonRow/MenuButton");
        nextBtn.Pressed += OnNextPressed;
        menuBtn.Pressed += OnMenuPressed;

        // Power-up signals
        _hintButton.Pressed += OnHintPressed;
        _shuffleButton.Pressed += OnShufflePressed;
        _revealButton.Pressed += OnRevealPressed;

        // Load progress
        ProgressManager.Load();

        // Start at the player's current level
        _currentLevelNumber = ProgressManager.CurrentLevel;
        StartLevel(_currentLevelNumber);
    }

    public override void _Process(double delta)
    {
        // Update power-up button states based on coin balance
        UpdatePowerUpButtons();
    }

    // ──────────────────────────────────────────────────────────
    // Level lifecycle
    // ──────────────────────────────────────────────────────────
    private void StartLevel(int levelNumber)
    {
        _currentLevelNumber = levelNumber;

        // Generate the level using adaptive difficulty
        double prevTime = GameManager.LastSolveTime();
        _levelData = LevelGenerator.Generate(levelNumber, prevTime);

        BuildLevel();
    }

    private void BuildLevel()
    {
        // Header
        _themeLabel.Text = $"Level {_currentLevelNumber} — {_levelData.Theme}";
        _themeLabel.AddThemeColorOverride("font_color", ThemeColor);
        _themeLabel.AddThemeFontSizeOverride("font_size", 36);

        // Coins
        UpdateCoinDisplay();

        // Completion label
        _completeLabel.AddThemeFontSizeOverride("font_size", 48);
        _completeLabel.AddThemeColorOverride("font_color", ThemeColor);

        BuildGrid();

        // Set up the wheel with our letter pool (as chars)
        char[] letters = _levelData.Letters.Select(s => s.Length > 0 ? s[0] : 'A').ToArray();
        _letterWheel.Setup(letters);

        _completionOverlay.Visible = false;
        _foundWords.Clear();
        _foundBonusWords.Clear();
        _levelStartTime = Time.GetUnixTimeFromSystem();
        _levelActive = true;
    }

    // ──────────────────────────────────────────────────────────
    // Grid
    // ──────────────────────────────────────────────────────────
    private void BuildGrid()
    {
        foreach (var child in _gridArea.GetChildren())
            child.QueueFree();
        _wordSlots.Clear();

        _gridArea.AddThemeConstantOverride("separation", RowGap);

        foreach (var word in _levelData.TargetWords)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", SlotGap);
            row.Alignment = BoxContainer.AlignmentMode.Center;

            var slots = new List<Label>();
            foreach (var ch in word)
            {
                var slot = CreateSlot();
                row.AddChild(slot);
                slots.Add(slot);
            }

            _gridArea.AddChild(row);
            _wordSlots[word] = slots;
        }
    }

    private Label CreateSlot()
    {
        var label = new Label
        {
            CustomMinimumSize = new Vector2(SlotSize, SlotSize),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = ""
        };
        label.AddThemeFontSizeOverride("font_size", 36);
        label.AddThemeColorOverride("font_color", SlotText);

        var style = new StyleBoxFlat
        {
            BgColor = SlotEmpty,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8
        };
        label.AddThemeStyleboxOverride("normal", style);
        return label;
    }

    // ──────────────────────────────────────────────────────────
    // Word submission handling
    // ──────────────────────────────────────────────────────────
    private void OnWordSubmitted(string word)
    {
        if (!_levelActive) return;

        word = word.ToUpperInvariant();

        // Already found as target?
        if (_foundWords.Contains(word))
        {
            FlashRow(word, new Color(0.4f, 0.6f, 0.8f));
            return;
        }

        // Already found as bonus?
        if (_foundBonusWords.Contains(word))
        {
            ShowToast($"Already found: {word}");
            return;
        }

        // Is it a target word?
        if (_wordSlots.TryGetValue(word, out var slots))
        {
            _foundWords.Add(word);
            FillSlots(word, slots);
            CheckCompletion();
            return;
        }

        // Is it a valid bonus word?
        if (word.Length >= 3
            && LevelGenerator.CanFormWordFromStrings(word, _levelData.Letters)
            && WordDatabase.IsValidBonusWord(word))
        {
            _foundBonusWords.Add(word);
            ProgressManager.AddCoins(BonusWordCoins);
            ProgressManager.RecordBonusWord();
            UpdateCoinDisplay();

            // Bonus flash on the wheel area
            ShowToast($"Bonus: {word} (+{BonusWordCoins} coin)");
            return;
        }

        // Invalid — wheel handles its own shake
    }

    private void FillSlots(string word, List<Label> slots)
    {
        for (int i = 0; i < word.Length; i++)
        {
            slots[i].Text = word[i].ToString();
            var style = (StyleBoxFlat)slots[i].GetThemeStylebox("normal").Duplicate();
            style.BgColor = SlotFilled;
            slots[i].AddThemeStyleboxOverride("normal", style);
        }
    }

    private void FlashRow(string word, Color flashColor)
    {
        if (!_wordSlots.TryGetValue(word, out var slots)) return;

        foreach (var slot in slots)
        {
            var style = (StyleBoxFlat)slot.GetThemeStylebox("normal").Duplicate();
            var original = style.BgColor;
            style.BgColor = flashColor;
            slot.AddThemeStyleboxOverride("normal", style);

            var tween = CreateTween();
            tween.TweenInterval(0.25);
            tween.TweenCallback(Callable.From(() =>
            {
                var s = (StyleBoxFlat)slot.GetThemeStylebox("normal").Duplicate();
                s.BgColor = original;
                slot.AddThemeStyleboxOverride("normal", s);
            }));
        }
    }

    // ──────────────────────────────────────────────────────────
    // Level completion
    // ──────────────────────────────────────────────────────────
    private void CheckCompletion()
    {
        if (_foundWords.Count < _levelData.TargetWords.Length) return;

        _levelActive = false;
        _letterWheel.SetEnabled(false);

        // Record solve time
        double solveTime = Time.GetUnixTimeFromSystem() - _levelStartTime;
        GameManager.RecordSolveTime(solveTime);

        // Award coins
        ProgressManager.AddCoins(LevelCompleteCoins);
        UpdateCoinDisplay();

        // Advance to the next level
        ProgressManager.CurrentLevel = _currentLevelNumber + 1;
        ProgressManager.Save();

        // Show completion overlay
        int totalCoins = LevelCompleteCoins + (_foundBonusWords.Count * BonusWordCoins);
        _completeLabel.Text = $"Level {_currentLevelNumber} Complete!\n" +
                              $"+{LevelCompleteCoins} coins" +
                              (_foundBonusWords.Count > 0
                                  ? $"\n{_foundBonusWords.Count} bonus words found"
                                  : "");
        _completionOverlay.Visible = true;
    }

    // ──────────────────────────────────────────────────────────
    // Completion overlay buttons
    // ──────────────────────────────────────────────────────────
    private void OnNextPressed()
    {
        _completionOverlay.Visible = false;
        _letterWheel.SetEnabled(true);
        StartLevel(_currentLevelNumber + 1);
    }

    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/LevelSelect.tscn");
    }

    // ──────────────────────────────────────────────────────────
    // Power-ups
    // ──────────────────────────────────────────────────────────
    private void UpdatePowerUpButtons()
    {
        _hintButton.Disabled = ProgressManager.Coins < HintCost || !_levelActive;
        _shuffleButton.Disabled = ProgressManager.Coins < ShuffleCost || !_levelActive;
        _revealButton.Disabled = ProgressManager.Coins < RevealCost || !_levelActive;

        _hintButton.Text = $"HINT\n({HintCost})";
        _shuffleButton.Text = $"SHUFFLE\n({ShuffleCost})";
        _revealButton.Text = $"REVEAL\n({RevealCost})";
    }

    private void OnHintPressed()
    {
        if (!_levelActive) return;

        if (!ProgressManager.SpendCoins(HintCost))
        {
            ShowToast("Not enough coins");
            return;
        }

        UpdateCoinDisplay();

        // Find an unfilled slot in an unfound target word and reveal one letter
        foreach (var word in _levelData.TargetWords)
        {
            if (_foundWords.Contains(word)) continue;
            if (!_wordSlots.TryGetValue(word, out var slots)) continue;

            // Find empty slots in this word
            var emptyIndices = new List<int>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (string.IsNullOrEmpty(slots[i].Text))
                    emptyIndices.Add(i);
            }

            if (emptyIndices.Count == 0) continue;

            // Pick a random empty slot
            var rng = new Random();
            int idx = emptyIndices[rng.Next(emptyIndices.Count)];
            slots[idx].Text = word[idx].ToString();
            var style = (StyleBoxFlat)slots[idx].GetThemeStylebox("normal").Duplicate();
            style.BgColor = new Color(0.40f, 0.50f, 0.34f); // hint color
            slots[idx].AddThemeStyleboxOverride("normal", style);

            ShowToast("Hint revealed!");
            return;
        }

        // No empty slots found — refund
        ProgressManager.AddCoins(HintCost);
        UpdateCoinDisplay();
        ShowToast("No slots to reveal");
    }

    private void OnShufflePressed()
    {
        if (!_levelActive) return;

        if (!ProgressManager.SpendCoins(ShuffleCost))
        {
            ShowToast("Not enough coins");
            return;
        }

        UpdateCoinDisplay();
        _letterWheel.ShuffleLetters();
        ShowToast("Shuffled!");
    }

    private void OnRevealPressed()
    {
        if (!_levelActive) return;

        if (!ProgressManager.SpendCoins(RevealCost))
        {
            ShowToast("Not enough coins");
            return;
        }

        UpdateCoinDisplay();

        // Find an unfound target word and fill it completely
        foreach (var word in _levelData.TargetWords)
        {
            if (_foundWords.Contains(word)) continue;
            if (!_wordSlots.TryGetValue(word, out var slots)) continue;

            _foundWords.Add(word);
            FillSlots(word, slots);
            ShowToast($"Revealed: {word}");
            CheckCompletion();
            return;
        }

        // All words already found — refund
        ProgressManager.AddCoins(RevealCost);
        UpdateCoinDisplay();
        ShowToast("All words already found");
    }

    // ──────────────────────────────────────────────────────────
    // UI helpers
    // ──────────────────────────────────────────────────────────
    private void UpdateCoinDisplay()
    {
        _coinLabel.Text = $"🪙 {ProgressManager.Coins}";
    }

    private void ShowToast(string message)
    {
        _toastLabel.Text = message;
        _toastLabel.Visible = true;

        var tween = CreateTween();
        tween.TweenInterval(1.5);
        tween.TweenCallback(Callable.From(() =>
        {
            _toastLabel.Visible = false;
        }));
    }
}
