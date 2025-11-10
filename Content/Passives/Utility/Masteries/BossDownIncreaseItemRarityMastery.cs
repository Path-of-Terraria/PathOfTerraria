using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class BossDownIncreaseItemRarityMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		float count = BossTracker.TotalBossesDowned.Count * 0.005f;
		player.GetModPlayer<ItemDropModifierPlayer>().MagicFind += count;
	}

	public override string DisplayTooltip => base.DisplayTooltip + " " + BossTracker.TotalBossesDowned.Count * 0.5f + "%";
}
