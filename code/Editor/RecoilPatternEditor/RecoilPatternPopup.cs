using Sandbox;
using System;
using Editor;

namespace Gunfight.Editor;

public class RecoilPatternPopup : PopupWidget
{
    public Action<RecoilPattern> OnValueChanged { get; set; }

	RecoilPattern _value;

    public RecoilPattern Value
    {
        get => _value;

        set
        {
            _value = value;
            Update();
            OnValueChanged?.Invoke(_value);
        }
    }

	RecoilPatternEditor Editor;

    public RecoilPatternPopup( Widget parent, RecoilPattern value ) : base(parent)
    {
        Value = value;
        MinimumSize = new Vector2(500, 500);

        Editor = new RecoilPatternEditor(this);
        Editor.Size = new Vector2(500, 500);
        Editor.MinimumSize = Editor.Size;

		// Add an instance
		Editor.AddInstance( () => Value, x => Value = x );

        Layout = Layout.Column();
        Layout.Margin = 8;
        Layout.Add(Editor);

        AddShadow();
    }

    Vector2? lastM;
    protected override void OnMousePress(MouseEvent e)
    {

        if (e.LeftMouseButton && LocalRect.IsInside(e.LocalPosition))
        {
            lastM = e.ScreenPosition;
            e.Accepted = true;
            return;
        }

        base.OnMousePress(e);

    }

    protected override void OnMouseReleased(MouseEvent e)
    {
        base.OnMouseReleased(e);

        lastM = null;
    }

    protected override void OnMouseMove(MouseEvent e)
    {

        if (e.ButtonState.HasFlag(MouseButtons.Left) && lastM.HasValue)
        {
            var d = e.ScreenPosition - lastM.Value;
            lastM = e.ScreenPosition;

            Position += d;
            ConstrainToScreen();
            e.Accepted = true;
            return;
        }

        base.OnMouseMove(e);

    }
}
