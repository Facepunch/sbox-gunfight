using Sandbox.UI;

namespace Facepunch.Gunfight;

[UseTemplate]
public partial class InputHint : Panel
{
	// @ref
	public Image Glyph { get; set; }
	public InputButton Button { get; set; }
	public string Text { get; set; }
	public Label ActionLabel { get; set; }


	public void SetButton( InputButton button )
	{
		Button = button;
	}

	public override void SetContent( string value )
	{
		base.SetContent( value );

		ActionLabel.SetText( value );
		Text = value;
	}

	public override void Tick()
	{
		base.Tick();


			Texture glyphTexture = Input.GetGlyph( Button, InputGlyphSize.Medium, GlyphStyle.Dark.WithNeutralColorABXY().WithNeutralColorABXY() );
			if ( glyphTexture is null )
				return;

			Glyph.Texture = glyphTexture;


			// @TODO: sort this out, it's pretty shitty
			if ( glyphTexture.Width > glyphTexture.Height )
			{
				Glyph.Style.Width = Length.Pixels( 24f );
				Glyph.Style.Height = Length.Pixels( 24f );
			}
			else
			{
				Glyph.Style.Width = Length.Pixels( 24f );
				Glyph.Style.Height = Length.Pixels( 24f );
			}
	
	}
}
