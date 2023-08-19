using Facepunch.Gunfight.UI;
using System;

namespace Facepunch.Gunfight;

public partial class Progression
{
	public static partial class Levelling
	{
		public const int MAX_LEVEL = 50;
		public const int MAX_EXPERIENCE = 1000000;
		public const int DEFAULT_XP = 300;

		private static int _level = 0;
		private static int _experience;

		public static int CurrentLevel
		{
			get => _level;
		}

		public static int TotalExperience
		{
			get => _experience;
			set
			{
				value = value.Clamp( 0, MAX_EXPERIENCE );

				var level = GetLevelFromExperience( value )
					.Clamp( 0, MAX_LEVEL );

				var previousXp = _experience;
				var previousLevel = _level;

				_experience = value;

				_level = level;

				if ( previousXp > 0 )
				{
					Event.Run( "gunfight.progression.xp", new ExperienceChangedData
					{
						PreviousXp = previousXp,
						CurrentXp = _experience,
						PreviousLevel = previousLevel,
						CurrentLevel = _level
					} );
				}

				if ( _level != previousLevel ) BroadcastLevel( _level );
			}
		}

		public static int GetRequiredExperienceForLevel( int level )
		{
			var exponent = 1.5f;
			var baseXp = 1000;

			return (int)MathF.Floor( baseXp * MathF.Pow( level, exponent ) );
		}

		public static int GetLevelFromExperience( int experience )
		{
			int lvl = 0;
			while ( lvl <= MAX_LEVEL )
			{
				var requiredXp = GetRequiredExperienceForLevel( lvl );

				if ( requiredXp >= experience ) return lvl - 1;

				lvl++;
			}

			return lvl;
		}

		private static async void LoadAsync()
		{
			await Sandbox.Services.Stats.LocalPlayer.Refresh();

			var stat = Sandbox.Services.Stats.LocalPlayer.Get( "experience" );
			var xp = stat.Value;
			if ( xp == 0 ) xp = DEFAULT_XP;

			TotalExperience = (int)xp;
		}

		[ConCmd.Client( "gunfight_progression_load_xp" )]
		public static void Load()
		{
			LoadAsync();
		}

		public static void Save()
		{
			Sandbox.Services.Stats.SetValue( "experience", _experience );
		}

		public static void GiveExperience( int experience )
		{
			TotalExperience += experience;
			Save();
		}

		/// <summary>
		/// Tell the server what your level is.
		/// This is purely cosmetic.
		/// </summary>
		/// <param name="level"></param>
		[ConCmd.Server( "gunfight_progression_broadcast_lvl" )]
		public static void BroadcastLevel( int level )
		{
			ConsoleSystem.Caller.Components.GetOrCreate<PlayerLevelComponent>().Level = level;
		}

		[ClientRpc]
		public static void RpcLoad()
		{
			Load();
		}

		/// <summary>
		/// Event for when xp changes
		/// </summary>
		public struct ExperienceChangedData
		{
			public int PreviousXp;
			public int CurrentXp;
			public int PreviousLevel;
			public int CurrentLevel;
		}

		[ConCmd.Server( "gunfight_progression_export" )]
		public static void Export()
		{
			for ( int i = 0; i <= MAX_LEVEL; i++ )
			{
				var xpRequiredForLevel = Progression.Levelling.GetRequiredExperienceForLevel( i );

				Log.Info( $"Level {i} requires {xpRequiredForLevel} XP" );
			}
		}

		[Event( "gunfight.progression.xp" )]
		public static void OnXpGain( ExperienceChangedData data )
		{
			if ( data.CurrentLevel > data.PreviousLevel )
			{
				NotificationSystem.Notify( $"You are now lvel {data.CurrentLevel}" );
			}
		}

		[ConCmd.Server( "gunfight_progression_level_reset" )]
		public static void Reset()
		{
			Progression.Levelling.TotalExperience = DEFAULT_XP;
			Progression.Levelling.Save();
		}
	}
}
