using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Core.Systems.TreeSystem;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;

internal class JewelSocket : Passive
{
	public Jewel Socketed;
	public override string DisplayTooltip => Socketed == null ? "" : Socketed.EquppedTooltip;
	public override string InternalIdentifier => "JewelSocket";
	
	public override void BuffPlayer(Player player)
	{
		if (Socketed is not null)
		{
			PoTGlobalItem.ApplyAffixes(Socketed.Item, player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier);
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