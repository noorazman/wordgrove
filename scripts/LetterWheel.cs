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

    // Computed each time PositionLetters runs
    private float _wheelRadius;
    private float _letterRadius;   // dynamically sized
    private int _fontSize;
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

        // Re-layout when the control is resized (orientation change, etc.)
        Resized += OnResized;
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

    /// <summary>
    /// Rearranges the letters on the wheel into a new random order
    /// (same letters, different positions). Used by the SHUFFLE power-up.
    /// </summary>
    public void ShuffleLetters()
    {
        if (_letters.Length <= 1) return;

        var rng = new Random();
        var shuffled = (char[])_letters.Clone();
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        _letters = shuffled;
        _selectedIndices.Clear();
        _dragging = false;
        _trail.ClearPoints();
        BuildWheel();
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

    private void OnResized()
    {
        if (_letters.Length == 0) return;
        // Reposition existing letters to match the new size
        RebuildWheel();
    }

    private void RebuildWheel()
    {
        foreach (var btn in _buttons)
        {
            btn.Container.QueueFree();
        }
        _buttons.Clear();
        _trail.ClearPoints();
        PositionLetters();
    }

    private void PositionLetters()
    {
        var size = Size;
        if (size.X < 1 || size.Y < 1) return; // not laid out yet

        _center = new Vector2(size.X / 2f, size.Y / 2f);

        int count = _letters.Length;
        if (count == 0) return;

        // ── Compute wheel radius to fit inside the control ──
        // Use the smaller dimension so the wheel is always fully visible,
        // and cap the absolute size so it stays reasonable on large screens.
        float fitDim = Mathf.Min(size.X, size.Y);
        fitDim = Mathf.Min(fitDim, 700f); // absolute cap

        // Leave a 10% margin on each side for the letter discs
        float margin = fitDim * 0.10f;
        float maxR = (fitDim / 2f - margin) / (1f + Mathf.Pi / count);

        _wheelRadius = maxR;
        _letterRadius = Mathf.Pi * _wheelRadius / count;

        // Clamp letter radius to a sensible range
        _letterRadius = Mathf.Clamp(_letterRadius, 16f, 44f);
        _fontSize = Mathf.Clamp((int)(_letterRadius * 0.8f), 12, 36);

        // Final safety: ensure the whole wheel fits within the control
        float totalExtent = _wheelRadius + _letterRadius;
        float halfMin = Mathf.Min(size.X, size.Y) / 2f;
        if (totalExtent > halfMin - 8f)
        {
            _wheelRadius = halfMin - _letterRadius - 12f;
            _wheelRadius = Mathf.Max(_wheelRadius, 40f);
        }

        float angleStep = Mathf.Tau / count;

        for (int i = 0; i < count; i++)
        {
            // Start at top (-PI/2), go clockwise
            float angle = -Mathf.Pi / 2f + i * angleStep;
            float x = _center.X + Mathf.Cos(angle) * _wheelRadius;
            float y = _center.Y + Mathf.Sin(angle) * _wheelRadius;

            var btn = CreateLetterButton(_letters[i], i, new Vector2(x, y));
            _buttons.Add(btn);
        }

        // Redraw the background arc
        QueueRedraw();
    }

    private LetterButton CreateLetterButton(char letter, int index, Vector2 centerPos)
    {
        float d = _letterRadius * 2f; // diameter

        // Container for positioning — MUST NOT consume input
        var container = new Control
        {
            Position = new Vector2(centerPos.X - _letterRadius, centerPos.Y - _letterRadius),
            Size = new Vector2(d, d),
            MouseFilter = MouseFilterEnum.Ignore
        };

        // Rounded panel as visual background
        var panel = new Panel
        {
            Position = Vector2.Zero,
            Size = new Vector2(d, d),
            MouseFilter = MouseFilterEnum.Ignore
        };
        var panelStyle = new StyleBoxFlat
        {
            BgColor = LetterBg,
            CornerRadiusTopLeft = (int)_letterRadius,
            CornerRadiusTopRight = (int)_letterRadius,
            CornerRadiusBottomLeft = (int)_letterRadius,
            CornerRadiusBottomRight = (int)_letterRadius
        };
        panel.AddThemeStyleboxOverride("panel", panelStyle);

        var label = new Label
        {
            Position = Vector2.Zero,
            Size = new Vector2(d, d),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = letter.ToString(),
            MouseFilter = MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", _fontSize);
        label.AddThemeColorOverride("font_color", LetterFg);
        label.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());

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
    public override void _GuiInput(InputEvent ev)
    {
        if (!_enabled) return;

        if (ev is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    StartDrag(mb.Position);
                    AcceptEvent();
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

        string word = new string(_selectedIndices.Select(i => _buttons[i].Letter).ToArray());

        if (word.Length > 0)
        {
            EmitSignal(SignalName.WordSubmitted, word);
        }

        var tween = CreateTween();
        tween.TweenInterval(0.15);
        tween.TweenCallback(Callable.From(ClearSelection));
    }

    private void SelectLetter(int index)
    {
        _selectedIndices.Add(index);
        SetButtonSelected(_buttons[index], true);
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
        if (_dragging)
        {
            _trail.AddPoint(currentPos);
        }
    }

    // ── Hit testing ────────────────────────────────────────
    private int HitTestLetter(Vector2 pos)
    {
        float hitRadius = _letterRadius * 1.3f;
        float maxDist = _wheelRadius * 1.4f;

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
        DrawArc(_center, _wheelRadius + _letterRadius + 10f, 0f, Mathf.Tau, 64, WheelCircleColor, 3f);
    }
}
