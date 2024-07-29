using PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;
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
			bool zenithTarget = SwordCommon.GetZenithTarget(player, targetPosition, 400f, out int npcTargetIndex);

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
}
