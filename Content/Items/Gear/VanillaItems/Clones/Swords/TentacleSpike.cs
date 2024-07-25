using System.Reflection;
using PathOfTerraria.Common.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class TentacleSpike : VanillaClone
{
	protected override short VanillaItemId => ItemID.TentacleSpike;

	private static FieldInfo spikesMax5Field = null;

	private bool spawnSpikeFlag = false;

	public override void Load()
	{
		base.Load();

		spikesMax5Field = typeof(Player).GetField("_tentacleSpikesMax5", BindingFlags.Static | BindingFlags.NonPublic);
	}

	public override void Unload()
	{
		spikesMax5Field = null;
	}

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override bool? UseItem(Player player)
	{
		spawnSpikeFlag = true;
		return true;
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (!target.CanBeChasedBy() || !spawnSpikeFlag || Main.myPlayer != player.whoAmI)
		{
			return;
		}

		spawnSpikeFlag = false;

		Vector2 v = target.Center - player.MountedCenter;
		v = v.SafeNormalize(Vector2.Zero);
		Vector2 vector = target.Hitbox.ClosestPointInRect(player.MountedCenter) + v;
		Vector2 vector2 = (target.Center - vector) * 0.8f;
		int num = Projectile.NewProjectile(player.GetSource_OnHit(target), vector.X, vector.Y, vector2.X, vector2.Y, 971, damageDone, 0f, player.whoAmI, 1f, target.whoAmI);
		Main.projectile[num].StatusNPC(target.whoAmI);

		Projectile.KillOldestJavelin(num, 971, target.whoAmI, spikesMax5Field.GetValue(null) as Point[]);
	}
}
