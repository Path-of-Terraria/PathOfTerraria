// #define DEBUG_LOG
// #define INSTANT_REFILL

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Content.Conflux;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using ReLogic.Graphics;
using SubworldLibrary;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Chat;

#nullable enable

namespace PathOfTerraria.Common.Conflux;

/// <summary> This system spawns rifts in domain worlds. </summary>
internal sealed class ConfluxRifts : ModSystem
{
	public record struct GenerationCfg(int MaxAmount);
	public record struct NaturalSpawnCfg(int MaxAmount, double AccumulationTimeRate);
	public record struct NaturalRiftSlot(ConfluxRiftKind Kind)
	{
		public uint DespawnTicks;
		public Vector2? Position;
		public (int Index, int Identity)? Projectile;
	}
	
	private static bool ranGeneration;
	private static float progressBarAlpha;
	private static float progressBarProgress;
	private static float progressBarPulse;
	private static Asset<Texture2D>? progressBarTexture;
	private static Asset<Texture2D>? progressBarOutline;
	private static GameInterfaceLayer? progressBarLayer;

	/// <summary> Types of rifts to try to spawn naturally. Accumulated as time passes. </summary>
	private static readonly List<NaturalRiftSlot> naturalRifts = [];
	private static double accumulatedTime;
	private static int numSpawnsToAnnounce;

	private static Span<NaturalRiftSlot> NaturalRifts => CollectionsMarshal.AsSpan(naturalRifts);

	public override void ClearWorld()
	{
		ranGeneration = false;
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag["accumulatedTime"] = accumulatedTime;
		tag["naturalRifts"] = new TagCompound {
			{ "count", naturalRifts.Count },
			{ "kinds", naturalRifts.Select(r => r.Kind.ToString()).ToArray() },
			{ "positions", naturalRifts.Select(r => r.Position ?? -Vector2.One).ToArray() },
		};
	}
	public override void LoadWorldData(TagCompound tag)
	{
		naturalRifts.Clear();
		accumulatedTime = tag.TryGet("accumulatedTime", out double time) ? time : 0;

		if (tag.TryGet("naturalRifts", out TagCompound tagNatural))
		{
			int count = tagNatural.GetInt("count");
			IList<string> kinds = tagNatural.GetList<string>("kinds");
			IList<Vector2> positions = tagNatural.GetList<Vector2>("positions");

			for (int i = 0; i < count; i++)
			{
				if (!Enum.TryParse(kinds[i], out ConfluxRiftKind kind)) { continue; }
				Vector2? position = positions[i].X >= 0 && positions[i].Y >= 0 ? positions[i] : null;

				naturalRifts.Add(new(kind) { Position = position });
			}
		}
	}

	private static bool HasActivePlayers()
	{
		foreach (Player p in Main.ActivePlayers) { if (!p.dead) { return true; } }
		return false;
	}

	private static int? FindRiftSlot(ConfluxRift rift)
	{
		int spawnIndex = naturalRifts.FindIndex(s => s.Projectile?.Identity == rift.Projectile.identity);
		return spawnIndex >= 0 ? spawnIndex : null;
	}
	// Only called on the world side.
	internal static void OnRiftActivated(ConfluxRift rift)
	{
		if (rift.BitFlags.HasFlag(ConfluxRift.Flags.NaturallySpawned))
		{
			if (FindRiftSlot(rift) is int slotIndex)
			{
				naturalRifts.RemoveAt(slotIndex);
			}
		}
	}

	public override void PostUpdateWorld()
	{
		NaturalSpawnCfg naturalCfg = GetNaturalSpawnConfig();
#if INSTANT_REFILL
		naturalCfg.AccumulationTimeRate = 1;
#endif

		// Clear excessive spawns.
		while (naturalRifts.Count > naturalCfg.MaxAmount) { naturalRifts.RemoveAt(naturalRifts.Count - 1); }

		// Accumulate pending natural spawns.
		if (naturalRifts.Count < naturalCfg.MaxAmount)
		{
			accumulatedTime = Math.Max(0, accumulatedTime + Main.dayRate);
			if (accumulatedTime > naturalCfg.AccumulationTimeRate)
			{
				accumulatedTime = 0;
				naturalRifts.Add(new(RollRiftKind()));
				numSpawnsToAnnounce = Math.Max(0, numSpawnsToAnnounce + 1);

#if DEBUG_LOG
				DebugUtils.DebugLog($"{GetType().Name}: New conflux rift spawn pending.");
#endif
			}
		}
		
		// Spawn rifts.
		if (HasActivePlayers())
		{
			if (!ranGeneration && GetGenerationConfig() is { } genCfg && genCfg.MaxAmount > 0)
			{
				SpawnRifts(generation: true, genCfg.MaxAmount);
				ranGeneration = true;
			}

			const int naturalSpawnOneInXChance = 100;
			if (naturalRifts.Count > 0 && Main.rand.NextBool(naturalSpawnOneInXChance))
			{
				SpawnNaturalRifts();
			}
			else
			{
				DespawnRifts(ConfluxRift.Flags.NaturallySpawned, checkDistanceFromPlayers: true);
			}
		}
	}

	public override void UpdateUI(GameTime gameTime)
	{
		// Find the closest rift, if any.

		Vector2 playerCenter = Main.LocalPlayer.Center;
		float minSqrDistance = float.PositiveInfinity;
		ConfluxRift? closestRift = null;

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.ModProjectile is not ConfluxRift { } rift) { continue; }

			if (!rift.CountsAsActiveBattle()) { continue; }

			float sqrDistance = projectile.DistanceSQ(playerCenter);
			if (sqrDistance < minSqrDistance)
			{
				minSqrDistance = sqrDistance;
				closestRift = rift;
			}
		}

		// Update alpha, decay pulse effects.
		progressBarAlpha = MathHelper.Lerp(progressBarAlpha, closestRift != null ? 1f : 0f, 3f * TimeSystem.LogicDeltaTime);
		progressBarPulse = MathF.Max(0f, progressBarPulse - (2f * TimeSystem.LogicDeltaTime));

		// When the progress value changes, various animations are played through the pulse value.
		if (closestRift != null)
		{
			if (closestRift.Progress != progressBarProgress && closestRift.Progress != 0f)
			{
				progressBarPulse = 1f;
			}

			progressBarProgress = closestRift.Progress;
		}
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		progressBarLayer ??= new LegacyGameInterfaceLayer($"{nameof(PathOfTerraria)}/ConfluxRiftProgress", DrawProgressBar, InterfaceScaleType.UI);
		progressBarLayer.Active = true;
		layers.Insert(Math.Max(0, layers.FindIndex(l => l.Name.Equals("Vanilla: Mouse Text")) - 1), progressBarLayer);
	}

	public static GenerationCfg GetGenerationConfig()
	{
		// Spawn on exploration maps.
		if (SubworldSystem.Current is MappingWorld and IExplorationWorld and not RavencrestSubworld)
		{
			WeightRand<int> targetRiftPool = new()
			{
				{ 3, 0.1f + (MathF.Pow(MappingWorld.MapTier - 1.0f, 2.50f) * 0.3f) },
				{ 2, 0.3f + (MathF.Pow(MappingWorld.MapTier - 1.0f, 2.05f) * 0.6f) },
				{ 1, 0.7f }
			};

			return new(targetRiftPool.RollValue());
		}

		return default;
	}
	public static NaturalSpawnCfg GetNaturalSpawnConfig()
	{
		// Spawn in the overworld after the wall of flesh has been defeated.
		if (SubworldSystem.Current == null && Main.hardMode)
		{
			int amount = 3;
			double rate = (Main.dayLength + Main.nightLength) * 1.2f;

			// Faster rift spawn rate after Golem.
			rate *= NPC.downedGolemBoss ? 0.75f : 1;
			// More active rifts after Plantera.
			amount += NPC.downedPlantBoss ? 1 : 0;
			// Faster rift spawn rate after Lunatic Cultist.
			rate *= NPC.downedAncientCultist ? 0.75f : 1;
			// More active rifts after Moon Lord.
			amount += NPC.downedMoonlord ? 1 : 0;

			return new(MaxAmount: amount, AccumulationTimeRate: rate);
		}

		return default;
	}

	private static readonly WeightRand<ConfluxRiftKind> riftKindPool = new()
	{
		{ ConfluxRiftKind.Infernal, 3f },
		{ ConfluxRiftKind.Glacial, 2f },
		{ ConfluxRiftKind.Celestial, 1f },
	};
	public static ConfluxRiftKind RollRiftKind()
	{
		ConfluxRiftKind result = riftKindPool.RollValue();
		return result;
	}

	private static List<Vector2>? riftLocations = null;
	private static List<Vector2> GetRiftLocations()
	{
		List<Vector2> result;
		if (riftLocations == null) { result = riftLocations = new List<Vector2>(8); }
		else { riftLocations.Clear(); result = riftLocations; }

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (!(projectile.ModProjectile is ConfluxRift { } rift)) { continue; }
			result.Add(projectile.Center);
		}
		
		return result;
	}

	public static void SpawnNaturalRifts()
	{
		foreach (ref NaturalRiftSlot slot in NaturalRifts)
		{
			// Skip if this is already spawned.
			if (slot.Projectile is { } projRef && Main.projectile[projRef.Index] is { active: true } proj && proj.identity == projRef.Identity)
			{
				continue;
			}
			
			// Try spawning. Break on failure.
			if (SpawnRift(generation: false, forcedPosition: slot.Position, forcedKind: slot.Kind) is not Projectile rift)
			{
				break;
			}

			// Record information.
			slot.Projectile = (rift.whoAmI, rift.identity);
			slot.Position = rift.position; // Not Center!

			if (numSpawnsToAnnounce > 0)
			{
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey($"Mods.{nameof(PathOfTerraria)}.Misc.Rifts.NaturalSpawn"), Color.Magenta);
				numSpawnsToAnnounce--;
			}

			break;
		}
	}

	/// <summary> Attempts to spawn a conflux rift. </summary>
	public static Projectile? SpawnRift(bool generation, Vector2? forcedPosition = null, ConfluxRiftKind? forcedKind = null)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) { return null; }

		bool natural = !generation;

		int worldEdgeOffset = 40;
		int freeWidthInTiles = 25;
		int freeHeightInTiles = 17;
		var minDistanceFromRifts = new SqrScalar<float>(2000);
		var distanceFromPlayers = new SqrRange<float>(natural ? 4000 : 2000, natural ? 15_000 : 0);
		var despawnDistanceFactor = new SqrScalar<float>(1.25f);

		// There's a small chance for natural spawns to use generation logic.
		const int oneInXThatFullAreaIsUsed = 10;
		Vector4Int spawnArea = ((generation || Main.rand.NextBool(oneInXThatFullAreaIsUsed))
			// Pre-generation spans almost the whole world.
			? new(worldEdgeOffset, worldEdgeOffset, Main.maxTilesX - 1- worldEdgeOffset, Main.maxTilesY - 1 - worldEdgeOffset)
			// Natural spawning is biased towards the underground, excluding underworld.
			: new(worldEdgeOffset, (int)Main.worldSurface, Main.maxTilesX - 1 - worldEdgeOffset, Main.UnderworldLayer - 100)
		);
		Rectangle spawnRect = new(spawnArea.X, spawnArea.Y, spawnArea.Z - spawnArea.X, spawnArea.W - spawnArea.Y);

		IEntitySource? source = Entity.GetSource_None();
		List<Vector2> riftLocations = GetRiftLocations();

		bool IsAValidPlacementPoint(Point16 point)
		{
			Vector2 worldPos = point.ToWorldCoordinates();

			if (riftLocations.Any(p => p.DistanceSQ(worldPos) < minDistanceFromRifts.SqrValue))
			{
				return false;
			}

			return true;
		}

		var placement = new SpawnPlacement
		{
			Area = spawnRect,
			CollisionSize = new(freeWidthInTiles * TileUtils.TileSizeInPixels, freeHeightInTiles * TileUtils.TileSizeInPixels),
			OnGround = true,
			MinDistanceFromEnemies = 0,
			MaxDistanceFromEnemies = 0,
			MinDistanceFromPlayers = distanceFromPlayers.Min,
			MaxDistanceFromPlayers = distanceFromPlayers.Max,
			MaxSearchAttempts = generation ? 4096 : 1024,
			SkippedLiquids = LiquidMask.All,
			CustomPredicate = IsAValidPlacementPoint,
		};

		Vector2 position;
		if (forcedPosition != null)
		{
			position = forcedPosition.Value;
		}
		else if (!EnemySpawning.TryFindingSpawnPosition(out position, in placement))
		{
			return null;
		}

		// Offset placement into the center.
		position.X += placement.CollisionSize.X * 0.5f;
		position.Y += placement.CollisionSize.Y * 0.5f;
		position.X -= 8;
		position.Y -= 8;

		ConfluxRiftKind kind = forcedKind ?? RollRiftKind();
		int type = kind switch
		{
			ConfluxRiftKind.Infernal => ModContent.ProjectileType<InfernalRift>(),
			ConfluxRiftKind.Glacial => ModContent.ProjectileType<GlacialRift>(),
			ConfluxRiftKind.Celestial => ModContent.ProjectileType<CelestialRift>(),
			_ => throw new NotImplementedException(),
		};

		ConfluxRift.Flags flags = 0;
		flags |= natural ? ConfluxRift.Flags.NaturallySpawned : 0;
		flags |= generation ? ConfluxRift.Flags.PreGenerated : 0;
		float flagsAsFloat = Unsafe.BitCast<ConfluxRift.Flags, float>(flags);

		var rift = Projectile.NewProjectileDirect(source, position, Vector2.Zero, type, 0, 0f, ai0: flagsAsFloat);
		DebugUtils.DebugLog($"Spawned a rift at coordinates: [{position.X:0},{position.Y:0}].");
		riftLocations.Add(rift.Center);

		return rift;
	}
	/// <summary> Attempts to spawn conflux rifts, returns the amount created. </summary>
	public static int SpawnRifts(bool generation, int targetRifts, int maxSpawns = 0, ConfluxRiftKind? forcedKind = null)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) { return 0; }

		List<Vector2> riftLocations = GetRiftLocations();
		int numTotalRifts = riftLocations.Count;
		int numToSpawn = Math.Max(0, targetRifts - numTotalRifts);
		int numSpawned = 0;

		for (int i = 0; i < numToSpawn; i++)
		{
			if (SpawnRift(generation, forcedKind: forcedKind) is not Projectile rift)
			{
				break;
			}

			numSpawned++;
			
			if (maxSpawns > 0 && numSpawned >= maxSpawns)
			{
				break;
			}
		}

		if (generation)
		{
			DebugUtils.DebugLog($"Pre-generated {numSpawned} rifts.");
		}

		return numSpawned;
	}

	private static void DespawnRifts(ConfluxRift.Flags requiredFlags, bool checkDistanceFromPlayers)
	{
		var despawnRange = new SqrScalar<float>(15_000 + 2_000);
		const int despawnTimeInTicks = 60 * 60;
		
		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (!(projectile.ModProjectile is ConfluxRift { } rift)) { continue; }
			if (!rift.BitFlags.HasFlag(requiredFlags)) { continue; }

			int? slotIndex = FindRiftSlot(rift);
			bool hasSlot = slotIndex != null;
			ref NaturalRiftSlot slot = ref (slotIndex != null ? ref NaturalRifts[slotIndex.Value] : ref Unsafe.NullRef<NaturalRiftSlot>());

			if (checkDistanceFromPlayers)
			{
				Vector2 riftCenter = projectile.Center;
				float minSqrDistance = float.PositiveInfinity;
				foreach (Player player in Main.ActivePlayers)
				{
					minSqrDistance = MathF.Min(minSqrDistance, player.Center.DistanceSQ(riftCenter));
				}

				if (minSqrDistance <= despawnRange.SqrValue)
				{
					if (hasSlot) { slot.DespawnTicks = 0; }
					continue;
				}

				if (hasSlot)
				{
					if (slot.DespawnTicks < despawnTimeInTicks)
					{
						slot.DespawnTicks++;
						continue;
					}
				}
			}

			// Clear position information from the slot.
			if (hasSlot)
			{
				slot.Position = null;
			}
			
			projectile.Kill();
		}
	}

	private static bool DrawProgressBar()
	{
		if (progressBarAlpha <= 0.001f)
		{
			return true;
		}

		// Load and wait for textures.

		progressBarTexture ??= ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Conflux/StabilityProgress");
		progressBarOutline ??= ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Conflux/StabilityOutline");

		if (progressBarTexture is not { IsLoaded: true, Value: { } uiTexture }) { return true; }

		if (progressBarOutline is not { IsLoaded: true, Value: { } uiOutline }) { return true; }

		float uiPulse = progressBarPulse * progressBarPulse;
		Color alphaColor = new(Vector4.One * progressBarAlpha);
		Color alphaOutlineColor = new(Vector4.One * MathF.Min(1f, progressBarAlpha * 4f));
		SpriteBatch sb = Main.spriteBatch;

		// Draw animated background.

		const int frameCount = 5;
		byte uiFrame = (byte)MathF.Floor((frameCount - 1) * MathHelper.Clamp(progressBarProgress, 0f, 1f));
		Rectangle srcRect = new SpriteFrame(1, 5).With(0, uiFrame).GetSourceRectangle(uiTexture);
		Vector2 uiCenter = (Main.ScreenSize.ToVector2() * new Vector2(0.5f, 0.0f)) + new Vector2(0f, 120f);
		// Shake effect.
		uiCenter += Vector2.UnitX.RotatedBy(Main.time * 0.8f) * 4f * uiPulse;
		Vector2 uiOrigin = srcRect.Size() * 0.5f;
		Vector2 uiScale = Vector2.One;
		var uiBaseColor = Color.Lerp(Color.White, Color.LightPink, uiPulse);
		Color uiPrimaryColor = uiBaseColor.MultiplyRGBA(alphaColor);
		Color uiOutlineColor = uiBaseColor.MultiplyRGBA(alphaOutlineColor);

		sb.Draw(uiOutline, uiCenter, srcRect, uiOutlineColor, 0f, uiOrigin, uiScale, 0, 0f);
		sb.Draw(uiTexture, uiCenter, srcRect, uiPrimaryColor, 0f, uiOrigin, uiScale, 0, 0f);

		// Draw progress percentage.

		DynamicSpriteFont textFont = FontAssets.MouseText.Value;
		string text = $"{progressBarProgress * 100:0}%";
		Vector2 textSize = textFont.MeasureString(text);
		Vector2 textCenter = uiCenter + new Vector2(0f, 4f);
		// Shake effect.
		uiCenter += Vector2.UnitX.RotatedBy(Main.time * 0.75f) * 5f * uiPulse;
		Vector2 textOrigin = textSize * 0.5f;
		Vector2 fontSize = uiScale + (Vector2.One * uiPulse * 0.05f);
		Color textColor = (progressBarProgress == 1f ? Color.YellowGreen : Color.Lerp(Color.White, Color.MediumVioletRed, uiPulse)).MultiplyRGBA(alphaColor);

		ChatManager.DrawColorCodedString(sb, textFont, text, textCenter, textColor, 0f, textOrigin, uiScale);

		Rectangle uiHoverArea = new Rectangle((int)uiCenter.X, (int)uiCenter.Y, 0, 0).Inflated((int)(srcRect.Width * 0.4f), (int)(srcRect.Height * 0.4f));
		if (uiHoverArea.Contains(Main.MouseScreen.ToPoint()))
		{
			Main.instance.MouseText(Language.GetTextValue("Mods.PathOfTerraria.Misc.Rifts.Stability"));
		}

		return true;
	}
}
