using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that can be used to reroll the affixes of a magic item.
/// </summary>
public class GlimmeringShard : CurrencyShard
{
	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 5000f;
	}

	public override bool CanRightClick()
	{
		if (Main.LocalPlayer.HeldItem.GetInstanceData().Rarity == ItemRarity.Magic)
		{
			return base.CanRightClick();
		}

		//Main.NewText(Language.GetTextValue($"Mods.{PoTMod.ModName}.Misc.ShardNotifs.Glimmering"));
		return false;
	}

	public override void RightClick(Player player)
	{
		PoTItemHelper.Roll(player.HeldItem, player.HeldItem.GetInstanceData().RealLevel);
	}
}
