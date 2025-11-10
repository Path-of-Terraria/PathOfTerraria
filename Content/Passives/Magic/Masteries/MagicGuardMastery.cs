using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class MagicGuardMastery : Passive
{
	internal class MagicGuardPlayer : ModPlayer
	{
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().HasNode<MagicGuardMastery>() && Player.statMana > Player.statManaMax2 * 0.5f)
			{
				modifiers.FinalDamage -= 0.92f;
			}
		}
	}

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format("8", "12");

	public override void BuffPlayer(Player player)
	{
		if (player.statMana <= player.statManaMax2 * 0.5f)
		{
			player.GetDamage(DamageClass.Generic) += 0.12f;
		}
	}
}
