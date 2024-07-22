using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class HamBat : VanillaClone
{
	protected override short VanillaItemId => ItemID.HamBat;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}

	public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
	{
		int multiplier = 0;

		if (player.FindBuffIndex(BuffID.WellFed3) != -1)
		{
			multiplier = 3;
		}
		else if (player.FindBuffIndex(BuffID.WellFed2) != -1)
		{
			multiplier = 2;
		}
		else if (player.FindBuffIndex(BuffID.WellFed) != -1)
		{
			multiplier = 1;
		}

		modifiers.FinalDamage += 0.05f * multiplier;
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.life <= 0)
		{
			player.AddBuff(BuffID.HeartyMeal, 7 * 60);
		}
	}
}