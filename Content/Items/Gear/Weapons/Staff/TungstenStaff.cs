using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal class TungstenStaff : Staff
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 12;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.shoot = ModContent.ProjectileType<TungstenStaffProjectile>();
		Item.damage = 47;
		Item.knockBack = 4;
	}

	public class TungstenStaffProjectile : StaffProjectile
	{
		public override int DustType => DustID.GemRuby;
		public override int TorchType => TorchID.Red;
		public override int MaxCharge => 100;

		public override void SetDefaults()
		{
			base.SetDefaults();

			Projectile.penetrate = 3;
		}
	}
}
