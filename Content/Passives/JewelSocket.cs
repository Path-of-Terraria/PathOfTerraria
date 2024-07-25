using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.TreeSystem;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;

internal class JewelSocket : Passive
{
	public Jewel Socketed;
	public override string DisplayTooltip => Socketed == null ? "" : Socketed.EquppedTooltip;
	public override string InternalIdentifier => "JewelSocket";
	
	public override void BuffPlayer(Player player)
	{
		Socketed?.ApplyAffixes(player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier);
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