using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.World.Generation;
using System.IO;
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

			case DevourerState.FlameAdds:
				FlameAddsAI();
				break;
		}
	}

	private void FlameAddsAI()
	{
		Timer++;

		if (Timer < 80)
		{
			NPC.Opacity = 1 - Timer / 80f;
		}

		if (Timer > 600)
		{
			NPC.Opacity = (Timer - 600) / 80f;
		}

		if (Timer > 680)
		{
			SetState(DevourerState.ReturnToIdle);
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

			Vector2 target = IdleSpot + new Vector2(side * 1555, 525);
			NPC.velocity += (target - NPC.Center).SafeNormalize(Vector2.Zero) * 2.2f;
			NPC.velocity *= 0.9f;

			if (NPC.DistanceSQ(target) < 40 * 40)
			{
				bezier = Spline.InterpolateXY([target, IdleSpot - new Vector2(0, 500), IdleSpot + new Vector2(-side * 1800, 525)], 12);

				Timer = 1;
				MiscData = 0;
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

			if (Timer % 2 == 0 && Main.myPlayer != NetmodeID.MultiplayerClient && swingAround == 0)
			{
				var vel = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(2) + 5);
				int type = ModContent.ProjectileType<SunDevourerEruptionProjectile>();
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
					}
				}
			}
		}
	}

	private void ReturnToIdleAI()
	{
		if (NPC.DistanceSQ(IdleSpot) < 20 * 20)
		{
			NPC.velocity *= 0.85f;
			Timer++;

			if (Timer > 120)
			{
				SetState(DevourerState.Firefall);
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
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		base.ReceiveExtraAI(reader);

		MiscData = reader.ReadSingle();
		AdditionalData = reader.ReadSingle();
	}
}