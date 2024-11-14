using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

internal class EchoingShard : CurrencyShard
{
	public override bool CanRightClick()
	{
		return base.CanRightClick() && Main.LocalPlayer.HeldItem.GetInstanceData().Rarity != ItemRarity.Normal;
	}
	
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Master;
	}

	/// <summary>
	/// Clones the item in the player's hand, adding the Cloned tag on the item
	/// </summary>
	/// <param name="player"></param>
	public override void RightClick(Player player)
	{
		PoTInstanceItemData data = player.HeldItem.GetInstanceData();

		var clonedItem = new Item();
		clonedItem.SetDefaults(player.HeldItem.type);
		PoTInstanceItemData clonedData = clonedItem.GetInstanceData();
		clonedData.Rarity = data.Rarity;
		clonedData.Influence = data.Influence;
		clonedData.SpecialName = data.SpecialName;
		clonedData.ImplicitCount = data.ImplicitCount;
		clonedData.RealLevel = data.RealLevel;
		clonedData.Affixes = [..data.Affixes];
		clonedData.Cloned = true;

		var source = new Terraria.DataStructures.EntitySource_Misc("EchoingShard");
		player.QuickSpawnItem(source, clonedItem, 1);
	}
}