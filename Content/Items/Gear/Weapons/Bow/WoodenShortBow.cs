using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal class WoodenShortBow : Bow
{
	protected override int AnimationSpeed => 5;
	protected override float CooldownTimeInSeconds => 3.5f;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.useTime = 45;
		Item.useAnimation = 45;
		Item.width = 18;
		Item.height = 30;
		Item.damage = 7;
	}
}