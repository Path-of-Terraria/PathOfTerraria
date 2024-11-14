﻿using PathOfTerraria.Common.NPCs.OverheadDialogue;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Subworlds.BossDomains.BoCDomain;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class CrimsonMaw : ModProjectile
{
	public int Target
	{
		get => (int)Projectile.ai[0];
		set => Projectile.ai[0] = value;
	}

	public bool TargettingPlayer => Projectile.ai[1] == 0;

	private ref float Timer => ref Projectile.ai[2];

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 9;
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 45;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(100, 100);
		Projectile.Opacity = 0.5f;
		Projectile.netImportant = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		const int EatenWaitTime = 220;

		if (Projectile.timeLeft == 1)
		{
			Projectile.timeLeft++;
			Projectile.hide = true;
			Timer++;

			if (Timer >= EatenWaitTime)
			{
				if (TargettingPlayer && Main.myPlayer == Target)
				{
					Main.player[Target].GetModPlayer<PersistentReturningPlayer>().ReturnPosition = Main.player[Target].Center;
					//SubworldSystem.Enter<BrainDomain>();
					Projectile.Kill();
				}
			}

			if (!TargettingPlayer)
			{
				((IOverheadDialogueNPC)(Main.npc[Target].ModNPC as LloydNPC)).CurrentDialogue = null;
			}

			return;
		}

		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.1f);
		Projectile.velocity *= 0.96f;
		Projectile.frame = 8 - (int)(Projectile.timeLeft / 5f);

		if (TargettingPlayer)
		{
			Main.player[Target].GetModPlayer<CrimsonMawPlayer>().StopTime = 2;
		}
		else
		{
			Main.npc[Target].velocity = Vector2.Zero;
		}

		if (Projectile.timeLeft == 16)
		{
			for (int i = 0; i < 30; ++i)
			{
				Vector2 vel = new Vector2(-Main.rand.NextFloat(4, 8), 0).RotatedByRandom(MathHelper.Pi);
				Dust.NewDustPerfect(Projectile.Center + new Vector2(8, Main.rand.NextFloat(-16, 16)), DustID.Crimson, vel);
			}

			SoundEngine.PlaySound(SoundID.Item89 with { PitchRange = (-0.8f, -0.5f) }, Projectile.Center);
			SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);

			if (TargettingPlayer)
			{
				Main.player[Target].GetModPlayer<CrimsonMawPlayer>().Time = 16 + EatenWaitTime;

				if (Main.player[Target].mount.Active)
				{
					Main.player[Target].QuickMount();
				}
			}
			else
			{
				Main.npc[Target].hide = true;
			}
		}
	}

	//public override bool PreDraw(ref Color lightColor)
	//{
	//	Texture2D tex = TextureAssets.Projectile[Type].Value;

	//	for (int i = 0; i < 3; ++i)
	//	{
	//		float rotation = Projectile.rotation * (i % 2 == 0 ? -1 : 1);
	//		Vector2 position = Projectile.Center - Main.screenPosition;
	//		Color color = lightColor * ((3 - i) * 0.2f) * Projectile.Opacity;
	//		Main.spriteBatch.Draw(tex, position, null, color, rotation, tex.Size() / 2f, 1f - i * 0.2f, SpriteEffects.None, 0);
	//	}

	//	return false;
	//}
}
