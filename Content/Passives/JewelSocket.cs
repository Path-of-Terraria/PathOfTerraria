using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Core.Items;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;

internal class JewelSocket : Passive
{
	public Jewel Socketed;
	public override string DisplayTooltip => Socketed == null ? "" : Socketed.EquppedTooltip;

	public override void BuffPlayer(Player player)
	{
		if (Socketed is not null)
		{
			PoTItemHelper.ApplyAffixes(Socketed.Item, player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier, player);
		}
	}

	public void SaveJewel(TagCompound tag)
	{
		Socketed.SaveData(tag);
	}

	public void LoadJewel(TagCompound tag)
	{
		Socketed = Jewel.LoadFrom(tag);
	}
}