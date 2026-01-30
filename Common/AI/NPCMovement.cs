using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;

#nullable enable

namespace PathOfTerraria.Common.AI;

internal struct MovementInput()
{
	public Vector2 MovementVector;
	public Vector2 JumpVector;
}

internal sealed class MovementData()
{
	public struct PushCfg()
	{
		public (float Closest, float Farthest) Range = (6f, 32f);
		public (float Target, float Acceleration) Speed = (2f, 0.5f);
		public int RequiredNpcType = -1;
		public int IgnoredNpcType = -1;
		public bool ApplyOnlyOnGround = true;
	}

	// Config
	public PushCfg Push = default;
	public float MaxSpeed = 3f;
	public float Acceleration = 16f;
	public (float Ground, float Air) Friction = (8f, 2f);

	// State
	public Counter<ushort> NoAccelerationTime;
	public Counter<ushort> NoFrictionTime;
	public MovementInput? InputOverride;
	public Vector2? TargetOverride;
	public Vector2 LastTargetPoint;

	public void ResetOverrides()
	{
		InputOverride = null;
	}
}

/// <summary> Implements generalized movement logic for NPCs. </summary>
internal sealed class NPCMovement : NPCComponent<MovementData>
{
	public ref struct Ctx(NPC npc)
	{
		public NPC NPC = npc;
		public Vector2 Center = npc.Center;
		public NPCTargeting? Targeting { get; } = npc.TryGetGlobalNPC(out NPCTargeting c) ? c : null;
		public NPCNavigation? Navigation { get; } = npc.TryGetGlobalNPC(out NPCNavigation c) ? c : null;
	}

	public void ManualUpdate(in Ctx ctx)
	{
		if (!Enabled) { return; }

		NPC npc = ctx.NPC;

		// Push against peers.
		Push(in ctx);

		// Friction.
		if (Data.NoFrictionTime.Value > 0)
		{
			Data.NoFrictionTime.CountDown();
			float friction = npc.velocity.Y == 0f ? Data.Friction.Ground : Data.Friction.Air;
			npc.velocity.X = MathUtils.StepTowards(npc.velocity.X, 0f, friction * TimeSystem.LogicDeltaTime);
		}

		// Slopes.
		if (npc.velocity.Y == 0f) { Collision.StepDown(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY); }

		Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

		if ((!npc.HasValidTarget && !Data.TargetOverride.HasValue) || Data.NoAccelerationTime.Value > 0)
		{
			Data.NoAccelerationTime.CountDown();
			Data.ResetOverrides();
			return;
		}

		MovementInput input;

		if (Data.InputOverride.HasValue)
		{
			input = Data.InputOverride.Value;
		}
		else if (ctx.Navigation != null)
		{
			Vector2 targetCenter = Data.LastTargetPoint = Data.TargetOverride ?? ctx.Targeting?.GetTargetCenter(npc) ?? npc.GetTargetData(false).Center;

			ctx.Navigation.Process(out NPCNavigation.Result navResult, new()
			{
				NPC = npc,
				TargetPosition = targetCenter,
			});

			input = new()
			{
				MovementVector = navResult.MovementVector,
				JumpVector = navResult.JumpVector,
			};
		}
		else
		{
			return;
		}

		// Horizontal acceleration.
		if (npc.velocity.Y == 0f && input.MovementVector.X != 0f)
		{
			npc.velocity.X = MathUtils.StepTowards(npc.velocity.X, Data.MaxSpeed * input.MovementVector.X, Data.Acceleration * TimeSystem.LogicDeltaTime);
			npc.direction = input.MovementVector.X > 0f ? 1 : -1;
		}

		// Jumping.
		if (input.JumpVector != default)
		{
			npc.velocity = input.JumpVector;
			npc.direction = input.JumpVector.X != 0f ? (input.JumpVector.X > 0f ? 1 : -1) : npc.direction;
		}
	}

	private void Push(in Ctx ctx)
	{
		NPC npc = ctx.NPC;
		ref readonly MovementData.PushCfg push = ref Data.Push;

		//if (!ctx.PushOthersRatherThanSelf && ctx.ApplyOnlyOnGround && ctx.Velocity.Y != 0f) { return; }

		float minSqrDst = push.Range.Farthest * push.Range.Farthest;
		if (minSqrDst <= 0f) { return; }

		foreach (NPC other in Main.ActiveNPCs)
		{
			if (other.whoAmI == npc.whoAmI || other.type == push.IgnoredNpcType) { continue; }
			if ((push.RequiredNpcType >= 0 && other.type != push.RequiredNpcType)) { continue; }
			//if (ctx.PushOthersRatherThanSelf && ctx.ApplyOnlyOnGround && other.velocity.Y != 0f) { continue; }

			Vector2 otherCenter = other.Center;
			Vector2 otherDiff = ctx.Center - otherCenter;

			float sqrDst = otherDiff.LengthSquared();
			if (sqrDst > minSqrDst) { continue; }
			float dst = MathF.Sqrt(sqrDst);

			float power = MathUtils.DistancePower(dst, push.Range.Closest, push.Range.Farthest);
			Vector2 direction = dst > 0f ? (otherDiff / dst) : (-Vector2.UnitX * npc.direction);

			ref Vector2 affectedVel = ref npc.velocity;

			//if (ctx.PushOthersRatherThanSelf)
			//{
			//	affectedVel = ref other.velocity;
			//	direction *= -1;
			//}

			affectedVel = MovementUtils.DirAccelQ(affectedVel, direction, push.Speed.Target, push.Speed.Acceleration);
		}
	}
}
