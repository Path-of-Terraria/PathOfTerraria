using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

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

		public override void OnHurt(Player.HurtInfo info)
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.whoAmI != Player.whoAmI && player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<WillingSacrificeMastery>(out float value))
				{
					player.Hurt(info.DamageSource, (int)(info.Damage * (1 - value / 100f)), 0, false, false, -1, false, 0, 1, 0);
				}
			}
		}
	}

	const int DefenseBuff = 15;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, DefenseBuff);

	public override void BuffPlayer(Player player)
	{
		player.statDefense += DefenseBuff;
		player.noKnockback = true;
	}
}
