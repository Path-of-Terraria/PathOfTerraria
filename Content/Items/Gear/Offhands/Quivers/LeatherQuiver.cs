using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Quivers;

internal class LeatherQuiver : Quiver
{
	protected override float AmmoConsumptionChance => 0.05f;
	protected override float MovementSpeedBonus => 0.0f;
	protected override int Defence => 3;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 1;
	}

	protected override void InternalDefaults()
	{
		Item.Size = new(32, 32);
		Item.value = Item.buyPrice(0, 0, 1, 0);
		Item.defense = Defence;
	}
}