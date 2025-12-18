using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class WillingSacrificeMastery : Passive
{
	internal class WillingSacrificePlayer : ModPlayer
	{
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.whoAmI != Player.whoAmI && player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<WillingSacrificeMastery>(out float value))
				{
					modifiers.FinalDamage *= value / 100f;
				}
			}
		}
	}

	const int DefenseBuff = 15;

	public override void BuffPlayer(Player player)
	{
		player.statDefense += DefenseBuff;
		player.noKnockback = true;
	}
}
