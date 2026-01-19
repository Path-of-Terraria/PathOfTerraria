//#define NEVER_ATTACK

using System.IO;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

#nullable enable
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE2003 // Blank line required between block and subsequent statement

namespace PathOfTerraria.Common.AI;

internal sealed class AttackingData()
{
	// Config
	public bool DisableContactDamage = true;
	public bool ManualInitiation = false;
	public bool InitiateOnlyOnGround = false;
	public Vector2 InitiationRange = new(100f, 250f);
	public ushort LengthInTicks = 60;
	public ushort NoGravityLength = 15;
	public ushort CooldownLength = 60;
	public (float Slide, float Friction) Movement = (+0.4f, 0.9f);
	public (ushort Start, ushort End, EntityKind Filter) Damage = (20, 35, DamageInstance.EnemyAttackFilter);
	public (ushort Start, ushort End, Vector2 Velocity) Dash = (20, 35, new(10f, 5f));
	public (Point16 Size, Vector2 Extent, Vector2 Offset) Hitbox = (new(56, 56), new(24f, 32f), new(+12f, +2f));
	public ((Vector2 Min, Vector2 Max) Value, (float Min, float Max) Charge) AimLag = ((new(0.0f), new(0.2f)), (0f, 0.99f));
	public (ushort Tick, SoundStyle Style)[] Sounds = [(0, SoundID.Item71)];
	public Asset<Texture2D>? SlashTexture;

	// State
	public float Angle;
	public short Progress = -1;
	public Counter<ushort> Cooldown;
	public DamageInstance? Attack;
}

/// <summary> Implements charged melee attack behavior logic for NPCs. </summary>
internal sealed class NPCAttacking : NPCComponent<AttackingData>
{
	public ref struct Context(NPC npc)
	{
		public NPC NPC = npc;
		public Vector2 Center = npc.Center;
		public NPCTargeting Tracking = npc.GetGlobalNPC<NPCTargeting>();
		public NPCMovement? Movement = npc.TryGetGlobalNPC(out NPCMovement c) ? c : null;
		public Vector2 TargetCenter = npc.GetGlobalNPC<NPCTargeting>().GetTargetCenter(npc);
		public Rectangle TargetRect = npc.GetTargetData().Hitbox;
	}

	public struct Result()
	{
		public bool Initiated;
		public bool Ended;
	}

	public int Sign => Direction.X >= 0f ? 1 : -1;
	public bool Active => Data.Progress >= 0;
	public bool DealingDamage => Data.Progress >= 0 && Data.Progress >= Data.Damage.Start && Data.Progress < Data.Damage.End;
	public Vector2 Direction
	{
		get => Data.Angle.ToRotationVector2();
		set => Data.Angle = value.ToRotation();
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter writer)
	{
		if (!Enabled) { return; }

		writer.Write((short)Data.Progress);
		writer.Write((ushort)Data.Cooldown.Value);
	}
	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
	{
		if (!Enabled) { return; }

		Data.Progress = reader.ReadInt16();
		Data.Cooldown.Value = reader.ReadUInt16();
	}

	public override void PostDraw(NPC npc, SpriteBatch sb, Vector2 screenPos, Color drawColor)
	{
		if (!Enabled) { return; }

		if (DealingDamage && Data.SlashTexture is { IsLoaded: true, Value: { } texture })
		{
			DrawSlash(new(npc), sb, texture, screenPos, drawColor);
		}
	}

	public bool TryStarting(in Context ctx)
	{
#if NEVER_ATTACK
		return false;
#endif

		NPC npc = ctx.NPC;

		if (Data.Progress >= 0 || Data.Cooldown.Value != 0)
		{
			return false;
		}

		Vector2 targetDiff = ctx.TargetCenter - ctx.Center;
		if (MathF.Abs(targetDiff.X) > Data.InitiationRange.X || MathF.Abs(targetDiff.Y) > Data.InitiationRange.Y)
		{
			return false;
		}

		if (!Collision.CanHitLine(npc.position, npc.width, npc.height, ctx.TargetRect.TopLeft(), ctx.TargetRect.Width, ctx.TargetRect.Height))
		{
			return false;
		}

		Start(in ctx);

		return true;
	}

	public void Start(in Context ctx)
	{
		_ = ctx;
		Data.Progress = 0;
	}

	public Result ManualUpdate(in Context ctx)
	{
		NPC npc = ctx.NPC;
		var result = new Result();

		//TODO: Implement in a way that prevents spawn-time damage.
		if (Data.DisableContactDamage)
		{
			npc.damage = 0;
		}

		// Cool down cooldowns.
		Data.Cooldown.CountDown();

		Vector2 targetDiff = ctx.TargetCenter - ctx.Center;

		// Reset mutated variables.
		npc.noGravity = Data.NoGravityLength != 0 ? ContentSamples.NpcsByNetId[npc.type].noGravity : npc.noGravity;

		// Check if we can initiate an attack.
		if (!Data.ManualInitiation && (!Data.InitiateOnlyOnGround || npc.velocity.Y == 0f) && TryStarting(in ctx))
		{
			result.Initiated = true;
		}

		// Update attacks.
		if (Data.Progress >= 0)
		{
			if (ctx.Movement is { } movement)
			{
				movement.Data.NoAccelerationTime.SetIfGreater(1);
				movement.Data.NoFrictionTime.SetIfGreater(Data.Progress >= Data.Dash.Start ? (ushort)1 : (ushort)0);
			}

			// Play sounds.
			if (!Main.dedServ)
			{
				foreach ((ushort tick, SoundStyle style) in Data.Sounds)
				{
					if (Data.Progress == tick) { SoundEngine.PlaySound(style, ctx.Center); }
				}
			}

			if (Data.Progress < Data.Damage.Start)
			{
				// Update Attack direction.
				Direction = targetDiff.SafeNormalize(Vector2.UnitX * Sign);

				// Aim starts to update slower and slower.
				float aimLagFactor = MathUtils.Clamp01(((Data.Progress / (float)Data.Damage.Start) - Data.AimLag.Charge.Min) / (Data.AimLag.Charge.Max - Data.AimLag.Charge.Min));
				ctx.Tracking.AimLag = Vector2.Lerp(Data.AimLag.Value.Min, Data.AimLag.Value.Max, aimLagFactor);

				// Slow-slide.
				npc.velocity.X = Data.Movement.Slide * Sign;
			}
			else if (Data.Progress == Data.Damage.Start)
			{
				// Dash.
				ctx.NPC.velocity = Direction * Data.Dash.Velocity;

				// Prepare damage.
				Data.Attack = new DamageInstance
				{
					Aabb = default,
					Direction = default,
					Damage = npc.defDamage,
					Knockback = 10f,
					Filter = Data.Damage.Filter,
					Predicate = e => e is not NPC n || n.type != npc.type,
					DeathReason = _ => PlayerDeathReason.ByNPC(npc.whoAmI),
					ExcludedEntity = npc,
					HitEntities = Data.Attack?.HitEntities,
				};
				Data.Attack.HitEntities?.Clear();
			}
			else
			{
				// Slow down.
				npc.velocity.X *= Data.Movement.Friction;
				// But also defy gravity for a few ticks.
				npc.noGravity = Data.Progress >= Data.Damage.Start && Data.Progress < (Data.Damage.Start + Data.NoGravityLength);
			}

			// Force direction.
			npc.direction = Sign;

			// Deal damage starting with the dash.
			if (Data.Attack != null && Data.Progress >= Data.Damage.Start && Data.Progress < Data.Damage.End)
			{
				(Vector2 hitboxCenter, Rectangle hitboxAabb) = GetDamageArea(in ctx);
				Data.Attack.Aabb = hitboxAabb;
				Data.Attack.Direction = Direction;
				Data.Attack.DamageEntities();
			}

			Data.Progress++;

			// End attack, start cooldown.
			if (Data.Progress >= Data.LengthInTicks)
			{
				Data.Progress = -1;
				Data.Cooldown.Set(Data.CooldownLength);
				ctx.Tracking.AimLag = default;
				result.Ended = true;
			}
		}

		return result;
	}

	public (Vector2 Center, Rectangle Aabb) GetDamageArea(in Context ctx)
	{
		(Point16 Size, Vector2 Extent, Vector2 Offset) = Data.Hitbox;
		Vector2 center = ctx.NPC.Center + new Vector2(Offset.X * Sign, Offset.Y) + (Direction * Extent);
		Rectangle aabb = new Rectangle((int)center.X, (int)center.Y, 0, 0).Inflated(Size.X / 2, Size.Y / 2);

		return (center, aabb);
	}

	public void DrawSlash(in Context ctx, SpriteBatch sb, Texture2D texture, Vector2 screenPos, Color lightColor)
	{
		// First column is the diffuse, second is the glowmask.
		var baseFrame = new SpriteFrame(2, (byte)(texture.Height / (texture.Width / 2))) { PaddingX = 0, PaddingY = 0 };
		byte frameIndex = (byte)Math.Min((baseFrame.RowCount - 1), Math.Floor((Data.Progress - Data.Damage.Start) / (float)(Data.Damage.End - Data.Damage.Start) * baseFrame.RowCount));
		baseFrame = baseFrame.With(0, Sign > 0 ? frameIndex : (byte)((baseFrame.RowCount - 1) - frameIndex));

		Vector2 center = GetDamageArea(in ctx).Center;
		Rectangle srcRect = baseFrame.GetSourceRectangle(texture);
		Rectangle dstRect = new((int)(center.X), (int)(center.Y), srcRect.Width, srcRect.Height);
		dstRect.X -= (int)screenPos.X;
		dstRect.Y -= (int)screenPos.Y;
		Vector2 origin = srcRect.Size() * 0.5f;

		sb.Draw(texture, dstRect, srcRect, lightColor, Data.Angle, origin, 0, 0f);
		sb.Draw(texture, dstRect, srcRect with { X = srcRect.Width }, Color.White, Data.Angle, origin, 0, 0f);
	}
}
