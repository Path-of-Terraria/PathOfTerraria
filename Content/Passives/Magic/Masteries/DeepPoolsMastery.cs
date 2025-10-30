using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class DeepPoolsMastery : Passive
{
	internal class DeepPoolsPlayerh : ModPlayer
	{
		public override void PostUpdateEquips()
		{
			Player.GetDamage(DamageClass.Magic) += Player.statManaMax2 / 25f;
		}
	}

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value * 40f, Value * 20f);

	public override void BuffPlayer(Player player)
	{
		player.statManaMax2 *= 2;
	}
}
