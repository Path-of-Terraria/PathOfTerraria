using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.World.Generation;
using System.IO;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

public sealed partial class SunDevourerNPC : ModNPC
{
	public override void AI()
	{
		base.AI();

		NPC.TargetClosest();

		switch (State)
		{
			case DevourerState.Trapped:
				TrappedAI();
				break;

			case DevourerState.Rise:
				RiseAI();
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

			if (side is not -1 or 1)
			{
				side = Main.rand.NextBool() ? -1 : 1;
				NPC.netUpdate = true;
			}

			Vector2 target = IdleSpot + new Vector2(side * 1555, 525);
			NPC.velocity = (target - NPC.Center).SafeNormalize(Vector2.Zero) * 14;

			if (NPC.DistanceSQ(target) < 20 * 20)
			{
				bezier = Spline.InterpolateXY([target, IdleSpot - new Vector2(0, 300), IdleSpot + new Vector2(-side * 1555, 525)], 9);

				Timer = 1;
				MiscData = 0;
			}
		}
		else if (Timer == 1)
		{
			ref float index = ref MiscData;

			NPC.velocity += (bezier[(int)index] - NPC.Center).SafeNormalize(Vector2.Zero) * 1f;

			if (NPC.velocity.LengthSquared() > 15 * 15)
			{
				NPC.velocity = Vector2.Normalize(NPC.velocity) * 15;
			}

			if (NPC.DistanceSQ(bezier[(int)index]) < 40 * 40)
			{
				index++;

				if (index >= bezier.Length)
				{
					index = 0;
					SetState(DevourerState.ReturnToIdle);
				}
			}
		}
	}

	private void ReturnToIdleAI()
	{
		ref float factor = ref MiscData;
		factor = MathHelper.Lerp(factor, NPC.DistanceSQ(IdleSpot) < 120 * 120 ? 0 : 1f, 0.1f);

		Main.NewText(factor);

		NPC.velocity = (IdleSpot - NPC.Center) * 0.05f * factor;
		Timer++;

		if (Timer > 180)
		{
			SetState(DevourerState.Firefall);
		}
	}

	private void RiseAI()
	{
		Timer++;

		if (Timer < 40)
		{
			NPC.velocity.Y -= 0.3f;
		}
		else
		{
			NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0, 0.1f);
		}

		if (NPC.velocity.Y > -0.05f)
		{
			SetState(DevourerState.ReturnToIdle);
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
			SetState(DevourerState.Rise);

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
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		base.ReceiveExtraAI(reader);

		MiscData = reader.ReadSingle();
	}
}