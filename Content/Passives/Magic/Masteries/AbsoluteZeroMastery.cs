using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class AbsoluteZeroMastery : Passive
{
	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value * 2, Value);

	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.FreezeEffectiveness += Value / 100f;
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ChilledEffectiveness += Value / 50f;
	}
}
