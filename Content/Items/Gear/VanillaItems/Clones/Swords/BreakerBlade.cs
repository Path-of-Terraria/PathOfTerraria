using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class BreakerBlade : VanillaClone
{
	protected override short VanillaItemId => ItemID.BreakerBlade;

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Core.ItemType.Melee;
	}

	public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (target.life > target.lifeMax * 0.9f)
		{
			modifiers.FinalDamage += 2f;
		}
	}
}
