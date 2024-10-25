using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal class PlatinumStaff : Staff
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 17;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.shoot = ModContent.ProjectileType<PlatinumStaffProjectile>();
		Item.damage = 58;
		Item.knockBack = 5;
	}

	public class PlatinumStaffProjectile : StaffProjectile
	{
		public override int DustType => DustID.GemDiamond;
		public override int MaxCharge => 120;
		public override int TorchType => TorchID.White;

		public override void SetDefaults()
		{
			base.SetDefaults();

			Projectile.penetrate = 3;
			Projectile.Size = new Vector2(24);
		}
	}
}
