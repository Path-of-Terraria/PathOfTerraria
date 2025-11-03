using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class DeepPoolsMastery : Passive
{
	internal class DeepPoolsPlayer : ModPlayer
	{
		public override void PostUpdateEquips()
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().HasNode<DeepPoolsMastery>())
			{
				Player.GetDamage(DamageClass.Magic) += Player.statManaMax2 / 2500f;
			}
		}
	}

	public override void BuffPlayer(Player player)
	{
		player.statManaMax2 = (int)(player.statManaMax2 * (1 + Value / 100f));
	}
}
