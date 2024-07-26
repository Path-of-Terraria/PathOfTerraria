using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class Flymeal : VanillaClone
{
	protected override short VanillaItemId => ItemID.Flymeal;

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Core.ItemType.Melee;
	}

	public override bool? CanHitNPC(Player player, NPC target)
	{
		return target.isLikeATownNPC ? true : base.CanHitNPC(player, target);
	}

	public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (target.isLikeATownNPC)
		{
			modifiers.ScalingArmorPenetration += 1f;

			if (target.type == NPCID.Nurse)
			{
				modifiers.FinalDamage += 1;
			}
		}
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		target.AddBuff(BuffID.Stinky, 5 * 60);
	}
}
