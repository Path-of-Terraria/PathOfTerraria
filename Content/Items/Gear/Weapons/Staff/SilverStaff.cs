using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal class SilverStaff : Staff
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

		Item.shoot = ModContent.ProjectileType<SilverStaffProjectile>();
		Item.damage = 30;
		Item.knockBack = 2;
	}

	public class SilverStaffProjectile : StaffProjectile
	{
		public override int DustType => DustID.GemEmerald;
		public override int MaxCharge => 70;
		public override int TorchType => TorchID.Green;
	}
}
