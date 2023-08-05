﻿using System.ComponentModel;

namespace Facepunch.Gunfight;

public partial class GunfightPlayer
{
	/// <summary>
	/// Should be Input.AnalogMove
	/// </summary>
	[ClientInput] public Vector2 MoveInput { get; set; }

	/// <summary>
	/// Normalized accumulation of Input.AnalogLook
	/// </summary>
	[ClientInput] public Angles LookInput { get; set; }

	/// <summary>
	/// Position a player should be looking from in world space.
	/// </summary>
	[Browsable( false )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
	/// </summary>
	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	/// <summary>
	/// Override the aim ray to use the player's eye position and rotation.
	/// </summary>
	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	public override void BuildInput()
	{
		if ( Game.LocalClient.Components.Get<DevCamera>() != null ) return;

		ActiveChild?.BuildInput();
		Controller?.BuildInput();

		BuildWeaponInput();

		MoveInput = Input.AnalogMove;
		var lookInput = (LookInput + Input.AnalogLook).Normal;

		// Since we're a FPS game, let's clamp the player's pitch between -90, and 90.
		LookInput = lookInput.WithPitch( lookInput.pitch.Clamp( -90f, 90f ) );
	}
}
