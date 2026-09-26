using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Wordgrove;

public partial class Main : Control
{
    // ── Level data (hardcoded for now) ──────────────────────
    private readonly string _theme = "ANIMALS";
    private readonly string[] _targetWords = { "CAT", "DOG", "BIRD", "FISH" };
    private readonly char[] _letterPool = { 'C', 'A', 'T', 'D', 'O', 'G', 'B', 'I', 'R', 'F', 'S', 'H' };

    // ── Node references ────────────────────────────────────
    private Label _themeLabel;
    private VBoxContainer _gridArea;
    private LetterWheel _letterWheel;
    private ColorRect _completionOverlay;

    // ── State ──────────────────────────────────────────────
    // Each word row is a list of Label nodes (one per letter slot)
    private readonly Dictionary<string, List<Label>> _wordSlots = new();
    private readonly HashSet<string> _foundWords = new();

    // ── Styling constants ──────────────────────────────────
    private const int SlotSize = 64;
    private const int SlotGap = 8;
    private const int RowGap = 12;
    private static readonly Color SlotEmpty = new(0.22f, 0.28f, 0.34f);
    private static readonly Color SlotFilled = new(0.18f, 0.56f, 0.34f);
    private static readonly Color SlotText = new(1f, 1f, 1f);
    private static readonly Color ThemeColor = new(0.95f, 0.76f, 0.22f);

    public override void _Ready()
    {
        _themeLabel = GetNode<Label>("ThemeLabel");
        _gridArea = GetNode<VBoxContainer>("GridArea");
        _letterWheel = GetNode<LetterWheel>("WheelArea/LetterWheel");
        _completionOverlay = GetNode<ColorRect>("CompletionOverlay");

        _letterWheel.WordSubmitted += OnWordSubmitted;

        BuildLevel();
    }

    // ── Build the grid + wheel for the current level ───────
    private void BuildLevel()
    {
        _themeLabel.Text = _theme;
        _themeLabel.AddThemeColorOverride("font_color", ThemeColor);
        _themeLabel.AddThemeFontSizeOverride("font_size", 48);

        // Style the completion label while we're at it
        var completeLabel = _completionOverlay.GetNode<Label>("CompleteLabel");
        completeLabel.AddThemeFontSizeOverride("font_size", 64);
        completeLabel.AddThemeColorOverride("font_color", ThemeColor);

        BuildGrid();
        _letterWheel.Setup(_letterPool);
        _completionOverlay.Visible = false;
        _foundWords.Clear();
    }

    // ── Grid: one row per target word ──────────────────────
    private void BuildGrid()
    {
        // Clear previous
        foreach (var child in _gridArea.GetChildren())
        {
            child.QueueFree();
        }
        _wordSlots.Clear();

        _gridArea.AddThemeConstantOverride("separation", RowGap);

        foreach (var word in _targetWords)
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
        // A Panel-like container: PanelContainer with a Label inside
        var label = new Label
        {
            CustomMinimumSize = new Vector2(SlotSize, SlotSize),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = ""
        };
        label.AddThemeFontSizeOverride("font_size", 36);
        label.AddThemeColorOverride("font_color", SlotText);

        // Use a StyleBoxFlat as background
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

    // ── Word submitted from the wheel ──────────────────────
    private void OnWordSubmitted(string word)
    {
        word = word.ToUpperInvariant();

        if (_foundWords.Contains(word))
        {
            // Already found — flash the row briefly
            FlashRow(word, new Color(0.4f, 0.6f, 0.8f));
            return;
        }

        if (_wordSlots.TryGetValue(word, out var slots))
        {
            // Correct word!
            _foundWords.Add(word);
            FillSlots(word, slots);
            CheckCompletion();
        }
        else
        {
            // Invalid word — the wheel handles its own shake
        }
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

            // Reset after a short delay via a tween
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

    private void CheckCompletion()
    {
        if (_foundWords.Count >= _targetWords.Length)
        {
            _completionOverlay.Visible = true;
            _letterWheel.SetEnabled(false);
        }
    }

    // ── Tap-to-reset when overlay is showing ───────────────
    public override void _UnhandledInput(InputEvent ev)
    {
        if (!_completionOverlay.Visible) return;

        if (ev is InputEventMouseButton { Pressed: true } or InputEventScreenTouch { Pressed: true })
        {
            ResetLevel();
            GetViewport().SetInputAsHandled();
        }
    }

    private void ResetLevel()
    {
        _completionOverlay.Visible = false;
        _foundWords.Clear();

        // Reset all slots
        foreach (var (word, slots) in _wordSlots)
        {
            foreach (var slot in slots)
            {
                slot.Text = "";
                var style = new StyleBoxFlat
                {
                    BgColor = SlotEmpty,
                    CornerRadiusTopLeft = 8,
                    CornerRadiusTopRight = 8,
                    CornerRadiusBottomLeft = 8,
                    CornerRadiusBottomRight = 8
                };
                slot.AddThemeStyleboxOverride("normal", style);
            }
        }

        _letterWheel.Reset();
        _letterWheel.SetEnabled(true);
    }
}
