using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class HermesBoonMastery : Passive
{
	internal class OutOfCombatPlayer : ModPlayer
	{
		public const int MaxTime = 360;

		public bool InCombat => _combatTime >= 0;

		int _combatTime = 0;

		public override void ResetEffects()
		{
			_combatTime--;
		}

		public override void OnHitAnything(float x, float y, Entity victim)
		{
			_combatTime = MaxTime;
		}

		public override void OnHurt(Player.HurtInfo info)
		{
			_combatTime = MaxTime;
		}
	}

	public override void BuffPlayer(Player player)
	{
		if (player.GetModPlayer<OutOfCombatPlayer>().InCombat)
		{
			return;
		}

		player.moveSpeed *= 1 + Value / 100f;
		player.jumpSpeedBoost *= 1 + Value / 10f;
	}
}
