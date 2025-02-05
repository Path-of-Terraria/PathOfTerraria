namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal class CopperStaff : Staff
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.shoot = ModContent.ProjectileType<CopperStaffProjectile>();
	}

	public class CopperStaffProjectile : StaffProjectile
	{
		public override Vector2 ChargeOffset => base.ChargeOffset - new Vector2(0f, 4f);
	}
}
