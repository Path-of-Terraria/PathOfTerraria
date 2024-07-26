using System.Reflection;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class BloodButcherer : VanillaClone
{
	protected override short VanillaItemId => ItemID.BloodButcherer;

	// Just about everything for the Blood Butcherer is private. This recreates everything.
	// The reflection should be perfectly performant; it's run once on use or on hit.

	private static MethodInfo vanillaTryButcher = null;
	private static FieldInfo vanillaButcherFlag = null;

	public override void Load()
	{
		base.Load();

		vanillaTryButcher = typeof(Player).GetMethod("BloodButcherer_TryButchering", BindingFlags.Instance | BindingFlags.NonPublic);
		vanillaButcherFlag = typeof(Player).GetField("_spawnBloodButcherer", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	public override void Unload()
	{
		base.Unload();

		vanillaTryButcher = null;
		vanillaButcherFlag = null;
	}

	public override void SetDefaults()
	{
		ItemType = Core.ItemType.Melee;
	}

	public override void HoldItem(Player player)
	{
		base.HoldItem(player);

		if (player.ItemAnimationJustStarted)
		{
			vanillaButcherFlag.SetValue(player, true);
		}
	}

	public override void MeleeEffects(Player player, Rectangle hitbox)
	{
		for (int j = 0; j < 2; j++)
		{
			float pointOnPath = 0.2f + 0.8f * Main.rand.NextFloat();
			Rectangle itemRectangle = SwordCommon.GetItemRectangle(player, Item);

			SwordCommon.GetPointOnSwungItemPath(player, 60f, 60f, pointOnPath, player.GetAdjustedItemScale(Item), out Vector2 location2, out Vector2 direction);
			Vector2 vector2 = direction.RotatedBy((float)Math.PI / 2f * player.direction * player.gravDir);
			Dust.NewDustPerfect(location2, 5, vector2 * 2f, 100, default, 0.7f + Main.rand.NextFloat() * 0.6f);

			if (Main.rand.NextBool(20))
			{
				int num6 = Dust.NewDust(new Vector2(itemRectangle.X, itemRectangle.Y), itemRectangle.Width, itemRectangle.Height, DustID.CrimtaneWeapons,
					player.velocity.X * 0.2f + player.direction * 3, player.velocity.Y * 0.2f, 140, default, 0.7f);

				Main.dust[num6].position = location2;
				Main.dust[num6].fadeIn = 1.2f;
				Main.dust[num6].noGravity = true;
				Main.dust[num6].velocity *= 0.25f;
				Main.dust[num6].velocity += vector2 * 5f;
			}
		}
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		vanillaTryButcher.Invoke(player, [target, Item, damageDone, hit.Knockback]);
	}
}