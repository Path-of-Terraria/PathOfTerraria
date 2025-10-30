using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class AfterburnMastery : Passive
{
	internal class AfterburnPlayer : ModPlayer
	{
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			float str = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<AfterburnMastery>();
			modifiers.FinalDamage += str * 0.2f;
		}
	}

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value * 40f, Value * 20f);

	public override void BuffPlayer(Player player)
	{
		float str = player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<AfterburnMastery>() * 0.4f;
		player.GetModPlayer<ElementalPlayer>().Container[ElementType.Fire].Multiplier *= 1 + str;
	}
}
