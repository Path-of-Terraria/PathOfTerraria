using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.EnergyShield;
using PathOfTerraria.Common.Systems.EquipmentRequirements;
using PathOfTerraria.Common.Systems.ItemStats;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor;

internal enum ArmorPieceType
{
	Helmet,
	Chestplate,
	Leggings,
}

internal abstract class EnergyShieldArmor : Gear, IEnergyShieldItem, IEnergyShieldRangeItem, IItemRequirements
{
	protected abstract ArmorPieceType PieceType { get; }
	protected abstract int MinimumDropItemLevel { get; }
	protected abstract int MaximumDropItemLevel { get; }
	protected abstract int MinimumEnergyShield { get; }
	protected abstract int MaximumEnergyShield { get; }

	public (int Minimum, int Maximum) EnergyShieldRange => (MinimumEnergyShield, MaximumEnergyShield);
	public ItemRequirements Requirements => ItemRequirements.CreateArmorBase(MinimumDropItemLevel, RequirementSlot, RequirementAttribute.Intelligence);

	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Armor/{TextureFolder}/{GetType().Name}";
	protected override string GearLocalizationCategory => PieceType.ToString();

	private string TextureFolder => PieceType switch
	{
		ArmorPieceType.Helmet => "Helmet",
		ArmorPieceType.Chestplate => "Body",
		ArmorPieceType.Leggings => "Legs",
		_ => throw new ArgumentOutOfRangeException(),
	};

	private ArmorRequirementSlot RequirementSlot => PieceType switch
	{
		ArmorPieceType.Helmet => ArmorRequirementSlot.Helmet,
		ArmorPieceType.Chestplate => ArmorRequirementSlot.Chestplate,
		ArmorPieceType.Leggings => ArmorRequirementSlot.Leggings,
		_ => throw new ArgumentOutOfRangeException(),
	};

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.SetDropItemLevelRange(MinimumDropItemLevel, MaximumDropItemLevel);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		this.GetInstanceData().ItemType = PieceType switch
		{
			ArmorPieceType.Helmet => ItemType.Helmet,
			ArmorPieceType.Chestplate => ItemType.Chestplate,
			ArmorPieceType.Leggings => ItemType.Leggings,
			_ => throw new ArgumentOutOfRangeException(),
		};
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return PieceType == ArmorPieceType.Leggings
			? [(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(8)]
			: [];
	}

	public override void UpdateEquip(Player player)
	{
		if (EquipmentRequirementPlayer.IsItemEnabled(player, Item))
		{
			player.GetModPlayer<EnergyShieldPlayer>().AddArmorEnergyShield(Item);
		}
	}

	public override void PostRoll()
	{
		EnergyShieldItem.RollBaseEnergyShield(Item, MinimumEnergyShield, MaximumEnergyShield);
		Item.defense = 0;
	}
}
