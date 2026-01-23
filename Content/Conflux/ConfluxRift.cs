using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

internal enum ConfluxRiftKind : byte
{
	Glacial,
	Infernal,
	Celestial,
	Count,
}

internal sealed class GlacialRift : ConfluxRift
{
	public override ConfluxRiftKind Kind => ConfluxRiftKind.Glacial;
}

internal sealed class InfernalRift : ConfluxRift
{
	public override ConfluxRiftKind Kind => ConfluxRiftKind.Infernal;
}

internal sealed class CelestialRift : ConfluxRift
{
	public override ConfluxRiftKind Kind => ConfluxRiftKind.Celestial;
}

/// <summary> Used for spawn quotas. </summary>
internal record struct EnemyRole
{
	public float Fodder;
	public float Heavy;
	public float Boss;

	public readonly bool CanPay(in EnemyRole price)
	{
		return Fodder >= price.Fodder && Heavy >= price.Heavy && Boss >= price.Boss;
	}

	public bool TryPay(in EnemyRole price)
	{
		if (CanPay(in price))
		{
			Fodder -= price.Fodder;
			Heavy -= price.Heavy;
			Boss -= price.Boss;
			return true;
		}

		return false;
	}
}

internal abstract class ConfluxRift : ModProjectile, IRightClickableProjectile
{
	[Flags]
	public enum Flags : int
	{
		Activated = 1,
		Closing = 2,
	}

	private record struct VisualParams(int DustId, Color ColorBase, Color lightA, Color lightB, string? Filter);

	private static SoundStyle SoundActivation => new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/RiftActivation")
	{
		Volume = 0.9f,
		PauseBehavior = PauseBehavior.PauseWithGame,
	};
	private static SoundStyle SoundDeactivation => new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/RiftDeactivation")
	{
		Volume = 0.9f,
		PauseBehavior = PauseBehavior.PauseWithGame,
	};
	private static SoundStyle SoundApproach => new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/RiftApproach")
	{
		Volume = 0.4f,
		PitchVariance = 0.1f,
		PauseBehavior = PauseBehavior.PauseWithGame,
	};
	private static SoundStyle SoundWithdraw => new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/RiftWithdraw")
	{
		Volume = 0.4f,
		PitchVariance = 0.1f,
		PauseBehavior = PauseBehavior.PauseWithGame,
	};
	private static SoundStyle SoundInactive => new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/RiftInactive")
	{
		Volume = 0.4f,
		IsLooped = true,
		PauseBehavior = PauseBehavior.PauseWithGame,
	};
	private static SoundStyle SoundApproached => new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/RiftApproached")
	{
		Volume = 0.5f,
		IsLooped = true,
		PauseBehavior = PauseBehavior.PauseWithGame,
	};
	private static SoundStyle SoundActive => new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/RiftActive")
	{
		Volume = 0.22f,
		IsLooped = true,
		PauseBehavior = PauseBehavior.PauseWithGame,
	};

	private static Asset<Effect>? shader;

	private (SlotId Handle, float Volume) soundDormant;
	private (SlotId Handle, float Volume) soundRousing;
	private (SlotId Handle, float Volume) soundAwoken;
	private ProjectileAudioTracker? audioTracker;
	private bool approached;

	public uint EndTime { get; set; }
	public Encounter Encounter { get; private set; }
	public float OpeningAnimation { get; protected set; }
	public float ClosingAnimation { get; protected set; }
	public float ProgressAnimation { get; protected set; }
	public float ApproachAnimation { get; protected set; }

	/// <summary> The rift's type. </summary>
	public abstract ConfluxRiftKind Kind { get; }
	/// <summary> The rift's state flags. </summary>
	public Flags BitFlags
	{
		get => Unsafe.BitCast<float, Flags>(Projectile.ai[1]);
		set => Projectile.ai[1] = Unsafe.BitCast<Flags, float>(value);
	}
	/// <summary> The synchronized progress of the rift's encounter. </summary>
	public ref float Progress => ref Projectile.ai[2];
	/// <summary> Whether the rift has been opened. </summary>
	public bool Activated => (BitFlags & Flags.Activated) != 0;
	/// <summary> Whether the rift is about to disappear. </summary>
	public bool Closing => (BitFlags & Flags.Closing) != 0;

	public override string Texture => (GetType().Namespace + "." + nameof(ConfluxRift)).Replace('.', '/');

	public virtual bool CanInteract()
	{
		return true;
	}
	public virtual bool CountsAsActiveBattle()
	{
		return Activated;
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.IsInteractable[Type] = true;

		if (!Main.dedServ)
		{
			shader = Mod.Assets.Request<Effect>($"Assets/Effects/ConfluxRift");
		}
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(120, 120);
		Projectile.Opacity = 0f;
		Projectile.hide = true;
		Projectile.netImportant = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override bool PreAI()
	{
		// Despawn only after the closing animation finishes.
		if (!Closing || ClosingAnimation < 1f)
		{
			Projectile.timeLeft++;
		}

		return base.PreAI();
	}

	public override void AI()
	{
		Vector2 center = Projectile.Center;
		VisualParams visuals = GetVisualParameters();

		Projectile.rotation += 0.05f;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.25f);
		ProgressAnimation = MathHelper.Lerp(ProgressAnimation, Progress, 0.1f);

		if (Closing)
		{
			float oldValue = ClosingAnimation;
			ClosingAnimation = MathF.Min(1f, ClosingAnimation + (TimeSystem.LogicDeltaTime / 4f));
			ApproachAnimation = 0f;

			// Effects.
			if (oldValue <= 0f)
			{
				if (!Main.dedServ && visuals.Filter != null) { Filters.Scene.Deactivate(visuals.Filter); }
			}
			// Item drop.
			else if (oldValue < 0.6f && ClosingAnimation >= 0.6f)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					DropRewards();
				}

				if (!Main.dedServ)
				{
					for (int i = 0; i < 50; i++)
					{
						Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), visuals.DustId, Main.rand.NextVector2Circular(8f, 8f));
					}
				}
			}
		}
		else if (Activated)
		{
			OpeningAnimation = MathF.Min(1f, OpeningAnimation + (TimeSystem.LogicDeltaTime / 7f));
			ApproachAnimation = 1f;
		}
		else
		{
			if (ApproachAnimation is <= 0f or >= 1f)
			{
				(Player? closestPlayer, float minSqrDist) = (null, float.PositiveInfinity);
				foreach (Player player in Main.ActivePlayers)
				{
					(closestPlayer, minSqrDist) = (player, MathF.Min(minSqrDist, player.DistanceSQ(center)));
				}

				Vector2 compareSpot = Main.LocalPlayer.Center;
				bool isInteractible = closestPlayer?.IsProjectileInteractibleAndInInteractionRange(Projectile, ref compareSpot) == true && CanInteract();

				if (isInteractible != approached)
				{
					approached = isInteractible;
					SoundEngine.PlaySound(approached ? SoundApproach : SoundWithdraw, center);
				}
			}

			float targetApproach = approached ? 1f : 0f;
			float oldValue = ApproachAnimation;
			ApproachAnimation = MathHelper.Lerp(ApproachAnimation, targetApproach, 0.05f);
			ApproachAnimation = MathUtils.StepTowards(ApproachAnimation, targetApproach, 0.01f);
		}

		// Encounter logic.
		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			// Synchronize the progress to clients.
			if (Activated && Encounter.IsValid)
			{
				UpdateProgress();
			}

			if (Activated && !Closing && OpeningAnimation >= 0.1f)
			{
				// Create the encounter once enough time has passed.
				if (Encounter == default)
				{
					const uint lengthInSeconds = 60;
					const uint spawnStartDelay = 1;

					EndTime = Main.GameUpdateCount + ((lengthInSeconds + spawnStartDelay) * (uint)TimeSystem.LogicFramerate);
					Encounter = CreateEncounter(lengthInSeconds);
				}
				// Start closing when the encounter has been completed, or if the players have ran out of time.
				else if (!Encounter.IsValid || Encounter.Instance.State == EncounterState.Completed || Main.GameUpdateCount >= EndTime)
				{
					Close();
				}
			}
		}

		// Effects.
		if (!Main.dedServ)
		{
			UpdateEffects();
			UpdateAudio();
		}
	}

	private void UpdateEffects()
	{
		if (Main.dedServ) { return; }

		VisualParams visuals = GetVisualParameters();

		if (Main.rand.NextBool(10) && Activated && OpeningAnimation > 0.34f)
		{
			Dust.NewDust(Projectile.position + new Vector2(8), Projectile.width - 16, Projectile.height - 16, visuals.DustId);
		}

		float lightOffsetMul = 16f * MathUtils.Clamp01(OpeningAnimation - ClosingAnimation);
		float lightPowerMul = MathUtils.Clamp01(MathF.Pow(MathUtils.Clamp01(1f - ClosingAnimation), 0.5f));

		for (float offsetMul = OpeningAnimation - ClosingAnimation, yOffset = -5; yOffset <= +5; yOffset++)
		{
			float pulse = (MathF.Sin((float)Main.timeForVisualEffects * 0.1f) + 1f) * 0.5f;
			var color = Color.Lerp(visuals.lightA, visuals.lightB, pulse).ToVector3();
			Lighting.AddLight(Projectile.Center + new Vector2(0f, yOffset * lightOffsetMul), color * lightPowerMul);
		}

		if (Activated && !Closing && visuals.Filter != null)
		{
			Filters.Scene.Activate(visuals.Filter);
		}
	}
	private void UpdateAudio()
	{
		Vector2 center = Projectile.Center;
		float distMul = new ExponentialRange(0f, 2048f, 2f).DistanceFactor(Main.LocalPlayer.Distance(center));
		float distAndClosing = distMul * (1f - ClosingAnimation);

		ProjectileAudioTracker tracker = (audioTracker ??= new ProjectileAudioTracker(Projectile));

		void UpdateSound(in SoundStyle style, ref (SlotId Handle, float Volume) tuple, float target)
		{
			tuple.Volume = MathUtils.StepTowards(tuple.Volume, target, MathF.Max(0f, 2f * TimeSystem.LogicDeltaTime));
			SoundUtils.UpdateLoopingSound(ref tuple.Handle, center, tuple.Volume, null, in style, _ => tracker.IsActiveAndInGame());
		}

		float posApproach = ApproachAnimation > 0f ? 1f : float.Epsilon;
		float negApproach = ApproachAnimation > 0f ? float.Epsilon : 1f;

		UpdateSound(SoundInactive, ref soundDormant, (Activated ? 0.2f : 1.0f) * negApproach * distAndClosing);
		UpdateSound(SoundApproached, ref soundRousing, (Activated ? 0.3f : 1.0f) * posApproach * distAndClosing);
		UpdateSound(SoundActive, ref soundAwoken, (Activated ? 1.0f : 0.0f) * distAndClosing);
	}

	public override void OnKill(int timeLeft)
	{
		Close();
	}

	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
		behindNPCsAndTiles.Add(index);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition;

		if (shader?.Value is not Effect effect)
		{
			return false;
		}

		float openFactor = MathUtils.Clamp01((OpeningAnimation * 0.75f) + (ApproachAnimation * 0.25f));
		float closeFactor = MathUtils.Clamp01((ClosingAnimation * 0.8f) + (ProgressAnimation * 0.2f));

		effect.Parameters["progress"].SetValue(openFactor);
		effect.Parameters["closingProgress"].SetValue(closeFactor);
		effect.Parameters["timeManual"].SetValue((float)Main.timeForVisualEffects * 0.027f);
		effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture + "_PerlinNoiseMap").Value);
		effect.Parameters["_PaletteTex"].SetValue(ModContent.Request<Texture2D>(Texture + "_Palette_" + Kind.ToString()).Value);
		effect.Parameters["_PNoiseTex"].SetValue(ModContent.Request<Texture2D>(Texture + "_PerlinNoiseMap").Value);
		effect.Parameters["_DNoiseTex"].SetValue(ModContent.Request<Texture2D>(Texture + "_DisplacementNoiseMap").Value);

		effect.Parameters["xCameraOffset"].SetValue(Main.screenPosition.X - Projectile.Center.X);
		effect.Parameters["yCameraOffset"].SetValue(Main.screenPosition.Y - Projectile.Center.Y);

		Texture2D spaceTex1 = ModContent.Request<Texture2D>(Texture + "_SpaceMap1_" + Kind.ToString()).Value;
		Texture2D spaceTex2 = ModContent.Request<Texture2D>(Texture + "_SpaceMap2_" + Kind.ToString()).Value;

		effect.Parameters["mapResX"].SetValue(spaceTex1.Width);
		effect.Parameters["mapResY"].SetValue(spaceTex1.Height);

		effect.Parameters["resX"].SetValue(tex.Width * Projectile.scale * 4f);
		effect.Parameters["resY"].SetValue(tex.Height * Projectile.scale * 4f);
		effect.Parameters["_SpaceTex1"].SetValue(spaceTex1);
		effect.Parameters["_SpaceTex2"].SetValue(spaceTex2);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointClamp, default, default, effect, Main.GameViewMatrix.TransformationMatrix);
		Main.spriteBatch.Draw(tex, position, null, Color.White, 0, tex.Size() / 2f, Projectile.scale * 4.0f, SpriteEffects.None, 0);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		
		this.TryInteracting();

		/*
		(_, _, Color colorBase) = GetVisualParameters();
        var sizeTargets = Vector2.Lerp
        (
            Vector2.Lerp(new(0.5f, 0.75f), new(3.50f, 3.75f), OpeningAnimation),
            new(0.0f, 0.0f),
            ClosingAnimation
        );
        (float minSize, float maxSize) = (sizeTargets.X, sizeTargets.Y);

        const int numLayers = 3;

        for (int i = 0; i < numLayers; ++i)
        {
            float rotation = (Projectile.rotation * (i % 2 == 0 ? -1 : 1)) + (i * 1.5f);
            Color color = colorBase * ((3 - i) * 0.33f) * Projectile.Opacity;
            float size = MathHelper.Lerp(minSize, maxSize, i / (float)(numLayers - 1));

            Main.EntitySpriteDraw(tex, position, null, color, rotation, tex.Size() / 2f, size, SpriteEffects.None, 0);
        }

        this.DrawHighlightAndCheckRightClickInteraction(Highlight.Value, position, lightColor);
		*/

		return false;
	}

	public void Activate()
	{
		if ((BitFlags & Flags.Activated) != 0) { return; }

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			Vector2 compareSpot = Main.LocalPlayer.Center;
			if (!Main.LocalPlayer.IsProjectileInteractibleAndInInteractionRange(Projectile, ref compareSpot)) { return; }

			RiftInteractionHandler.Send(Projectile.identity);
			return;
		}

		BitFlags |= Flags.Activated;

		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
		}

		// Effects.
		if (!Main.dedServ)
		{
			VisualParams visuals = GetVisualParameters();

			SoundEngine.PlaySound(SoundActivation, Projectile.Center);

			for (int i = 0; i < 100; i++)
			{
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), visuals.DustId, Main.rand.NextVector2Circular(8f, 8f));
			}
		}
	}

	public void Close()
	{
		if (Closing || Main.netMode == NetmodeID.MultiplayerClient) { return; }

		BitFlags |= Flags.Closing;

		if (!Main.dedServ)
		{
			SoundEngine.PlaySound(SoundDeactivation, Projectile.Center);
		}

		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
		}

		UpdateProgress();
		RemoveEncounter();
	}

	public void UpdateProgress()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) { return; }

		float progress = Encounter.Instance.EncounterScore / MathF.Max(0.001f, Encounter.Instance.TotalBaseScore);
		if (progress != Progress)
		{
			Progress = progress;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
			}
		}
	}

	protected virtual Encounter CreateEncounter(uint lengthInSeconds)
	{
		const int extentsW = 40;
		const int extentsH = 20;
		Vector2 worldOrigin = Projectile.Center;
		Point16 tileOrigin = worldOrigin.ToTileCoordinates16();

		var area = new Vector4Int
		{
			X = Math.Max(tileOrigin.X - extentsW, 0),
			Y = Math.Max(tileOrigin.Y - extentsH, 0),
			Z = Math.Min(tileOrigin.X + extentsW, Main.maxTilesX - 1),
			W = Math.Min(tileOrigin.Y + extentsH, Main.maxTilesY - 1),
		};
		var rectangle = new Rectangle(area.X, area.Y, area.Z - area.X, area.W - area.Y);

		string[] encounterPaths = Kind switch
		{
			ConfluxRiftKind.Infernal => [
				"Content/Encounters/Squads/InfernalRift",
				"Content/Encounters/Squads/BruiserBrothers",
			],
			ConfluxRiftKind.Glacial => [
				"Content/Encounters/Squads/InfernalRift",
				"Content/Encounters/Squads/BruiserBrothers",
			],
			ConfluxRiftKind.Celestial => [
				"Content/Encounters/Squads/InfernalRift",
				"Content/Encounters/Squads/BruiserBrothers",
			],
			_ => throw new NotImplementedException(),
		};
		EncounterDescription[] baseEncounters = encounterPaths.Select(p => EncounterIO.GetEncounterFromModPath(Mod, p)).ToArray();
		(EnemySpawn Spawn, EnemyRole Role)[] enemyPool = baseEncounters.SelectMany(e => e.Waves.SelectMany(w => w.Spawns.Select(s => (s, new EnemyRole())))).ToArray();

		foreach (ref (EnemySpawn Spawn, EnemyRole Role) tuple in enemyPool.AsSpan())
		{
			tuple.Role = tuple.Spawn switch
			{
				// Ugly hardcode for the sake of testing.
				_ when tuple.Spawn.NpcType.Type == ModContent.NPCType<FallenSchemer>() => new() { Fodder = 2f },
				_ when tuple.Spawn.NpcType.Type == ModContent.NPCType<FallenSavage>() => new() { Fodder = 1f },
				_ when tuple.Spawn.NpcType.Type == ModContent.NPCType<FallenTyrant>() => new() { Heavy = 1f },
				_ when tuple.Spawn.NpcType.Type == ModContent.NPCType<Abominable>() => new() { Boss = 1f },
				_ => new EnemyRole { Fodder = 1f },
			};
		}

		var waves = new List<EncounterWave>();

		// In-wave spawn pacing.
		uint cooldownPerBatch = (uint)(180 * MathHelper.Lerp(1f, 0.2f, (MathHelper.Clamp(MappingWorld.MapTier, 1f, 10f) - 1f) / 9f));
		uint accumulatedCooldownPerEnemy = (uint)(60 / MathHelper.Lerp(1f, 5f, MathHelper.Clamp(MappingWorld.MapTier, 1f, 10f)));
		uint accumulatedCooldown = 0;

		// The amount of enemies spawned every second is scaled by map tier.
		uint waveCount = 4;
		uint spawningPeriodInSeconds = lengthInSeconds - Math.Min(15 + 3, lengthInSeconds / 2);
		uint spawningPeriodInTicks = (uint)(spawningPeriodInSeconds * TimeSystem.LogicFramerate);
		float waveDelayPowFactor = 1.1f; //0.85f;
		float waveDelayBase = spawningPeriodInTicks / MathF.Pow(waveDelayPowFactor, waveCount - 1) / waveCount;

		EnemySpawnEffect spawnEffect = Kind switch
		{
			ConfluxRiftKind.Glacial => EnemySpawnEffect.GlacialRift,
			ConfluxRiftKind.Infernal => EnemySpawnEffect.InfernalRift,
			ConfluxRiftKind.Celestial => EnemySpawnEffect.CelestialRift,
			_ => EnemySpawnEffect.Teleport,
		};

		uint GetWaveTick(int waveIndex)
		{
			return (uint)(waveIndex * waveDelayBase * MathF.Pow(waveDelayPowFactor, waveIndex));
		}

		EnemyRole GetWaveQuota(int waveIndex)
		{
			// Play with printing to balance this out.
			return new EnemyRole
			{
				Fodder = 4.00f + (MappingWorld.MapTier * 0.71f) + (Math.Max(0f, waveIndex) * -0.25f * MathF.Pow(1.00f, waveIndex)),
				Heavy = 0.50f + (MappingWorld.MapTier * 0.41f) + (Math.Max(0f, waveIndex) * 0.50f * MathF.Pow(1.00f, waveIndex)),
				Boss = 0.00f + (MappingWorld.MapTier * 0.26f) + (Math.Max(0f, waveIndex - 2) * 1.50f * MathF.Pow(0.90f, waveIndex)),
			};
		}

		// Add as many waves as needed.
		for (int waveIndex = 0; waveIndex < waveCount; waveIndex++)
		{
			var spawns = new List<EnemySpawn>();

			uint thisWaveTick = GetWaveTick(waveIndex);
			uint nextWaveTick = GetWaveTick(waveIndex + 1);
			uint delayUntilNextWave = nextWaveTick - thisWaveTick;

			EnemyRole quota = GetWaveQuota(waveIndex);

			// Add spawns to the wave as long the role quota is not filled.
			while (true)
			{
				bool addedAnySpawn = false;

				//TODO: Randomize this iteration.
				foreach ((EnemySpawn baseSpawn, EnemyRole price) in enemyPool)
				{
					// Pay the 'price' for this spawn.
					if (!quota.TryPay(price)) { continue; }

					EnemySpawn spawn = baseSpawn;
					spawn.Effect = spawnEffect;

					if (spawn.SpawnPlacement is { } placement)
					{
						spawn.SpawnPlacement = placement with { MinDistanceFromEnemies = 8f };
					}

					uint cooldown = 0;
					if ((accumulatedCooldown += accumulatedCooldownPerEnemy) >= cooldownPerBatch)
					{
						cooldown = cooldownPerBatch;
						accumulatedCooldown -= cooldownPerBatch;
					}

					spawn.CooldownInTicks = cooldown;
					spawns.Add(spawn);

					addedAnySpawn = true;
				}

				if (!addedAnySpawn) { break; }
			}

			spawns[^1] = spawns[^1] with { CooldownInTicks = delayUntilNextWave };

			waves.Add(new EncounterWave
			{
				Spawns = spawns.ToArray(),
				// Spawning does not wait for the player to kill anything. Waves can be thought of as cosmetic.
				TargetEncounterScore = 0.0f,
				TargetWaveScore = 0.0f,
				TargetSpawnScore = 0.0f,
			});
		}

		Encounter encounter = EnemyEncounters.CreateEncounter(new EncounterDescription
		{
			Identifier = "Rift",
			SpawnOrigin = tileOrigin,
			ActivationOrigin = worldOrigin,
			ActivationRange = 2048f,
			SpawnArea = rectangle,
			Waves = waves.ToArray(),
			Music = new(MusicID.LunarBoss),
			SceneEffectPriority = SceneEffectPriority.BossLow,
		});

		return encounter;
	}

	private void RemoveEncounter()
	{
		if (!Encounter.IsValid) { return; }

		// Despawn all remaining NPCs.
		foreach (NPC npc in Encounter.IterateRemainingEnemies())
		{
			EnemySpawnEffect spawnEffect = EnemySpawnEffect.Teleport;
			switch (Kind)
			{
				case ConfluxRiftKind.Glacial:
					spawnEffect = EnemySpawnEffect.GlacialRift;
					break;
				case ConfluxRiftKind.Infernal:
					spawnEffect = EnemySpawnEffect.InfernalRift;
					break;
				case ConfluxRiftKind.Celestial:
					spawnEffect = EnemySpawnEffect.CelestialRift;
					break;
			}
			EnemySpawning.SpawnEffects(npc, spawnEffect, npc.Center);
			npc.active = false;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
			}
		}

		// Remove encounter.
		Encounter.Remove();
	}

	private void DropRewards()
	{
		int rewardType = Kind switch
		{
			ConfluxRiftKind.Glacial => ModContent.ItemType<GlacialConflux>(),
			ConfluxRiftKind.Celestial => ModContent.ItemType<CelestialConflux>(),
			_ => ModContent.ItemType<InfernalConflux>(),
		};
		int rewardAmount = Progress switch
		{
			<= 0.20f => 0,
			<= 0.50f => 1,
			<= 0.80f => 2,
			< 1.000f => 3,
			_ => 4,
		};

		float multiplier = 1f;
		// 5% increase per map tier.
		multiplier += (MappingWorld.MapTier * 0.05f);
		// 2% increase per map modifier weight.
		multiplier += (MappingWorld.TotalWeight() * 0.02f);

		rewardAmount = (int)MathF.Floor(rewardAmount * multiplier);

		for (int i = 0; i < rewardAmount; i++)
		{
			Vector2 position = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
			int itemIdx = Item.NewItem(null, position, rewardType, 1, noBroadcast: true);
			Item item = Main.item[itemIdx];
			item.velocity = Vector2.UnitX.RotatedBy((i / (float)rewardAmount) * MathHelper.TwoPi).RotatedByRandom(0.1f) * 5f;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncItem, number: itemIdx);
			}
		}
	}

	private VisualParams GetVisualParameters()
	{
		return Kind switch
		{
			ConfluxRiftKind.Glacial => new(DustID.Firework_Blue, Color.AliceBlue, ColorUtils.FromHexRgb(0x3b4782), ColorUtils.FromHexRgb(0xa4e1e4), "Vortex"),
			ConfluxRiftKind.Infernal => new(DustID.Firework_Red, Color.OrangeRed, ColorUtils.FromHexRgb(0xa73d3d), ColorUtils.FromHexRgb(0xffcf85), "Solar"),
			ConfluxRiftKind.Celestial => new(DustID.WitherLightning, Color.MediumVioletRed, ColorUtils.FromHexRgb(0x7f3b82), ColorUtils.FromHexRgb(0xe4a4be), "Nebula"),
			_ => throw new NotImplementedException(),
		};
	}

	bool IRightClickableProjectile.RightClick(Player player, bool mouseDirectlyOver)
	{
		if (Activated || !CanInteract()) { return false; }

		if (Main.mouseRight && Main.mouseRightRelease)
		{
			Activate();

			return true;
		}

		if (mouseDirectlyOver)
		{
			Main.instance.MouseText(Language.GetTextValue("Mods.PathOfTerraria.Misc.Rifts.Touch"), rare: 1);
		}

		return false;
	}
}

/// <summary>
/// Synchronizes right click interactions with rifts.
/// </summary>
internal class RiftInteractionHandler : Handler
{
	public static void Send(int riftIdentity)
	{
		ModPacket packet = Networking.GetPacket<RiftInteractionHandler>();
		packet.Write(riftIdentity);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ModPacket packet = Networking.GetPacket(Id);
		int riftIdentity = reader.ReadInt32();

		if (Main.player[sender] is not { active: true } player) { return; }

		if (Main.projectile.FirstOrDefault(p => p.identity == riftIdentity) is not { ModProjectile: ConfluxRift rift }) { return; }

		// Increased reach distance for synchronization grace.
		Point tileTarget = rift.Projectile.Hitbox.ClosestPointInRect(player.Center).ToTileCoordinates();
		if (!player.IsInTileInteractionRange(tileTarget.X, tileTarget.Y, TileReachCheckSettings.Simple with { TileRangeMultiplier = 2 })) { return; }

		rift.Activate();
	}
}
