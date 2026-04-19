using System.Collections.Generic;
using System.IO;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Core.Camera;
using PathOfTerraria.Core.Lighting;
using PathOfTerraria.Core.Structures;
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

internal sealed class InfernalRealm : BossDomainSubworld, IOverrideBiome
{
	internal sealed class InfernalRealmSystem : ModSystem
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
	public static bool IsActive => SubworldSystem.IsActive<InfernalRealm>();

	public FightTracker FightTracker;

	public override int Width => 768;
	public override int Height => 768;
	public override (int time, bool isDay) ForceTime => (49000, true);
	// public override (int time, bool isDay) ForceTime => ((int)(Main.nightLength * 0.5), false);

	public bool FightActive => FightTracker.GetFirstNPC() is { ModNPC: InfernalBoss { Phase: > 0, CutsceneActive: false } };

	public override List<GenPass> Tasks =>
	[
		new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain)
	];

	public override void OnEnter()
	{
		base.OnEnter();
		
		ArenaCenter.Reset();
		FightTracker = new([ModContent.NPCType<InfernalBoss>()])
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
			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), spawnPos.X, spawnPos.Y, ModContent.NPCType<InfernalBoss>());
		}
		else if (state == FightState.JustCompleted)
		{
			IEntitySource src = Entity.GetSource_NaturalSpawn();
			Projectile.NewProjectile(src, playerExit.ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0);
		}

		bool inFight = FightActive;
		// Bias the camera towards the arena's mozaic windows.
		for (int i = 0; i < 3; i++)
		{
			Vector2 point = arenaCenter.ToWorldCoordinates() + new Vector2((i - 1) * 656 * 2, -900);
			CameraCurios.Create(new()
			{
				Identifier = $"{nameof(InfernalRealm)}_Pane{i + 1}_{(inFight ? "Fight" : "")}",
				Weight = inFight ? 0.2f : 0.45f,
				LengthInSeconds = 1f,
				Position = point,
				Range = new(Min: 200, Max: 1800, Exponent: 1.2f),
			});
		}
	}

	public override bool ChangeAudio()
	{
		if (FightActive) { return false; }
		
		Main.newMusic = MusicID.OtherworldlyCorruption;
		Main.curMusic = MusicID.OtherworldlyCorruption;
		return true;
	}
	void IOverrideBiome.OverrideBiome()
	{
		Main.LocalPlayer.ZoneSkyHeight = true;
		Main.LocalPlayer.ZoneMeteor = true;
		Main.windSpeedCurrent = 0;
		Main.windSpeedTarget = 0;
		Main.numClouds = 0;
		Main.cloudBGActive = 0f;
		Main.cloudAlpha = 0f;
		Main.cloudBGAlpha = 0f;
		Main.bgStyle = int.MaxValue;
		Main.bloodMoon = true;
	}

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		int lavaLevel = (int)(Height / 3 * 2);

		Main.worldSurface = lavaLevel;
		Main.rockLayer = lavaLevel - 4;

		const string Arena = "Assets/Structures/Infernal/Arena";
		Point16 structSize = StructureTools.GetSize(Arena);
		Point16 structPlacement = new(Main.maxTilesX / 2 + 20, Main.maxTilesY / 2 + 90);
		Point16 structPos = StructureTools.PlaceByOrigin(Arena, structPlacement, new Vector2(0.48f, 0.55f));

		// Generate lava.
		for (int x = 0; x < Main.maxTilesX; x++)
		{
			for (int y = 0; y < Main.maxTilesY; y++)
			{
				Tile tile = Main.tile[x, y];
				if (y < lavaLevel || tile.HasTile) { continue; }

				(tile.LiquidType, tile.LiquidAmount) = (LiquidID.Lava, byte.MaxValue);
			}
		}

		ArenaCenter.Reset();
		Point16 arenaCenter = ArenaCenter.Get()!.Value;
		Point16 playerSpawn = PlayerSpawn.Get()!.Value;
		
		(Main.spawnTileX, Main.spawnTileY) = (playerSpawn.X, playerSpawn.Y);
	}
}

internal sealed class InfernalRealmRendering : ModSystem
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
	private static Asset<Texture2D>? textureWall;
	private static Asset<Texture2D>? texturePane;
	private static Asset<Texture2D>? texturePillar;
	private static Asset<Texture2D>? textureBrickPlatform;
	private static Asset<Texture2D>? textureChandelier;
	private static Asset<Texture2D>? textureChandelierGlow;

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
			if (InfernalRealm.IsActive) { return; }
			orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);
		};
	}

	public void RenderObjects(Vector2 screenPos, DrawLayer layers, bool batchActive)
	{
		if (!InfernalRealm.IsActive) { return; }
		if (InfernalRealm.ArenaCenter.Get() is not Point16 baseTilePos) { return; }

		Texture2D texThrone = AssetUtils.ImmediateValue($"{nameof(PathOfTerraria)}/Assets/Conflux/InfernalArena_Throne", ref textureThrone);
		Texture2D texWall = AssetUtils.ImmediateValue($"{nameof(PathOfTerraria)}/Assets/Conflux/InfernalArena_Wall", ref textureWall);
		Texture2D texPane = AssetUtils.ImmediateValue($"{nameof(PathOfTerraria)}/Assets/Conflux/InfernalArena_Pane", ref texturePane);
		Texture2D texPillar = AssetUtils.ImmediateValue($"{nameof(PathOfTerraria)}/Assets/Conflux/InfernalArena_Pillar", ref texturePillar);
		Texture2D texChandelier = AssetUtils.ImmediateValue($"{nameof(PathOfTerraria)}/Assets/Conflux/InfernalArena_Chandelier", ref textureChandelier);
		Texture2D texChandelierGlow = AssetUtils.ImmediateValue($"{nameof(PathOfTerraria)}/Assets/Conflux/InfernalArena_Chandelier_Glow", ref textureChandelierGlow);
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

		void DrawWalls(Texture2D texture)
		{
			if (!layers.HasFlag(DrawLayer.BeforeWalls)) { return; }

			// Solid color entrance walls.
			var entranceSrcRect = new Rectangle(0, 64, 1, 1);
			for (int i = 0; i < 2; i++)
			{
				int xDir = i == 0 ? -1 : 1;
				var dstRect = new Rectangle
				(
					(int)(baseWorldPos.X + (wallStep * 2 * xDir) - 8 - texture.Width - screenPos.X),
					(int)(baseWorldPos.Y - 1336 - screenPos.Y),
					(int)(wallStep),
					(int)(texPillar.Height * drawScale)
				);
				sb.Draw(texture, dstRect, entranceSrcRect, Color.White, 0f, default, 0, 0);
			}
			// Window walls.
			for (int i = -1; i <= +1; i++)
			{
				float xOffset = wallStep * (i * 1.00f);
				Vector2 position = baseWorldPos + new Vector2(xOffset - 8, -648);
				Vector2 drawOrigin = texture.Size() * new Vector2(0.5f, 1.0f);
				sb.Draw(texture, position - screenPos, null, Color.White, 0f, texture.Size() * 0.5f, drawScale, 0, 0);
			}
		}
		void DrawWindows(Texture2D texture)
		{
			if (!layers.HasFlag(DrawLayer.BeforeWalls)) { return; }
			
			for (int i = -1; i <= +1; i++)
			{
				float xOffset = wallStep * (i * 1.00f);
				Vector2 position = baseWorldPos + new Vector2(xOffset - 10, -566);
				Vector2 drawOrigin = texture.Size() * new Vector2(0.5f, 1.0f);
				sb.Draw(texture, position - screenPos, null, Color.White, 0f, texture.Size() * 0.5f, drawScale, 0, 0);
			}
		}
		void DrawPillars(Texture2D texture)
		{
			if (!layers.HasFlag(DrawLayer.AfterSolids) && !layers.HasFlag(DrawLayer.BeforeSolids)) { return; }

			for (int i = -3; i <= +3; i += (i == -1 ? 2 : 1))
			{
				if (Math.Abs(i) == 3 && !layers.HasFlag(DrawLayer.BeforeSolids)) { continue; }

				int dir = Math.Sign(i);
				int indexOnSide = i - dir;
				float xOffset = wallStep * ((indexOnSide * 1.00f) + (dir * 0.50f) + (Math.Abs(i) >= 3 ? (-dir * 0.4f) : 0));
				Vector2 pillarPos = baseWorldPos + new Vector2(xOffset - 8, +8);
				var pillarSize = (Vector2)(texture.Size() * drawScale);
				var pillarHalf = (Vector2)(pillarSize * 0.5f);
				var pillarAabb = new Vector4(pillarPos.X - pillarHalf.X, pillarPos.Y - pillarSize.Y, pillarPos.X + pillarHalf.X, pillarPos.Y);

				if (Math.Abs(i) != 1)
				{
					pillarAabb.W -= 638;
				}

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
				if (Math.Abs(i) == 1)
				{
					for (int p = 0; p < 2; p++)
					{
						Vector2 worldPos = pillarPos + new Vector2(0, -400 + (-400 * p));
						Vector2 drawOrigin = texBrickPlatform.Size() * new Vector2(0.5f, 1.0f);
						sb.Draw(texBrickPlatform, worldPos - screenPos, null, Color.White, 0f, drawOrigin, drawScale, 0, 0);
					}
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
		void DrawChandelier(Texture2D texture, bool light)
		{
			if (!layers.HasFlag(DrawLayer.AfterSolids)) { return; }

			var srcRect = new Rectangle(249, 0, 248, 219);

			for (int i = -1; i <= +1; i++)
			{
				float xOffset = wallStep * i;
				float yOffset = -238;
				Vector2 worldPos = baseWorldPos + new Vector2(-8 + xOffset, -1098 + yOffset);
				Vector2 drawOrigin = srcRect.Size() * new Vector2(0.5f, 0.0f);
				float rotation = 0f;

				sb.Draw(texture, worldPos - screenPos, srcRect, Color.White, rotation, drawOrigin, drawScale, 0, 0);

				if (light)
				{
					Vector3 lightVec = Vector3.Lerp(Color.Orange.ToVector3(), Vector3.One, 0.5f) * 1.25f;
					for (int j = -11; j <= +11; j++)
					{
						if (Math.Abs(j) is not 2 or 6 or 9 or 11) { continue; }

						var offset = new Vector2(j * 20, 410);
						var lightPos = (Vector2)(worldPos + ((rotation + MathHelper.PiOver2).ToRotationVector2() * offset));
						Lighting.AddLight(lightPos, lightVec);
					}
				}
			}
		}

		// Glass
		using (sb.Scope(sbArgs with { SortMode = SpriteSortMode.Deferred, BlendState = BlendState.Additive, Effect = effect, Matrix = matrix }))
		{
			DrawWindows(texPane);
		}
		// Solids
		using (sb.Scope(sbArgs with { SortMode = SpriteSortMode.Deferred, BlendState = BlendState.AlphaBlend, Effect = effect, Matrix = matrix }))
		{
			DrawWalls(texWall);
			DrawPillars(texPillar);
			DrawThrone(texThrone);
			DrawChandelier(texChandelier, false);
		}
		// Glowmasks
		using (sb.Scope(sbArgs with { SortMode = SpriteSortMode.Deferred, BlendState = BlendState.AlphaBlend, Matrix = Main.GameViewMatrix.TransformationMatrix }))
		{
			DrawChandelier(texChandelierGlow, true);
		}

		if (batchActive) { sb.Begin(sbArgs); }
	}
}
