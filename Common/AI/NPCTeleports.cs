using System.IO;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader.IO;

#nullable enable
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE2003 // Blank line required between block and subsequent statement

namespace PathOfTerraria.Common.AI;

internal sealed class TeleportData()
{
	// Config
	public float FarAwayDistance;
	public (ushort Start, ushort End) Disappear = (0, 12);
	public (ushort Start, ushort End) Reappear = (42, 54);
	public (ushort Start, ushort End) Invulnerability = (0, 45);
	public ushort MaxCooldown = 150;
	public (float Factor, ushort FlatBonus) CooldownDamage = (0f, 0);

	// State
	public int LastSeenHealth;
	public Vector2 TeleportSource;
	public Vector2 TeleportTarget;
	public short Progress = -1;
	public Counter<ushort> Cooldown;
	public bool IsBusy;
}

/// <summary> Implements combat teleportation behavior logic for NPCs. </summary>
internal sealed class NPCTeleports : NPCComponent<TeleportData>
{
	public ref struct Ctx(NPC npc)
	{
		public NPC NPC = npc;
		public Vector2 Center = npc.Center;
		public NPCMovement? Movement = npc.TryGetGlobalNPC(out NPCMovement c) ? c : null;
		public Vector2 TargetCenter = npc.TryGetGlobalNPC(out NPCTargeting c) ? c.GetTargetCenter(npc) : npc.GetTargetData().Center;
		public Rectangle TargetRect = npc.GetTargetData().Hitbox;
		//TODO: Create an action counter/tracking/interaction system to remove this type of coupling.
		public NPCAttacking? Attacking = npc.TryGetGlobalNPC(out NPCAttacking c) ? c : null;
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
		writer.WriteVector2(Data.TeleportTarget);
	}
	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
	{
		if (!Enabled) { return; }

		Data.Progress = reader.ReadInt16();
		Data.Cooldown.Value = reader.ReadUInt16();
		Data.TeleportTarget = reader.ReadVector2();
	}

	public bool TryStarting(in Ctx ctx)
	{
		NPC npc = ctx.NPC;

		if (Main.netMode == NetmodeID.MultiplayerClient) { return false; }
		if (Active || Data.IsBusy || Data.Cooldown.Value > 0) { return false; }
		if (ctx.Attacking?.Active == true) { return false; }

		bool farAway = npc.HasValidTarget && ctx.Center.DistanceSQ(ctx.TargetCenter) >= Data.FarAwayDistance * Data.FarAwayDistance;
		bool targetMayBeAttacking = npc.HasValidTarget
			&& npc.GetTargetData().Type == Terraria.Enums.NPCTargetType.Player
			&& Main.player[npc.target] is { } p && p.itemAnimation > 0
			&& p.direction == MathF.Sign(ctx.Center.X - p.Center.X);

		if ((!farAway && !targetMayBeAttacking) || !TryPickPosition(in ctx)) { return false; }

		Start(in ctx);

		return true;
	}

	public void Start(in Ctx ctx)
	{
		Data.Progress = 0;
		ctx.NPC.netUpdate = true;
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
		if (TryStarting(in ctx))
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

			float entrancePower = MathUtils.DistancePower(MathF.Abs(Data.Progress - Data.Disappear.End), 1, 20);
			float exitPower = MathUtils.DistancePower(MathF.Abs(Data.Progress - Data.Reappear.Start), 1, 20);

			if (entrancePower > 0f) { Lighting.AddLight(Data.TeleportSource, Color.OrangeRed.ToVector3() * entrancePower); }
			if (exitPower > 0f) { Lighting.AddLight(Data.TeleportTarget, Color.OrangeRed.ToVector3() * exitPower); }

			if (Data.Progress == 0)
			{
				npc.velocity.X = npc.direction * 2f;
				npc.velocity.Y = -2f;
				npc.noGravity = true;
			}

			npc.dontTakeDamage = Data.Progress >= Data.Invulnerability.Start && Data.Progress <= Data.Invulnerability.End;

			if (Data.Progress < Data.Disappear.End)
			{
				Data.TeleportSource = npc.Center;
			}
			else if (Data.Progress == Data.Disappear.End)
			{
				SoundEngine.PlaySound(SoundID.Shimmer1 with { Pitch = 0.4f, PitchVariance = 0.1f }, npc.Center);

				// Try to find a more up-to-date position.
				TryPickPosition(in ctx);
			}
			else if (Data.Progress == Data.Reappear.Start)
			{
				npc.noGravity = false;
				npc.Center = Data.TeleportTarget;
				npc.spriteDirection = npc.direction = (ctx.Center.X - ctx.TargetCenter.X) > 0f ? 1 : -1;
				npc.velocity.X = npc.direction * 2f;
				npc.velocity.Y = -5f;

				SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack with { Pitch = -0.2f, PitchVariance = 0.1f }, npc.Center);
			}

			Data.Progress++;

			if (Data.Progress >= Data.Reappear.End)
			{
				npc.alpha = 0;
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

		Point targetPoint = ctx.TargetCenter.ToTileCoordinates();
		Rectangle targetArea = new Rectangle(targetPoint.X, targetPoint.Y, 0, 0).Inflated(10, 6);
		var spawn = new SpawnPlacement
		{
			Area = targetArea,
			CollisionSize = npc.Size.ToPoint(),
			MinDistanceFromPlayers = 32f,
			MinDistanceFromEnemies = 64f,
			SkippedLiquids = LiquidMask.All,
			OnGround = true,
		};

		static bool IsSpawnPointSuitable(in Ctx ctx, Vector2 spawnPos)
		{
			// Must be significantly closer.
			if (spawnPos.DistanceSQ(ctx.TargetCenter) > 128 + ctx.Center.DistanceSQ(ctx.TargetCenter)) { return false; }

			// If in front of the target, then the portal exit should go behind it, or vice-versa.
			if (Math.Sign(ctx.Center.X - ctx.TargetCenter.X) == Math.Sign(spawnPos.X - ctx.TargetCenter.X)) { return false; }

			return true;
		}

		if (EnemySpawning.TryFindingSpawnPosition(spawn, out Vector2 spawnPos) && IsSpawnPointSuitable(in ctx, spawnPos))
		{
			Data.TeleportTarget = spawnPos;
			npc.netUpdate = true;
			return true;
		}

		return false;
	}
}
