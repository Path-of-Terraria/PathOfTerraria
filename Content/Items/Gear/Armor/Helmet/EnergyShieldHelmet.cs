using PathOfTerraria.Common.Systems.EnergyShield;
using PathOfTerraria.Common.Systems.ItemStats;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

internal abstract class EnergyShieldHelmet : Helmet, IEnergyShieldItem, IEnergyShieldRangeItem
{
	protected abstract int MinimumDropItemLevel { get; }
	protected abstract int MaximumDropItemLevel { get; }
	protected abstract int MinimumEnergyShield { get; }
	protected abstract int MaximumEnergyShield { get; }

	public (int Minimum, int Maximum) EnergyShieldRange => (MinimumEnergyShield, MaximumEnergyShield);

	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Armor/Helmet/{GetType().Name}";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		this.GetStaticData().SetDropItemLevelRange(MinimumDropItemLevel, MaximumDropItemLevel);
	}

	public override void UpdateEquip(Player player)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddArmorEnergyShield(Item);
	}

	public override void PostRoll()
	{
		EnergyShieldItem.RollBaseEnergyShield(Item, MinimumEnergyShield, MaximumEnergyShield);
		Item.defense = 0;
	}
}
