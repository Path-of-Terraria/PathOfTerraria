using PathOfTerraria.Common.Systems.EnergyShield;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

internal abstract class EnergyShieldHelmet : Helmet, IEnergyShieldItem
{
	protected abstract int MinimumDropItemLevel { get; }
	protected abstract int MaximumDropItemLevel { get; }

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
		Item.defense = 0;
	}
}
