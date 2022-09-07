namespace Facepunch.Gunfight;

public partial class PlayerAnimator : StandardPlayerAnimator
{
	private float SlideAmount { get; set; }

	public override void Simulate()
	{
		base.Simulate();

		if ( HasTag( "sliding" ) )
			SlideAmount = SlideAmount.LerpTo( 1f, Time.Delta * 5f );
		else
			SlideAmount = SlideAmount.LerpTo( 0f, Time.Delta * 5f );

		SetAnimParameter( "skid", SlideAmount );
	}
}
