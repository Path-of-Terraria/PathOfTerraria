namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal class CopperStaff : Staff
{
	protected override int StaffType => ModContent.ProjectileType<CopperStaffHeld>();

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.shoot = ModContent.ProjectileType<CopperStaffProjectile>();
	}

	public class CopperStaffProjectile : StaffProjectile
	{
	}

	public class CopperStaffHeld : StaffHeldProjectile
	{
	}
}
