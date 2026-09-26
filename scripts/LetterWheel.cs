using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wordgrove;

public partial class LetterWheel : Control
{
    // ── Signal: emitted when the player lifts after forming a word ──
    [Signal]
    public delegate void WordSubmittedEventHandler(string word);

    // ── Styling ────────────────────────────────────────────
    private const float LetterRadius = 42f;
    private const float FontSize = 36f;
    private static readonly Color LetterBg = new(0.24f, 0.32f, 0.40f);
    private static readonly Color LetterBgSelected = new(0.95f, 0.76f, 0.22f);
    private static readonly Color LetterFg = new(1f, 1f, 1f);
    private static readonly Color LetterFgSelected = new(0.12f, 0.12f, 0.12f);
    private static readonly Color WheelCircleColor = new(0.18f, 0.24f, 0.30f);
    private static readonly Color InvalidFlashColor = new(0.85f, 0.2f, 0.2f);
    private static readonly Color TrailColor = new(0.95f, 0.76f, 0.22f, 0.7f);

    // ── Internal state ─────────────────────────────────────
    private char[] _letters = Array.Empty<char>();
    private readonly List<LetterButton> _buttons = new();
    private readonly List<int> _selectedIndices = new();
    private Line2D _trail;
    private bool _dragging;
    private bool _enabled = true;
    private float _wheelRadius;
    private Vector2 _center;

    // Represents one letter on the wheel
    private class LetterButton
    {
        public Label Label;
        public Panel Panel;
        public Control Container;
        public Vector2 Center;   // centre position relative to LetterWheel
        public int Index;
        public char Letter;
    }

    public override void _Ready()
    {
        _trail = GetNode<Line2D>("Trail");
        _trail.Width = 8f;
        _trail.DefaultColor = TrailColor;

        // LetterWheel MUST receive input — Stop means it captures events
        // inside its rect instead of letting them pass through.
        MouseFilter = MouseFilterEnum.Stop;
    }

    // ── Public API for Main.cs ─────────────────────────────
    public void Setup(char[] letters)
    {
        _letters = letters;
        _selectedIndices.Clear();
        _dragging = false;
        _enabled = true;
        BuildWheel();
    }

    public void Reset()
    {
        _selectedIndices.Clear();
        _dragging = false;
        _trail.ClearPoints();
        foreach (var btn in _buttons)
        {
            SetButtonSelected(btn, false);
        }
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
    }

    // ── Build the circular layout ──────────────────────────
    private void BuildWheel()
    {
        // Remove old letter buttons (but keep Trail)
        foreach (var btn in _buttons)
        {
            btn.Container.QueueFree();
        }
        _buttons.Clear();
        _trail.ClearPoints();

        // Wait one frame for size to settle, then position
        CallDeferred(MethodName.PositionLetters);
    }

    private void PositionLetters()
    {
        var size = Size;
        _center = new Vector2(size.X / 2f, size.Y / 2f);

        // Wheel radius: fit inside the container with padding
        _wheelRadius = Mathf.Min(size.X, size.Y) * 0.36f;

        int count = _letters.Length;
        float angleStep = Mathf.Tau / count;

        for (int i = 0; i < count; i++)
        {
            float angle = -Mathf.Pi / 2f + i * angleStep;
            float x = _center.X + Mathf.Cos(angle) * _wheelRadius;
            float y = _center.Y + Mathf.Sin(angle) * _wheelRadius;

            var btn = CreateLetterButton(_letters[i], i, new Vector2(x, y));
            _buttons.Add(btn);
        }

        // Redraw the background arc now that _center/_wheelRadius are set
        QueueRedraw();
    }

    private LetterButton CreateLetterButton(char letter, int index, Vector2 centerPos)
    {
        // Container for positioning — MUST NOT consume input
        var container = new Control
        {
            Position = new Vector2(centerPos.X - LetterRadius, centerPos.Y - LetterRadius),
            Size = new Vector2(LetterRadius * 2, LetterRadius * 2),
            MouseFilter = MouseFilterEnum.Ignore   // ← FIX: pass input through
        };

        // Rounded panel as visual background
        var panel = new Panel
        {
            Position = Vector2.Zero,
            Size = new Vector2(LetterRadius * 2, LetterRadius * 2),
            MouseFilter = MouseFilterEnum.Ignore   // ← FIX: pass input through
        };
        var panelStyle = new StyleBoxFlat
        {
            BgColor = LetterBg,
            CornerRadiusTopLeft = (int)LetterRadius,
            CornerRadiusTopRight = (int)LetterRadius,
            CornerRadiusBottomLeft = (int)LetterRadius,
            CornerRadiusBottomRight = (int)LetterRadius
        };
        panel.AddThemeStyleboxOverride("panel", panelStyle);

        var label = new Label
        {
            Position = Vector2.Zero,
            Size = new Vector2(LetterRadius * 2, LetterRadius * 2),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = letter.ToString(),
            MouseFilter = MouseFilterEnum.Ignore   // ← FIX: pass input through
        };
        label.AddThemeFontSizeOverride("font_size", (int)FontSize);
        label.AddThemeColorOverride("font_color", LetterFg);
        // Make label background transparent
        var labelStyle = new StyleBoxEmpty();
        label.AddThemeStyleboxOverride("normal", labelStyle);

        container.AddChild(panel);
        container.AddChild(label);
        AddChild(container);

        return new LetterButton
        {
            Label = label,
            Panel = panel,
            Container = container,
            Center = centerPos,
            Index = index,
            Letter = letter
        };
    }

    // ── Input handling ─────────────────────────────────────
    // All drag logic lives here in the parent LetterWheel.
    // Children have MouseFilter=Ignore so they never intercept.
    public override void _GuiInput(InputEvent ev)
    {
        if (!_enabled) return;

        // Handle both mouse and touch
        if (ev is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    StartDrag(mb.Position);
                    AcceptEvent();   // consume so nothing else gets it
                }
                else
                {
                    EndDrag();
                    AcceptEvent();
                }
            }
        }
        else if (ev is InputEventMouseMotion mm)
        {
            if (_dragging)
            {
                ContinueDrag(mm.Position);
                AcceptEvent();
            }
        }
        else if (ev is InputEventScreenTouch st)
        {
            if (st.Pressed)
            {
                StartDrag(st.Position);
                AcceptEvent();
            }
            else
            {
                EndDrag();
                AcceptEvent();
            }
        }
        else if (ev is InputEventScreenDrag sd)
        {
            if (_dragging)
            {
                ContinueDrag(sd.Position);
                AcceptEvent();
            }
        }
    }

    private void StartDrag(Vector2 pos)
    {
        var hit = HitTestLetter(pos);
        if (hit < 0) return;

        _dragging = true;
        _selectedIndices.Clear();
        _trail.ClearPoints();

        SelectLetter(hit);
        UpdateTrail(pos);
    }

    private void ContinueDrag(Vector2 pos)
    {
        if (!_dragging) return;

        var hit = HitTestLetter(pos);
        if (hit >= 0 && !_selectedIndices.Contains(hit))
        {
            SelectLetter(hit);
        }
        UpdateTrail(pos);
    }

    private void EndDrag()
    {
        if (!_dragging) return;
        _dragging = false;

        // Build the word from selected letters
        string word = new string(_selectedIndices.Select(i => _buttons[i].Letter).ToArray());

        if (word.Length > 0)
        {
            GD.Print($"Wheel: submitted \"{word}\"");
            EmitSignal(SignalName.WordSubmitted, word);
        }

        // Brief delay, then clear selection
        var tween = CreateTween();
        tween.TweenInterval(0.15);
        tween.TweenCallback(Callable.From(ClearSelection));
    }

    private void SelectLetter(int index)
    {
        _selectedIndices.Add(index);
        SetButtonSelected(_buttons[index], true);
        GD.Print($"Wheel: selected '{_buttons[index].Letter}'");
    }

    private void ClearSelection()
    {
        foreach (var idx in _selectedIndices)
        {
            if (idx < _buttons.Count)
                SetButtonSelected(_buttons[idx], false);
        }
        _selectedIndices.Clear();
        _trail.ClearPoints();
    }

    private void SetButtonSelected(LetterButton btn, bool selected)
    {
        var style = (StyleBoxFlat)btn.Panel.GetThemeStylebox("panel").Duplicate();
        style.BgColor = selected ? LetterBgSelected : LetterBg;
        btn.Panel.AddThemeStyleboxOverride("panel", style);
        btn.Label.AddThemeColorOverride("font_color", selected ? LetterFgSelected : LetterFg);
    }

    // ── Trail drawing ──────────────────────────────────────
    private void UpdateTrail(Vector2 currentPos)
    {
        _trail.ClearPoints();
        foreach (var idx in _selectedIndices)
        {
            _trail.AddPoint(_buttons[idx].Center);
        }
        // Add the current finger/mouse position as final point
        if (_dragging)
        {
            _trail.AddPoint(currentPos);
        }
    }

    // ── Hit testing ────────────────────────────────────────
    // Check distance from each letter center. Accept if within
    // the letter circle radius (with some forgiveness) AND within
    // a reasonable band of the wheel radius.
    private int HitTestLetter(Vector2 pos)
    {
        float hitRadius = LetterRadius * 1.3f; // slightly forgiving per-letter
        float maxDist = _wheelRadius * 1.4f;   // reject taps way outside the wheel

        float distFromCenter = pos.DistanceTo(_center);
        if (distFromCenter > maxDist)
            return -1;

        int best = -1;
        float bestDist = float.MaxValue;

        for (int i = 0; i < _buttons.Count; i++)
        {
            float d = pos.DistanceTo(_buttons[i].Center);
            if (d <= hitRadius && d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }

    // ── Flash invalid (called externally if needed) ────────
    public void FlashInvalid()
    {
        foreach (var idx in _selectedIndices)
        {
            var style = (StyleBoxFlat)_buttons[idx].Panel.GetThemeStylebox("panel").Duplicate();
            style.BgColor = InvalidFlashColor;
            _buttons[idx].Panel.AddThemeStyleboxOverride("panel", style);
        }

        var tween = CreateTween();
        tween.TweenInterval(0.2);
        tween.TweenCallback(Callable.From(ClearSelection));
    }

    // ── Draw wheel background circle ───────────────────────
    public override void _Draw()
    {
        if (_buttons.Count == 0) return;
        // Draw a subtle circle behind the letters
        DrawArc(_center, _wheelRadius + LetterRadius + 10f, 0f, Mathf.Tau, 64, WheelCircleColor, 3f);
    }
}
