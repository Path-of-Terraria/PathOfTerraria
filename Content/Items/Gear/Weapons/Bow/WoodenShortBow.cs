namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal class WoodenShortBow : Bow
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Bow/BasicWoodenBow";
	public override float DropChance => 1f;

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.useTime = 45;
		Item.useAnimation = 45;
		Item.height = 48;
		Item.damage = 7;
	}
}