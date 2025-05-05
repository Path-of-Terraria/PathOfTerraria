using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.World.Generation;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

public sealed partial class SunDevourerNPC : ModNPC
{
	public override void AI()
	{
		base.AI();

		ConstantTimer++;

		NPC.TargetClosest();
		NPC.rotation = (NPC.velocity.X - NPC.velocity.Y) * 0.015f;

		if (Math.Abs(NPC.velocity.X) > 0.1f)
		{
			NPC.spriteDirection = Math.Sign(NPC.velocity.X);
		}

		switch (State)
		{
			case DevourerState.Trapped:
				TrappedAI();
				break;

			case DevourerState.ReturnToIdle:
				ReturnToIdleAI();
				break;

			case DevourerState.Firefall:
				FirefallAI();
				break;

			case DevourerState.LightningAdds:
				LightningAdds();
				break;

			case DevourerState.BallLightning:
				BallLightningAI();
				break;

			case DevourerState.AbsorbSun:
				AbsorbSunAI();
				break;

			case DevourerState.Godrays:
				GodrayAI();
				break;
		}
	}

	private void GodrayAI()
	{
		const float DustWidth = 140;

		Timer++;
		NPC.velocity = Vector2.Zero;

		if (Timer < 120)
		{
			NPC.Opacity = 1f;

			if (Timer % 2 == 0)
			{
				Vector2 dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(-DustWidth, DustWidth), Main.rand.NextFloat(-60, 60));
				Dust.NewDustPerfect(dustPos, DustID.AncientLight, new Vector2(0, -6).RotatedByRandom(0.1f));
			}
		}
		else if (Timer == 120)
		{
			if (Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 15; ++i)
				{
					Vector2 dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(-DustWidth, DustWidth), Main.rand.NextFloat(-80, 80));
					Dust.NewDustPerfect(dustPos, DustID.AncientLight, new Vector2(0, -6).RotatedByRandom(0.1f), Scale: Main.rand.NextFloat(1, 2));
				}

				for (int i = 0; i < 8; ++i)
				{
					int count = Main.rand.Next(5, 9);
					Vector2 basePos = NPC.Center + new Vector2(MathHelper.Lerp(-DustWidth, DustWidth, i / 7f), Main.rand.NextFloat(-60, 60));
					float magnitude = Main.rand.NextFloat(2, 12);

					for (int j = 0; j < count; ++j)
					{
						float factor = (j + 1) / (float)count;
						Dust.NewDustPerfect(basePos, DustID.AncientLight, new Vector2(0, -magnitude * factor - 4).RotatedByRandom(0.1f));
					}
				}

				SoundEngine.PlaySound(new SoundStyle("PathOfTerraria/Assets/Sounds/LightDisappear") with { PitchRange = (-0.1f, 0.1f) }, NPC.Center);
			}

			NPC.dontTakeDamage = true;
			NPC.ShowNameOnHover = false;
			NPC.Opacity = 0f;
		}
		else if (Timer <= 300)
		{
			NPC.Opacity = 0f;

			if (Timer > 240)
			{
				NPC.Opacity = (Timer - 240) / 60f;
				NPC.ShowNameOnHover = true;
				NPC.dontTakeDamage = false;
			}

			if (Timer < 240 && Timer % 12 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				var position = new Vector2(Target.Center.X + Main.rand.Next(-20, 20), FloorY);
				int damage = ModeUtils.ProjectileDamage(80, 110, 150);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), position, new Vector2(0, -18), ModContent.ProjectileType<Lightray>(), damage, 0, Main.myPlayer);
			}
		}
		else if (Timer > 400)
		{
			SetState(DevourerState.ReturnToIdle);
		}
	}

	private void AbsorbSunAI()
	{
		Timer++;
		NPC.velocity = Vector2.Zero;
		SunDevourerSunEdit.Blackout = 1 - Timer / 300f;

		if (Timer > 300)
		{
			NightStage = true;
			SetState(DevourerState.ReturnToIdle);
		}
	}

	private void BallLightningAI()
	{
		Timer++;

		ref float ballSlot = ref AdditionalData;

		if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			var vel = new Vector2(NPC.direction * 4, 0);
			int type = ModContent.ProjectileType<BallLightning>();
			ballSlot = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, type, ModeUtils.ProjectileDamage(80, 110, 150), 0, Main.myPlayer);

			NPC.netUpdate = true;
			NPC.velocity -= vel;

			doDamage = true;
		}

		Projectile ball = Main.projectile[(int)ballSlot];

		if (Timer < 20)
		{
			NPC.velocity *= 0.95f;
		}

		if (Timer > 20)
		{
			ref float subTimer = ref MiscData;
			subTimer++;

			if (subTimer == 5)
			{
				addedPos = Main.rand.NextBool() ? Target.Center : Target.Center + Target.Center.DirectionFrom(ball.Center) * 60;
			}
			else if (subTimer > 5 && subTimer < 90)
			{
				NPC.velocity += NPC.DirectionTo(addedPos);
				NPC.position -= Target.velocity * 0.33f;
			}
			else if (subTimer > 100)
			{
				subTimer = 0;
			}

			if (NPC.velocity.LengthSquared() > 22 * 22)
			{
				NPC.velocity = Vector2.Normalize(NPC.velocity) * 22;
			}
		}

		float exitTime = NPC.life < NPC.lifeMax * 0.67f ? BallLightning.LifeTime * 0.7f : BallLightning.LifeTime;
		
		if (Timer > exitTime - 30)
		{
			NPC.velocity *= 0.9f;
		}

		if (Timer >= exitTime)
		{
			MiscData = 0;
			ballSlot = 0;
			SetState(DevourerState.ReturnToIdle);
			doDamage = false;
		}
	}

	private void LightningAdds()
	{
		const int EndTimer = 380;

		Timer++;

		if (Timer < 40)
		{
			NPC.Opacity = 1 - Timer / 40f;
		}

		if (Timer == 1)
		{
			NPC.dontTakeDamage = true;
			NPC.ShowNameOnHover = false;
		}
		else if (Timer == EndTimer)
		{
			NPC.dontTakeDamage = false;
			NPC.ShowNameOnHover = true;
		}

		if (Timer is 100 or 180 or 260 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			Vector2 pos = NPC.Center + new Vector2(Main.rand.Next(-500, 500), Main.rand.Next(1200, 1400));
			Vector2 glassPos = FindGlass(IdleSpot - new Vector2(0, 600));
			Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<LightningBurst>(), 50, 4, Main.myPlayer, 0, glassPos.X, glassPos.Y);
			//NPC.NewNPC(NPC.GetSource_FromAI(), (int)pos.X, (int)pos.Y, ModContent.NPCType<WormLightning>(), 0, 1, glassPos.X, glassPos.Y);
		}

		if (Timer > EndTimer - 40)
		{
			NPC.Opacity = (Timer - (EndTimer - 40)) / 40f;
		}

		if (Timer > EndTimer)
		{
			SetState(DevourerState.ReturnToIdle);
		}
	}

	public static Vector2 FindGlass(Vector2 basePos, float widthVariance = 200, float heightVariance = 120)
	{
		int reps = 0;

		while (true)
		{
			Vector2 pos = basePos + new Vector2(Main.rand.NextFloat(-widthVariance, widthVariance), Main.rand.NextFloat(-heightVariance, heightVariance));
			Point16 tilePos = pos.ToTileCoordinates16();
			reps++;
			
			if (Main.tile[tilePos].HasTile && Main.tile[tilePos].TileType == TileID.Glass || reps > 30000)
			{
				return pos;
			}
		} 
	}

	private void FirefallAI()
	{
		if (Timer == 0f)
		{
			// Local rename for readability, will repeat many times
			ref float side = ref MiscData;

			if (side != -1 && side != 1)
			{
				side = Main.rand.NextBool() ? -1 : 1;
				NPC.netUpdate = true;
			}

			Vector2 target = IdleSpot + new Vector2(side * 1580, 525);
			NPC.velocity += (target - NPC.Center).SafeNormalize(Vector2.Zero) * 2.2f;
			NPC.velocity *= 0.9f;

			if (NPC.DistanceSQ(target) < 40 * 40)
			{
				bezier = Spline.InterpolateXY([target, IdleSpot - new Vector2(side * 320, 750), IdleSpot + new Vector2(-side * 1800, 525)], 12);

				Timer = 1;
				MiscData = 0;

				doDamage = true;
			}
		}
		else if (Timer >= 1)
		{
			ref float index = ref MiscData;
			ref float swingAround = ref AdditionalData;

			float speed = swingAround == 1 ? 1.6f : 1f;
			float maxSpeed = swingAround == 1 ? 28 : 24;

			NPC.velocity += (bezier[(int)index] - NPC.Center).SafeNormalize(Vector2.Zero) * speed;

			if (NPC.velocity.LengthSquared() > maxSpeed * maxSpeed)
			{
				NPC.velocity = Vector2.Normalize(NPC.velocity) * maxSpeed;
			}

			Timer++;

			if (Timer % 2 == 0 && Main.myPlayer != NetmodeID.MultiplayerClient && swingAround == 0 && CanDropProjOnPlayer())
			{
				var vel = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(2) + 5);
				int type = ModContent.ProjectileType<DevourerFireball>();
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, type, 80, 0, Main.myPlayer, 0, 0, FloorY);
			}

			if (Timer % 15 == 0)
			{
				index++;

				if (index >= bezier.Length)
				{
					index = 0;

					if (swingAround == 0)
					{
						bezier = Spline.InterpolateXY([NPC.Center, IdleSpot + new Vector2(0, 500), IdleSpot], 7);
						Timer = 1;
						swingAround = 1;
					}
					else
					{
						index = 0;
						swingAround = 0;
						SetState(DevourerState.ReturnToIdle);

						doDamage = false;
					}
				}
			}
		}
	}

	private bool CanDropProjOnPlayer()
	{
		foreach (Player player in Main.ActivePlayers)
		{
			if (Math.Abs(player.Center.X - NPC.Center.X) < 500)
			{
				return true;
			}
		}

		return false;
	}

	private void ReturnToIdleAI()
	{
		if (MiscData == 0 && !NightStage)
		{
			if (maxGlassCount == 0)
			{
				glassCount = maxGlassCount = CountGlass();
			}
			else
			{
				glassCount = CountGlass();
			}

			MiscData = 1;

			if (glassCount < maxGlassCount * 0.4f)
			{
				MiscData = 0;
				SetState(DevourerState.AbsorbSun);
			}
		}

		if (NPC.DistanceSQ(IdleSpot) < 20 * 20)
		{
			NPC.velocity *= 0.85f;
			Timer++;

			if (Timer > 120)
			{
				MiscData = 0;

				if (!NightStage)
				{
					SetState(Main.rand.NextBool() ? DevourerState.BallLightning : DevourerState.Firefall);

					if (!NightStage && ConstantTimer > 60 * 20)
					{
						SetState(DevourerState.LightningAdds);
						ConstantTimer = 0;
					}
				}
				else
				{
					SetState(DevourerState.Godrays);
				}
			}
		}
		else
		{
			NPC.velocity += (IdleSpot - NPC.Center).SafeNormalize(Vector2.Zero) * 0.5f;

			if (NPC.velocity.LengthSquared() > 15 * 15)
			{
				NPC.velocity = Vector2.Normalize(NPC.velocity) * 15;
			}

			NPC.velocity *= 0.96f;
		}
	}

	private int CountGlass()
	{
		Point16 basePos = (IdleSpot - new Vector2(0, 600)).ToTileCoordinates16();
		int count = 0;

		for (int i = basePos.X - 20; i < basePos.X + 20; ++i)
		{
			for (int j = basePos.Y - 10; j < basePos.Y + 10; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.HasTile && tile.TileType == TileID.Glass)
				{
					count++;
				}
			}
		}

		return count;
	}

	private void TrappedAI()
	{
		bool anyPlayerFar = false;

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.DistanceSQ(NPC.Center) > 300 * 300)
			{
				anyPlayerFar = true;
				break;
			}
		}

		if (!anyPlayerFar)
		{
			SetState(DevourerState.ReturnToIdle);

			NPC.dontTakeDamage = false;
			NPC.boss = true;
			NPC.GetGlobalNPC<ArenaEnemyNPC>().Arena = true;
			NPC.GetGlobalNPC<ArenaEnemyNPC>().StillDropStuff = true;
		}
	}

	public void SetState(DevourerState state)
	{
		State = state;
		Timer = 0;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		base.SendExtraAI(writer);

		writer.Write(MiscData);
		writer.Write(AdditionalData);
		writer.WriteVector2(addedPos);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		base.ReceiveExtraAI(reader);

		MiscData = reader.ReadSingle();
		AdditionalData = reader.ReadSingle();
		addedPos = reader.ReadVector2();
	}
}