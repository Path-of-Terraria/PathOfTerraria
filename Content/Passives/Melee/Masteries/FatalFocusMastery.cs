using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class FatalFocusMastery : Passive
{
	const float DamageMod = 0.2f;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, DamageMod * 100);

	public override void BuffPlayer(Player player)
	{
		player.GetCritChance(DamageClass.Generic) *= 1 - DamageMod;
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.CriticalDamage += Value / 100f;
	}
}
