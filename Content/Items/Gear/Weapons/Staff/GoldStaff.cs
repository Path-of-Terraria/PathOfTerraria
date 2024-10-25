using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal class GoldStaff : Staff
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

		Item.shoot = ModContent.ProjectileType<GoldStaffProjectile>();
		Item.damage = 42;
		Item.knockBack = 3;
	}

	public class GoldStaffProjectile : StaffProjectile
	{
		public override int DustType => DustID.GemDiamond;
		public override int MaxCharge => 60;
		public override int TorchType => TorchID.White;

		public override void SetDefaults()
		{
			base.SetDefaults();

			Projectile.penetrate = 2;
			Projectile.Size = new Vector2(24);
		}
	}
}
