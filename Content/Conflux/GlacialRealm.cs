using System.Collections.Generic;
using System.IO;
using Mono.CompilerServices.SymbolWriter;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Core.Camera;
using PathOfTerraria.Core.Lighting;
using PathOfTerraria.Core.Structures;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

internal sealed class GlacialRealm : BossDomainSubworld, IOverrideBiome
{
	internal sealed class GlacialRealmSystem : ModSystem
	{
		public override void NetSend(BinaryWriter writer)
		{
			if (IsActive) { ArenaCenter.NetSend(writer); }
		}
		public override void NetReceive(BinaryReader reader)
		{
			if (IsActive) { ArenaCenter.NetReceive(reader); }
		}
	}

	public static StructurePointTracker ArenaCenter = new("ArenaCenter");
	public static StructurePointTracker PlayerSpawn = new("PlayerSpawn");
	public static StructurePointTracker PlayerExit = new("PlayerExit");
	public static bool IsActive => SubworldSystem.IsActive<GlacialRealm>();

	public FightTracker FightTracker;

	public override int Width => 1024;
	public override int Height => 1024;
	public override (int time, bool isDay) ForceTime => (11000, true);
	// public override (int time, bool isDay) ForceTime => ((int)(Main.nightLength * 0.5), false);

	public bool FightActive => FightTracker.GetFirstNPC() is { ModNPC: GlacialBoss { Phase: > 0, CutsceneActive: false } };

	public override List<GenPass> Tasks =>
	[
		new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain)
	];

	public override void OnEnter()
	{
		base.OnEnter();
		
		ArenaCenter.Reset();
		FightTracker = new([ModContent.NPCType<GlacialBoss>()])
		{
			ResetOnVanish = true,
			HaltTimeOnVanish = 60 * 10,
		};
	}
	
	public override void Update()
	{
		FightState state = FightTracker.UpdateState();

		if (ArenaCenter.Get() is not Point16 arenaCenter) { return; }
		if (PlayerExit.Get() is not Point16 playerExit) { return; }

		if (state == FightState.NotStarted)
		{
			// Spawn the boss.
			var spawnPos = (arenaCenter.ToWorldCoordinates() + new Vector2(0, -128)).ToPoint16();
			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), spawnPos.X, spawnPos.Y, ModContent.NPCType<GlacialBoss>());
		}
		else if (state == FightState.JustCompleted)
		{
			IEntitySource src = Entity.GetSource_NaturalSpawn();
			Projectile.NewProjectile(src, playerExit.ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0);
		}

		// Bias the camera towards the middle of the arena.
		// bool inFight = FightActive;
		// for (int i = 0; i < 3; i++)
		// {
		// 	Vector2 point = arenaCenter.ToWorldCoordinates() + new Vector2(0, -900 + (i - 1) * 656 * 2);
		// 	CameraCurios.Create(new()
		// 	{
		// 		Identifier = $"{nameof(GlacialRealm)}_Pane{i + 1}_{(inFight ? "Fight" : "")}",
		// 		Weight = inFight ? 0.2f : 0.45f,
		// 		LengthInSeconds = 1f,
		// 		Position = point,
		// 		Range = new(Min: 200, Max: 1800, Exponent: 1.2f),
		// 	});
		// }
	}

	public override bool ChangeAudio()
	{
		if (FightActive) { return false; }
		
		Main.newMusic = MusicID.OtherworldlyIce;
		Main.curMusic = MusicID.OtherworldlyIce;
		return true;
	}
	void IOverrideBiome.OverrideBiome()
	{
		Main.LocalPlayer.ZoneSnow = true;
		Main.LocalPlayer.ZoneSkyHeight = false;
		Main.LocalPlayer.ZoneMeteor = true;
		Main.windSpeedCurrent = 0;
		Main.windSpeedTarget = 0;
		Main.numClouds = 0;
		Main.cloudBGActive = 0f;
		Main.cloudAlpha = 0f;
		Main.cloudBGAlpha = 0f;
		Main.bgStyle = SurfaceBackgroundID.Snow;
		Main.bloodMoon = false;
	}

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		int lavaLevel = (int)(Height / 3 * 2);

		Main.worldSurface = lavaLevel;
		Main.rockLayer = lavaLevel - 4;

		const string Arena = "Assets/Structures/Glacial/Arena";
		Point16 structSize = StructureTools.GetSize(Arena);
		Point16 structPlacement = new(Main.maxTilesX / 2 + 20, Main.maxTilesY / 2 + 90);
		Point16 structPos = StructureTools.PlaceByOrigin(Arena, structPlacement, new Vector2(0.48f, 0.55f));

		// Generate water.
		for (int x = 0; x < Main.maxTilesX; x++)
		{
			for (int y = 0; y < Main.maxTilesY; y++)
			{
				Tile tile = Main.tile[x, y];
				if (y < lavaLevel || tile.HasTile) { continue; }

				(tile.LiquidType, tile.LiquidAmount) = (LiquidID.Water, byte.MaxValue);
			}
		}

		ArenaCenter.Reset();
		Point16 arenaCenter = ArenaCenter.Get()!.Value;
		Point16 playerSpawn = PlayerSpawn.Get()!.Value;
		
		(Main.spawnTileX, Main.spawnTileY) = (playerSpawn.X, playerSpawn.Y);
	}
}

internal sealed class GlacialRealmRendering : ModSystem
{
	public enum DrawLayer
	{
		BeforeWalls = 1 << 0,
		AfterWalls = 1 << 1,
		BeforeNonSolids = 1 << 2,
		AfterNonSolids = 1 << 3,
		BeforeSolids = 1 << 4,
		AfterSolids = 1 << 5,
	}
	
	private static Asset<Texture2D>? textureThrone;
	private static Asset<Texture2D>? texturePillar;
	private static Asset<Texture2D>? textureBrickPlatform;

	public override void Load()
	{
		On_Main.DoDraw_WallsAndBlacks += (orig, self) =>
		{
			RenderObjects(Main.screenPosition, DrawLayer.BeforeWalls, batchActive: true);
			orig(self);
			RenderObjects(Main.screenPosition, DrawLayer.AfterWalls, batchActive: true);
		};
		On_Main.DoDraw_Tiles_NonSolid += (orig, self) =>
		{
			RenderObjects(Main.screenPosition, DrawLayer.BeforeNonSolids, batchActive: true);
			orig(self);
			RenderObjects(Main.screenPosition, DrawLayer.AfterNonSolids, batchActive: true);
		};
		On_Main.DoDraw_Tiles_Solid += (orig, self) =>
		{
			RenderObjects(Main.screenPosition, DrawLayer.BeforeSolids, batchActive: false);
			orig(self);
			RenderObjects(Main.screenPosition, DrawLayer.AfterSolids, batchActive: false);
		};

		// Disable sun and moon in the realm.
		On_Main.DrawSunAndMoon += (orig, self, sceneArea, moonColor, sunColor, tempMushroomInfluence) =>
		{
			if (GlacialRealm.IsActive)
			{
				// Draw triple sun and triple moon.
				for (int i = 0; i < 3; i++)
				{
					Main.SceneArea usedArea = sceneArea;
					float iFactor = i / 3f;
					Vector2 offset = new Vector2(0, 64) + (Vector2.UnitX * (40 + (i * 20))).RotatedBy(iFactor * MathHelper.TwoPi);
					usedArea.SceneLocalScreenPositionOffset += offset;

					sunColor = new Color(128, 128, 255);
					using var _ = ValueOverride.Create(ref Main.time, 20_000);
					using var __ = ValueOverride.Create(ref Main.dayTime, false);
					using var ___ = ValueOverride.Create(ref Main.moonType, 1 + (int)(i * 1.0f));
					
					orig(self, usedArea, moonColor, sunColor, tempMushroomInfluence);
				}

				return;
			}

			orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);
		};
	}

	public void RenderObjects(Vector2 screenPos, DrawLayer layers, bool batchActive)
	{
		if (!GlacialRealm.IsActive) { return; }
		if (GlacialRealm.ArenaCenter.Get() is not Point16 baseTilePos) { return; }

		Texture2D texThrone = AssetUtils.ImmediateValue($"{nameof(PathOfTerraria)}/Assets/Conflux/InfernalArena_Throne", ref textureThrone);
		Texture2D texPillar = AssetUtils.ImmediateValue($"{nameof(PathOfTerraria)}/Assets/Conflux/InfernalArena_Pillar", ref texturePillar);
		Texture2D texBrickPlatform = AssetUtils.ImmediateValue($"{nameof(PathOfTerraria)}/Assets/Conflux/InfernalArena_BrickPlatform", ref textureBrickPlatform);

		SpriteBatch sb = Main.spriteBatch;
		SpriteBatchArgs sbArgs = !batchActive
			? new(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix)
			: sb.GetArguments();

		if (batchActive) { sb.End(); }

		Matrix matrix = Main.GameViewMatrix.NormalizedTransformationmatrix;
		Effect effect = LitShaders.LitSprite(in matrix);

		float drawScale = 2;
		Vector2 baseWorldPos = baseTilePos.ToWorldCoordinates();

		float wallStep = 656 * drawScale;

		void DrawPillars(Texture2D texture)
		{
			if (!layers.HasFlag(DrawLayer.AfterSolids) && !layers.HasFlag(DrawLayer.BeforeSolids)) { return; }

			using var _ = ValueOverride.Create(ref drawScale, 4);

			for (int ii = 1; ii > 0; ii--)
			{
				// if (Math.Abs(i) == 3 && !layers.HasFlag(DrawLayer.BeforeSolids)) { continue; }

				int i = (ii / 2) * (ii % 2 == 0 ? 1 : -1);

				int dir = Math.Sign(i);
				int indexOnSide = i - dir;
				float xOffset = 400 * ((indexOnSide * 1.00f) + (dir * 0.50f) + (Math.Abs(i) >= 3 ? (-dir * 0.4f) : 0));
				Vector2 pillarPos = baseWorldPos + new Vector2(xOffset - 8, +8);
				var pillarSize = (Vector2)(texture.Size() * drawScale);
				var pillarHalf = (Vector2)(pillarSize * 0.5f);
				var pillarAabb = new Vector4(pillarPos.X - pillarHalf.X, pillarPos.Y - pillarSize.Y, pillarPos.X + pillarHalf.X, pillarPos.Y);

				pillarAabb.Y -= 5378;

				for (int bit = 0; bit < 3; bit++)
				{
					const int tHeight = 18;
					const int bHeight = 70;
					const int tbHeight = tHeight + bHeight;
					var srcRect = (Rectangle)(bit switch
					{
						0 => new(0, 0, texture.Width, tHeight),
						1 => new(0, tHeight, texture.Width, texture.Height - tbHeight),
						2 => new(0, texture.Height - tbHeight, texture.Width, bHeight),
						_ => throw new NotImplementedException(),
					});
					var dstAabb = (Vector4)(bit switch
					{
						0 => new(pillarAabb.X, pillarAabb.Y, pillarAabb.Z, pillarAabb.Y + (tHeight * drawScale)),
						1 => new(pillarAabb.X, pillarAabb.Y + (tHeight * drawScale), pillarAabb.Z, pillarAabb.W - (bHeight * drawScale)),
						2 => new(pillarAabb.X, pillarAabb.W - (bHeight * drawScale), pillarAabb.Z, pillarAabb.W),
						_ => throw new NotImplementedException(),
					});
					var dstRect = new Rectangle((int)dstAabb.X, (int)dstAabb.Y, (int)(dstAabb.Z - dstAabb.X), (int)(dstAabb.W - dstAabb.Y));
					dstRect.X -= (int)Main.screenPosition.X;
					dstRect.Y -= (int)Main.screenPosition.Y;

					sb.Draw(texture, dstRect, srcRect, Color.White, 0f, default, 0, 0);
				}

				// Brick platforms.
				for (int p = 0; p < 20; p++)
				{
					Vector2 worldPos = pillarPos + new Vector2(0, -400 + (-400 * p));
					Vector2 drawOrigin = texBrickPlatform.Size() * new Vector2(0.5f, 1.0f);
					sb.Draw(texBrickPlatform, worldPos - screenPos, null, Color.White, 0f, drawOrigin, drawScale, 0, 0);
				}
			}
		}
		void DrawThrone(Texture2D texture)
		{
			if (!layers.HasFlag(DrawLayer.AfterSolids)) { return; }

			Vector2 worldPos = baseWorldPos + new Vector2(-24, +8);
			Vector2 drawOrigin = texture.Size() * new Vector2(0.5f, 1.0f);
			sb.Draw(texture, worldPos - screenPos, null, Color.White, 0f, drawOrigin, drawScale, 0, 0);
		}

		// Solids
		using (sb.Scope(sbArgs with { SortMode = SpriteSortMode.Deferred, BlendState = BlendState.AlphaBlend, Effect = effect, Matrix = matrix }))
		{
			DrawPillars(texPillar);
			DrawThrone(texThrone);
		}
		// Glowmasks
		// using (sb.Scope(sbArgs with { SortMode = SpriteSortMode.Deferred, BlendState = BlendState.AlphaBlend, Matrix = Main.GameViewMatrix.TransformationMatrix }))
		// {
		// }

		if (batchActive) { sb.Begin(sbArgs); }
	}
}
