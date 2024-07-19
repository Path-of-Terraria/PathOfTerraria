using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class Zenith : VanillaClone
{
	protected override short VanillaItemId => ItemID.Zenith;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		int useTimeFactor = (player.itemAnimationMax - player.itemAnimation) / player.itemTime;
		int swordType = FinalFractalHelper.GetRandomProfileIndex();

		if (useTimeFactor == 0)
		{
			swordType = ItemID.Zenith;
		}

		Vector2 targetPosition = Main.MouseWorld;
		player.LimitPointToPlayerReachableArea(ref targetPosition);
		Vector2 direction = targetPosition - player.MountedCenter;

		if (useTimeFactor == 1 || useTimeFactor == 2)
		{
			bool zenithTarget = GetZenithTarget(targetPosition, 400f, out int npcTargetIndex);

			if (zenithTarget)
			{
				direction = Main.npc[npcTargetIndex].Center - player.MountedCenter;
			}

			if (useTimeFactor == 2 || useTimeFactor == 1 && !zenithTarget)
			{
				direction += Main.rand.NextVector2Circular(150f, 150f);
			}
		}

		float offset = Main.rand.Next(-100, 101);
		Projectile.NewProjectile(source, position, direction / 2f, type, damage, knockback, player.whoAmI, offset, swordType);

		return false;
	}

	/// <summary>
	/// Copied from vanilla, as the base <see cref="Player.GetZenithTarget"/> method is private.
	/// </summary>
	private bool GetZenithTarget(Vector2 searchCenter, float maxDistance, out int npcTargetIndex)
	{
		npcTargetIndex = 0;
		int? index = null;
		float currentDistance = maxDistance;

		for (int i = 0; i < 200; i++)
		{
			NPC nPC = Main.npc[i];
			if (nPC.CanBeChasedBy(this))
			{
				float num3 = searchCenter.Distance(nPC.Center);
				if (!(currentDistance <= num3))
				{
					index = i;
					currentDistance = num3;
				}
			}
		}

		if (!index.HasValue)
		{
			return false;
		}

		npcTargetIndex = index.Value;
		return true;
	}
}
