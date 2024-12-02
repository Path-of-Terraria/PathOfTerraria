using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Content.Items.Consumables.Maps;
using SubworldLibrary;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class EoWPortal : ModProjectile, ISaveProjectile
{
	private ref float Uses => ref Projectile.ai[1];
	private ref float MaxUses => ref Projectile.ai[2];

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(20, 48);
		Projectile.Opacity = 0.5f;
		Projectile.netImportant = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		if (NPC.downedBoss1)
		{
			Projectile.Kill();

			for (int i = 0; i < 20; ++i)
			{
				Vector2 vel = new Vector2(-Main.rand.NextFloat(4, 8), 0).RotatedBy(Projectile.rotation);
				Dust.NewDustPerfect(Projectile.Center + new Vector2(8, Main.rand.NextFloat(-16, 16)), DustID.PurpleTorch, vel);
			}

			return;
		}

		Projectile.timeLeft++;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
		Projectile.velocity *= 0.96f;

		if (MaxUses == 0)
		{
			MaxUses = Map.GetBossUseCount();
		}

		if (Main.rand.NextBool(14))
		{
			Vector2 vel = new Vector2(-Main.rand.NextFloat(4, 8), 0).RotatedBy(Projectile.rotation);
			Dust.NewDustPerfect(Projectile.Center + new Vector2(8, Main.rand.NextFloat(-16, 16)), DustID.PurpleTorch, vel);
		}

		Lighting.AddLight(Projectile.Center, TorchID.Purple);

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.Hitbox.Intersects(Projectile.Hitbox) && Main.myPlayer == player.whoAmI)
			{
				SubworldSystem.Enter<EaterDomain>();

				Projectile.ai[1]++;
				Projectile.netUpdate = true;

				if (Projectile.ai[1] > Projectile.ai[2])
				{
					Projectile.Kill();
				}
			}
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(Projectile.rotation);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		Projectile.rotation = reader.ReadSingle();
	}

	public void SaveData(TagCompound tag)
	{
		tag.Add("uses", Uses);
		tag.Add("rotation", Projectile.rotation);
	}

	public void LoadData(TagCompound tag, Projectile projectile)
	{
		projectile.ai[1] = tag.GetFloat("uses");
		projectile.rotation = tag.GetFloat("rotation");
	}
}
