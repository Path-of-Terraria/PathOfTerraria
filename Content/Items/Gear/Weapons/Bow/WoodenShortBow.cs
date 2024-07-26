namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal class WoodenShortBow : Bow
{
	public override float DropChance => 1f;
	protected override int AnimationSpeed => 5;
	protected override float CooldownTimeInSeconds => 3.5f;

	public override void SetDefaults()
	{
		Item.useTime = 45;
		Item.useAnimation = 45;
		Item.width = 18;
		Item.height = 30;
		Item.damage = 7;
	}
}