using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal class IronStaff : Staff
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

		Item.shoot = ModContent.ProjectileType<IronStaffProjectile>();
		Item.damage = 24;
		Item.knockBack = 1.2f;
	}

	public class IronStaffProjectile : StaffProjectile
	{
		public override int DustType => DustID.GemTopaz;
		public override int TorchType => TorchID.Yellow;
	}
}
