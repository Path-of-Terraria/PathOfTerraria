using System.Collections.Generic;
using PathOfTerraria.Common.Systems.BlockSystem;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.Items;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Quivers;

internal abstract class Quiver : Offhand
{
	protected abstract float MovementSpeedBonus { get; }
	protected abstract float AmmoConsumptionChance { get; }
	protected abstract int Defence { get; }
	protected override string GearLocalizationCategory => "Quiver";

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 1;
	}

	public override void SetDefaults()
	{
		Item.accessory = true;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Quiver;

		InternalDefaults();
	}

	protected virtual void InternalDefaults()
	{
	}

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.moveSpeed += MovementSpeedBonus;

		player.GetModPlayer<AmmoConsumptionPlayer>().AmmoSaveChance += AmmoConsumptionChance;
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		if (MovementSpeedBonus > 0)
		{
			tooltips.Add(new TooltipLine(Mod, "StatMovement", Language.GetText("Mods.PathOfTerraria.Gear.Quiver.Movement").Format((MovementSpeedBonus * 100).ToString("#0.##"))));
		}
		
		if (AmmoConsumptionChance > 0)
		{
			tooltips.Add(new TooltipLine(Mod, "StatAmmo", Language.GetText("Mods.PathOfTerraria.Gear.Quiver.AmmoSave").Format((AmmoConsumptionChance * 100).ToString("#0.##"))));
		}
	}
}