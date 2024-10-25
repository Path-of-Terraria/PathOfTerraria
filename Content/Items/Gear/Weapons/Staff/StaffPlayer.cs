namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal class StaffPlayer : ModPlayer
{
	public bool Empowered => EmpoweredStaffTime > 0;

	public int EmpoweredStaffTime = 0;

	public override void ResetEffects()
	{
		EmpoweredStaffTime = Math.Max(0, EmpoweredStaffTime - 1);
	}
}
