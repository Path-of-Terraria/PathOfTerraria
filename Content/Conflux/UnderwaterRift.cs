using System;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

#nullable enable
#pragma warning disable IDE2003 // Blank line required between block and subsequent statement

namespace PathOfTerraria.Content.Conflux;

/// <summary>
/// An underwater rift used in the Fishron quest.
/// </summary>
internal sealed class UnderwaterRift : ConfluxRift
{
	private DamageInstance? playerDamage;
	private DamageInstance? npcDamage;
	private uint tickCounter;
	private float activationPower;
	private bool calledEnterSubworld;

	public override ConfluxRiftKind Kind => ConfluxRiftKind.Glacial;

	public override bool CanInteract() { return false; }
	public override bool CountsAsActiveBattle() { return false; }

	public override void AI()
	{
		base.AI();

		OpeningAnimation = 0.55f;
		ClosingAnimation = 0.05f;
		ApproachAnimation = 1.0f;
		Projectile.scale = 1f;

		if (Activated)
		{
			activationPower = MathF.Min(1f, activationPower + (TimeSystem.LogicDeltaTime / 3.5f));
		}

		PullEntities();
		MainPortalLogic();
		DamageEntities();
	}

	protected override Encounter CreateEncounter(uint lengthInSeconds)
	{
		return default;
	}

	private void DamageEntities()
	{
		const int damageTickRate = 30;

		if (++tickCounter % damageTickRate == 0)
		{
			for (int i = 0; i < 2; i++)
			{
				bool isPlayerDamage = i == 0;
				(isPlayerDamage ? ref playerDamage : ref npcDamage) = new DamageInstance(playerDamage)
				{
					Damage = isPlayerDamage ? 30 : 50,
					Knockback = 3f,
					Hitter = Projectile,
					Aabb = Projectile.getRect(),
					Direction = Vector2.UnitY,
					Filter = isPlayerDamage ? EntityKind.LocalPlayer : EntityKind.NPC,
					DeathReason = p => PlayerDeathReason.ByCustomReason(NetworkText.FromKey(this.GetLocalizationKey($"DeathReason.{Main.rand.Next(3)}"), p.name)),
				};
			}
		}

		if (!Activated) { playerDamage?.DamageEntities(); }

		npcDamage?.DamageEntities();
	}

	private void MainPortalLogic()
	{
		Vector2 center = Projectile.Center;
		Rectangle hitbox = Projectile.Hitbox;
		bool activated = false;

		void HandleActivator(Entity entity)
		{
			// Strong pull.
			PullPush(center, entity.Center, ref entity.velocity, (32f, 512f, 2f), (7.5f, 35f), default);

			if (Main.netMode != NetmodeID.MultiplayerClient && !activated && entity.Hitbox.Intersects(hitbox))
			{
				entity.active = false;
				Activate();
				activated = true;
			}
		}

		foreach (Item item in Main.ActiveItems)
		{
			if (item.type != ItemID.TruffleWorm) { continue; }

			HandleActivator(item);
		}
		foreach (Projectile proj in Main.ActiveProjectiles)
		{
			if (!proj.bobber) { continue; }
			if (!proj.TryGetOwner(out Player owner)) { continue; }
			if (owner.GetFishingConditions() is not { Bait.type: ItemID.TruffleWorm }) { continue; }

			HandleActivator(proj);
		}

		if (Activated && Main.netMode != NetmodeID.Server && Main.LocalPlayer is { } player && player.Hitbox.Intersects(hitbox))
		{
			if (!calledEnterSubworld && activationPower >= 0.75f)
			{
				SubworldSystem.Enter<FishronDomain>();
				calledEnterSubworld = true;
			}
		}
	}

	private void PullEntities()
	{
		Vector2 center = Projectile.Center;

		(float Min, float Max, float Pow) defRange = (32f, 400f, 3f);
		(float Min, float Max, float Pow) npcRange = (64f, 700f, 3f);
		(float Min, float Max, float Pow) playerRange = (32f, MathHelper.Lerp(400f, 1000f, activationPower), MathHelper.Lerp(3f, 2f, activationPower));
		(float Speed, float Accel) playerPull = (MathHelper.Lerp(10f, 20f, activationPower), MathHelper.Lerp(4f, 15f, activationPower));
		(float Speed, float Accel) playerPush = (MathHelper.Lerp(5f, 0f, activationPower), MathHelper.Lerp(2f, 0f, activationPower));

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) { continue; }

			if (NPCID.Sets.ProjectileNPC[npc.type]) { continue; }

			PullPush(center, npc.Center, ref npc.velocity, npcRange, (15f, 8f), (15f, 7f));
		}
		foreach (Player player in Main.ActivePlayers)
		{
			PullPush(center, player.Center, ref player.velocity, playerRange, playerPull, playerPush);
		}
		foreach (Item item in Main.ActiveItems)
		{
			PullPush(center, item.Center, ref item.velocity, defRange, (7.5f, 5f), (10f, 2.5f));
		}
		foreach (Gore gore in Main.gore.AsSpan(0, Main.maxGore))
		{
			if (!gore.active || !gore.sticky) { continue; }

			PullPush(center, gore.position, ref gore.velocity, defRange, (5f, 4.5f), (5f, 3.5f));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void PullPush(Vector2 origin, Vector2 center, ref Vector2 velocity, (float Min, float Max, float Pow) range, (float Speed, float Accel) pull, (float Speed, float Accel) push)
	{
		Vector2 diff = origin - center;
		float distance = diff.Length();
		float pullPower = MathF.Pow(MathUtils.DistancePower(distance, range.Min, range.Max), range.Pow);
		float pushPower = MathUtils.DistancePower(distance, range.Min, range.Max);
		Vector2 direction = distance > 0f ? diff / distance : Vector2.UnitX;
		Vector2 sideDir = direction.RotatedBy(MathHelper.PiOver2);

		Vector2 velocityCopy = velocity;
		velocity = MovementUtils.DirAccel(velocity, direction, pull.Speed, pullPower * pull.Accel * TimeSystem.LogicDeltaTime);
		velocity = MovementUtils.DirAccel(velocity, sideDir, push.Speed, pushPower * push.Accel * TimeSystem.LogicDeltaTime);

		// Don't prevent jumping.
		if (velocityCopy.Y == 0f && MathF.Abs(velocity.Y) < 0.1f)
		{
			velocity.Y = 0f;
		}
	}

	// Currently unused.
	private void FillWater()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) { return; }

		Rectangle worldRect = Projectile.Hitbox.Inflated(0, 3 * 16);
		Point xy = worldRect.TopLeft().ToTileCoordinates();
		Point zw = worldRect.BottomRight().ToTileCoordinates();
		int x1 = Math.Max(0, Math.Min(xy.X, Main.maxTilesX - 1)); 
		int x2 = Math.Max(0, Math.Min(zw.X, Main.maxTilesX - 1));
		int y1 = Math.Max(0, Math.Min(xy.Y, Main.maxTilesY - 1));
		int y2 = Math.Max(0, Math.Min(zw.Y, Main.maxTilesY - 1));

		for (int x = x1; x < x2; x++)
		{
			for (int y = y1; y < y2; y++)
			{
				Tile tile = Main.tile[x, y];

				if (WorldUtilities.SolidTile(tile)) { continue; }

				tile.LiquidType = LiquidID.Water;
				tile.LiquidAmount = 255;
				Liquid.AddWater(x, y);
			}
		}
	}
}
