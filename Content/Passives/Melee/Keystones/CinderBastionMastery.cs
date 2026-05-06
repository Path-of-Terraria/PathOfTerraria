using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

internal class CinderBastionMastery : Passive
{
	const float IgniteDamagePerLifePer10 = 1f; 
	private const float SelfIgnitePercent = 20f; // 20% max life per second
	
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
					
					IgnitedDebuff.ApplyTo(null, Player, damage, 60); //1s duration for self ignite
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
			ignitedPlayer.IgniteDamage += igniteDamageMultiplier - 1f; // Additive multiplier
		}
	}
}