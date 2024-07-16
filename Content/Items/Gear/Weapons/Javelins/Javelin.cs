using PathOfTerraria.Content.Projectiles.Magic;
using Terraria.Enums;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal abstract class Javelin : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Javelin/{GetType().Name}";
	public override float DropChance => 1f;

	protected override string GearLocalizationCategory => "Javelin";

	public override void Load()
	{
		
	}

	public override void Defaults() 
	{
		Item.DefaultToThrownWeapon(ModContent.ProjectileType<SparklingBall>(), 50, 8, true);
		Item.consumable = false;
		Item.SetWeaponValues(3, 8);
		Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(0, 0, 1, 0));
	}

	private class JavelinThrown(int itemId) : ModProjectile
	{
		private readonly int itemId = itemId;

		public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Javelin/{new Item(itemId).ModItem.GetType().Name}";
	}
}