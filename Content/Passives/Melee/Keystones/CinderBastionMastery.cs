using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

internal class CinderBastionMastery : Passive
{
	const float IgniteDamagePerLifePer10 = 2f; 
	private const float SelfIgnitePercent = 10f; // 10% max life per second by default
	
	internal class CinderBastionPlayer : ModPlayer
	{
		private int _movementTimer = 0;
		
		
		public override void PreUpdate()
		{
			if (!Player.GetModPlayer<PassiveTreePlayer>().HasNode<CinderBastionMastery>())
			{
				return;
			}

			bool isMoving = Player.velocity.LengthSquared() > 0.1f;

			if (isMoving)
			{
				_movementTimer++;

				// Self ignite
				if (_movementTimer >= 20)
				{
					_movementTimer = 0;
					
					int damage = (int)(Player.statLifeMax2 * SelfIgnitePercent / 100f);
					
					IgnitedDebuff.ApplyTo(null, Player, damage, 130);
				}
			}
			else
			{
				_movementTimer = 0;
			}
		}
	}


	public override void BuffPlayer(Player player)
	{
		if (player.TryGetModPlayer(out IgnitedPlayer ignitedPlayer))
		{
			float igniteDamageMultiplier = 1f + (player.statLifeMax2 / 10f) * IgniteDamagePerLifePer10 / 100f;
			ignitedPlayer.IgniteDamage += igniteDamageMultiplier - 1f;
		}
	}
}