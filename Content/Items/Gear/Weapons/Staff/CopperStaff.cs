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
	}
}
