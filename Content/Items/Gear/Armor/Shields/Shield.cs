using PathOfTerraria.Common.Systems.BlockSystem;
using PathOfTerraria.Content.Items.Gear.Offhands;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Armor.Shields;

internal abstract class Shield : Offhand
{
	protected abstract float BlockChance { get; }
	protected abstract float SpeedReduction { get; }

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 1;
	}

	public override void SetDefaults()
	{
		Item.accessory = true;

		InternalDefaults();
	}

	protected virtual void InternalDefaults()
	{
	}

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<BlockPlayer>().AddBlockChance(BlockChance);
		player.moveSpeed *= 1 - SpeedReduction;
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		tooltips.Add(new TooltipLine(Mod, "StatBlock", Language.GetText("Mods.PathOfTerraria.Gear.Shield.Block").Format((BlockChance * 100).ToString("#0.##"))));
		tooltips.Add(new TooltipLine(Mod, "StatSpeed", Language.GetText("Mods.PathOfTerraria.Gear.Shield.Speed").Format((SpeedReduction * 100).ToString("#0.##"))));
	}
}
