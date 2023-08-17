using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Gunfight;

public partial class Progression
{
	public static partial class Levelling
	{
		public const int MAX_LEVEL = 50;
		public const int MAX_EXPERIENCE = 1000000;

		private const string PERSISTENCE_BUCKET = "progression.level";

		private static int _level;
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

				var level = ExpToLevel( value )
					.Clamp( 0, MAX_LEVEL );

				_experience = value;
				_level = level;

				BroadcastLevel( _level );
			}
		}

		static int LevelToExp( int level )
		{
			return (int)MathF.Round( ( level ^ ( 50 / 27 ) ) * 600 );
		}

		static int ExpToLevel( int exp )
		{
			return (int)MathF.Round( ( exp / 600 ) ^ ( 27 / 50 ) ) + 1;
		}

		static int ExpRemaining( int exp )
		{
			return NextLevelExp() - exp;
		}

		static int NextLevelExp()
		{
			return LevelToExp( _experience + 1 );
		}

		public static void Load()
		{
			var xp = PersistenceSystem.Instance.Get( PERSISTENCE_BUCKET, "experience", 0 );
			
			TotalExperience = xp;
		}

		public static void Save()
		{
			PersistenceSystem.Instance.Set( PERSISTENCE_BUCKET, "experience", _experience );
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
		/// <param name="cl"></param>
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
	}
}
