using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class MasterBuilderMastery : Passive
{
	private const float TileSpeedBoost = 50;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, TileSpeedBoost);

	public override void BuffPlayer(Player player)
	{
		if (player.velocity.LengthSquared() < 0.1f)
		{
			player.blockRange += Value;
			player.tileSpeed += TileSpeedBoost / 100f;
		}
	}
}
