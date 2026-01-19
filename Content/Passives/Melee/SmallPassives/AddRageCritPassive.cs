using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class AddRageCritPassive : Passive
{
	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value / 10f);

	public override void BuffPlayer(Player player)
	{
		player.GetCritChance(DamageClass.Generic) += player.GetModPlayer<RagePlayer>().Rage * Value * 0.01f;
	}
}

