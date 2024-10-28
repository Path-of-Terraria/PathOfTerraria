using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal class LeadStaff : Staff
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 5;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.shoot = ModContent.ProjectileType<LeadStaffProjectile>();
		Item.damage = 34;
		Item.knockBack = 3;
	}

	public class LeadStaffProjectile : StaffProjectile
	{
		public override int DustType => DustID.GemSapphire;
		public override int MaxCharge => 90;
		public override int TorchType => TorchID.Blue;

		public override void SetDefaults()
		{
			base.SetDefaults();

			Projectile.penetrate = 2;
		}
	}
}
