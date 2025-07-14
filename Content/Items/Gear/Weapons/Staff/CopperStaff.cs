namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal class CopperStaff : Staff
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.shoot = ModContent.ProjectileType<CopperStaffProjectile>();
		Item.value = Item.buyPrice(0, 0, 0, 10);
	}

	public class CopperStaffProjectile : StaffProjectile
	{
		public override Vector2 ChargeOffset => base.ChargeOffset - new Vector2(0f, 4f);
	}
}
