// #define DEBUG_LOG
// #define DEBUG_GIZMOS

using System.Diagnostics;
using System.IO;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

#nullable enable
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE2003 // Blank line required between block and subsequent statement

namespace PathOfTerraria.Common.AI;

internal enum TeleportType
{
	None,
	Immediate,
	Interpolated,
}

internal sealed class TeleportData()
{
	// Config
	public ushort MaxCooldown = 150;
	public bool TurnInvisible;
	public bool DisableGravity;
	public Vector3 LightColor;
	public TeleportType Movement = TeleportType.Immediate;
	public (ushort Start, ushort End) Disappear = (0, 12);
	public (ushort Start, ushort End) Reappear = (42, 54);
	public (ushort Start, ushort End) Invulnerability = (0, 45);
	public (float Factor, ushort FlatBonus) CooldownDamage = (0f, 0);
	public (Vector2? Disappear, Vector2? Reappear) Velocity;
	public (ushort Tick, SoundStyle Style)? DisappearSound = (0, SoundID.Shimmer1);
	public (ushort Tick, SoundStyle Style)? ReappearSound = (0, SoundID.DD2_DarkMageAttack);
	// Triggers
	public bool TriggerIfEndangered;
	public (float Min, float Max)? TriggerAtDistance;
	// Placement
	public bool PlaceOriginAtTarget = false;
	public bool RequireReachablePoint = false;
	public bool? RequireDifferentDirection = false;
	public bool? RequirePlacementBehind = null;
	public bool? RequireLineOfSightOnTrigger = null;
	public bool? RequireLineOfSightOnExit = null;
	public (float Min, float Max)? RequiredTargetDistance = null;
	public (float Min, float Max)? RequiredTargetDistanceDiff = null;
	public SpawnPlacement BasePlacement = new()
	{
		Area = new(default, default, 64, 64),
		CollisionSize = default,
		MinDistanceFromPlayers = 32f,
		MinDistanceFromEnemies = 64f,
		SkippedLiquids = LiquidMask.All,
		OnGround = true,
	};

	// State
	public int LastSeenHealth;
	public Vector2 TeleportSource;
	public Vector2 TeleportTarget;
	public Vector2 StoredVelocity;
	public short Progress = -1;
	public Counter<ushort> Cooldown;
}

/// <summary> Implements combat teleportation behavior logic for NPCs. </summary>
internal sealed class NPCTeleports : NPCComponent<TeleportData>
{
	public ref struct Ctx(NPC npc)
	{
		public NPC NPC = npc;
		public Vector2 Center = npc.Center;
		public NPCMovement? Movement = npc.TryGetGlobalNPC(out NPCMovement c) ? c : null;
		public Vector2 TargetCenter = npc.TryGetGlobalNPC(out NPCTargeting c) ? c.GetTargetCenter(npc) : npc.GetTargetData(false).Center;
		public NPCAimedTarget Target = npc.GetTargetData(false);
		//TODO: Create an action counter/tracking/interaction system to remove this type of coupling.
		public NPCAttacking? Attacking = npc.TryGetGlobalNPC(out NPCAttacking c) ? c : null;

		public bool IsBusy;
	}
	public struct Result()
	{
		public bool Initiated;
		public bool Ended;
	}

	public bool Active => Data.Progress >= 0;

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter writer)
	{
		if (!Enabled) { return; }

		writer.Write((short)Data.Progress);
		writer.Write((ushort)Data.Cooldown.Value);
		if (Active)
		{
			writer.WriteVector2(Data.TeleportSource);
			writer.WriteVector2(Data.TeleportTarget);
			writer.WriteVector2(Data.StoredVelocity);
		}
	}
	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
	{
		if (!Enabled) { return; }

		Data.Progress = reader.ReadInt16();
		Data.Cooldown.Value = reader.ReadUInt16();
		if (Active)
		{
			Data.StoredVelocity = reader.ReadVector2();
			Data.TeleportTarget = reader.ReadVector2();
			Data.StoredVelocity = reader.ReadVector2();
		}
	}

	private bool StartBehaviors(in Ctx ctx)
	{
		NPC npc = ctx.NPC;

		bool HasReason(in Ctx ctx)
		{
			// Trigger at specific distances from the target.
			if (Data.TriggerAtDistance is { } range
			&& npc.HasValidTarget
			&& ctx.Center.DistanceSQ(ctx.TargetCenter) is float sqrDst
			&& MathUtils.IntersectsRangeSqr(range, sqrDst))
			{
				DebugLog("NPCTeleports: Within trigger distance.");
				return true;
			}

			// Trigger if our target may be initiating an attack on us.
			if (Data.TriggerIfEndangered
			&& npc.HasValidTarget && npc.GetTargetData().Type == Terraria.Enums.NPCTargetType.Player
			&& Main.player[npc.target] is { } p && p.itemAnimation > 0
			&& p.direction == MathF.Sign(ctx.Center.X - p.Center.X))
			{
				DebugLog("NPCTeleports: Endangered.");
				return true;
			}

			DebugLog("NPCTeleports: No reason to teleport.");
			
			return false;
		}

		if (!HasReason(in ctx)) { return false; }

		// Check line of sight if needed.
		if (Data.RequireLineOfSightOnTrigger is bool req
		&& req != Collision.CanHitLine(npc.position, npc.width, npc.height, ctx.Target.Position, ctx.Target.Width, ctx.Target.Height))
		{
			DebugLog("NPCTeleports: No line of sight on trigger.");
			return false;
		}

		return TryStarting(in ctx);
	}

	public bool TryStarting(in Ctx ctx, bool bypassCooldowns = false)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) { return false; }
		if (Active) { return false; }
		if (ctx.IsBusy) { return false; }
		if (!bypassCooldowns && (ctx.Attacking?.Active == true || Data.Cooldown.Value > 0)) { return false; }
		if (!TryPickPosition(in ctx)) { return false; }

		Data.Progress = 0;
		ctx.NPC.netUpdate = true;

		return true;
	}

	public Result ManualUpdate(in Ctx ctx)
	{
		NPC npc = ctx.NPC;
		var result = new Result();

		// Track health.
		int healthDiff = Data.LastSeenHealth - npc.life;
		Data.LastSeenHealth = npc.life;

		// Cool down cooldowns.
		if (Data.Cooldown.Value > 0)
		{
			Data.Cooldown.CountDown();

			// Reduce the cooldown further when losing health.
			if (Math.Max(0, healthDiff) is > 0 and int lifeLost)
			{
				Data.Cooldown.Value = (ushort)Math.Max(0, Data.Cooldown.Value - ((int)(lifeLost * Data.CooldownDamage.Factor) + Data.CooldownDamage.FlatBonus));
			}

			return result;
		}

		// Try initiating teleportation.
		if (StartBehaviors(in ctx))
		{
			result.Initiated = true;
		}

		if (Data.Progress >= 0)
		{
			if (ctx.Movement is { } movement)
			{
				movement.Data.NoAccelerationTime.SetIfGreater(1);
				movement.Data.NoFrictionTime.SetIfGreater(1);
			}

			if (ctx.Attacking is { } attacking)
			{
				attacking.Data.Cooldown.Set(2);
			}

			if (Data.Progress == 0)
			{
				Data.StoredVelocity = npc.velocity;
				if (Data.Velocity.Disappear is { } vel) { npc.velocity = vel * new Vector2(npc.direction, 1f); }

				npc.noGravity = Data.DisableGravity | npc.noGravity;
			}

			if (!Main.dedServ)
			{
				// Play audio.
				if (Data.DisappearSound is { } snd1 && Data.Progress == snd1.Tick) { SoundEngine.PlaySound(snd1.Style, npc.Center); }
				if (Data.ReappearSound is { } snd2 && Data.Progress == snd2.Tick) { SoundEngine.PlaySound(snd2.Style, npc.Center); }

				// Shine lights.
				float entrancePower = MathUtils.DistancePower(MathF.Abs(Data.Progress - Data.Disappear.End), 1, 20);
				float exitPower = MathUtils.DistancePower(MathF.Abs(Data.Progress - Data.Reappear.Start), 1, 20);
				if (Data.Movement is TeleportType.Interpolated)
				{
					Lighting.AddLight(ctx.Center, Data.LightColor * MathF.Max(entrancePower, exitPower));
				}
				else
				{
					if (entrancePower > 0f) { Lighting.AddLight(Data.TeleportSource, Data.LightColor * entrancePower); }
					if (exitPower > 0f) { Lighting.AddLight(Data.TeleportTarget, Data.LightColor * exitPower); }
				}
			}

			npc.dontTakeDamage = Data.Progress >= Data.Invulnerability.Start && Data.Progress <= Data.Invulnerability.End;

			if (Data.Progress < Data.Disappear.End)
			{
				Data.TeleportSource = npc.Center;
			}
			else if (Data.Progress == Data.Disappear.End)
			{
				npc.alpha = Data.TurnInvisible ? byte.MaxValue : npc.alpha;
				// Try to find a more up-to-date position.
				TryPickPosition(in ctx);
			}
			else if (Data.Progress == Data.Reappear.Start)
			{
				npc.alpha = Data.TurnInvisible ? 0 : npc.alpha;
				npc.noGravity = Data.DisableGravity ? ContentSamples.NpcsByNetId[npc.type].noGravity : npc.noGravity;
				npc.spriteDirection = npc.direction = (ctx.Center.X - ctx.TargetCenter.X) > 0f ? 1 : -1;

				if (Data.Movement == TeleportType.Immediate) { npc.Center = Data.TeleportTarget; }

				// Restore or reset velocity.
				npc.velocity = (Data.Velocity.Reappear is { } vel) ? (vel * new Vector2(npc.direction, 1f)) : Data.StoredVelocity;
			}

			if (Data.Movement is TeleportType.Interpolated
			&& Data.Progress >= Data.Disappear.End && Data.Progress <= Data.Reappear.Start)
			{
				static float Easing(float x)
				{
					return (x < 0.5f) ? (4f * x * x * x) : (1f - MathF.Pow((-2f * x) + 2f, 2f) / 2f);
				}

				float moveStep = (Data.Progress - Data.Disappear.End) / (float)(Data.Reappear.Start - Data.Disappear.End);
				if (Data.Progress >= Data.Reappear.Start) { moveStep = 1f; }
				moveStep = Easing(moveStep);
				npc.Center = Vector2.Lerp(Data.TeleportSource, Data.TeleportTarget, moveStep);
				npc.velocity = Data.Progress < Data.Reappear.Start ? default : npc.velocity;
			}

			Data.Progress++;

			if (Data.Progress >= Data.Reappear.End)
			{
				result.Ended = true;
				Data.Cooldown.Set(Data.MaxCooldown);
				Data.Progress = -1;
				npc.dontTakeDamage = false;
			}
		}

		return result;
	}

	private bool TryPickPosition(in Ctx ctx)
	{
		NPC npc = ctx.NPC;

		Vector2 center = ctx.Center;
		NPCAimedTarget target = ctx.Target;
		Vector2 targetCenter = ctx.TargetCenter;
		Point originPoint = (Data.PlaceOriginAtTarget ? targetCenter : center).ToTileCoordinates();
		Func<Point16, bool>? basePredicate = Data.BasePlacement.CustomPredicate;

		bool IsSpawnPointSuitable(Point16 tilePos)
		{
			Vector2 spawnPos = tilePos.ToWorldCoordinates();

			// Check if this is within the required distance from the target.
			if (Data.RequiredTargetDistance is { } rtd
			&& !MathUtils.IntersectsRangeSqr(rtd, spawnPos.DistanceSQ(targetCenter)))
			{
				TileGizmo(tilePos, Color.Red);
				return false;
			}

			// Check if this is sufficiently closer to or farther from the target relative to the previous position.
			if (Data.RequiredTargetDistanceDiff is { } rtdd
			&& !MathUtils.IntersectsRangeSqr(rtdd, MathF.Abs(spawnPos.DistanceSQ(targetCenter) - center.DistanceSQ(targetCenter))))
			{
				TileGizmo(tilePos, Color.Bisque);
				return false;
			}

			// Preference for whether to swap the relative direction towards the target.
			if (Data.RequireDifferentDirection is bool mustSwap
			&& (Math.Sign(center.X - targetCenter.X) != Math.Sign(spawnPos.X - targetCenter.X)) is bool swapped
			&& mustSwap != swapped)
			{
				TileGizmo(tilePos, Color.Yellow);
				return false;
			}

			// Preference for whether to teleport in front or behind the target.
			if (Data.RequirePlacementBehind is bool mustBeBehind
			&& npc.GetTargetEntity() is Entity targetEntity
			&& (((spawnPos.X - targetEntity.Center.X) >= 0 ? 1 : -1) != targetEntity.direction) is bool isBehind
			&& mustBeBehind != isBehind)
			{
				TileGizmo(tilePos, Color.BlueViolet);
				return false;
			}

			if (Data.RequireLineOfSightOnExit is bool req
			&& req != Collision.CanHitLine(spawnPos, 1, 1, target.Position, target.Width, target.Height))
			{
				TileGizmo(tilePos, Color.Purple);
				return false;
			}

			return basePredicate?.Invoke(tilePos) != false;
		}

		var areaSize = Data.BasePlacement.Area.Size().ToPoint();
		var areaRect = new Rectangle(originPoint.X - areaSize.X / 2, originPoint.Y - areaSize.Y / 2, areaSize.X, areaSize.Y);
		areaRect.X = Math.Max(0, areaRect.X);
		areaRect.Y = Math.Max(0, areaRect.Y);
		areaRect.Width = Math.Min(Main.maxTilesX - 1, areaRect.X + areaRect.Width) - areaRect.X;
		areaRect.Height = Math.Min(Main.maxTilesY - 1, areaRect.Y + areaRect.Height) - areaRect.Y;

		SpawnPlacement spawn = Data.BasePlacement with
		{
			Area = areaRect,
			AreaOrigin = Data.RequireReachablePoint ? originPoint.ToPoint16() : null,
			CollisionSize = npc.Size.ToPoint(),
			CustomPredicate = IsSpawnPointSuitable,
		};

		if (EnemySpawning.TryFindingSpawnPosition(out Vector2 spawnPos, spawn))
		{
			Data.TeleportTarget = spawnPos;
			npc.netUpdate = true;
			return true;
		}

		return false;
	}

	private static void DebugLog(string msg)
	{
#if DEBUG_LOG
		Main.chatMonitor.NewText(msg, R: Color.DarkCyan.R, G: Color.DarkCyan.G, B: Color.DarkCyan.B);
#endif
	}
	private static void TileGizmo(Point16 tilePos, Color color)
	{
#if DEBUG_GIZMOS
		var worldPos = tilePos.ToWorldCoordinates(0, 0).ToPoint();
		var gizmoRect = new Rectangle(worldPos.X, worldPos.Y, 16, 16);
		DebugUtils.DrawRectInWorld(gizmoRect, color);
#endif
	}
}
