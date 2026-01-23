using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Content.Conflux;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using ReLogic.Graphics;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.Utilities;

#nullable enable

namespace PathOfTerraria.Common.Conflux;

/// <summary> This system spawns rifts in domain worlds. </summary>
internal sealed class ConfluxRifts : ModSystem
{
	private static bool checkedRifts;
	private static float progressBarAlpha;
	private static float progressBarProgress;
	private static float progressBarPulse;
	private static Asset<Texture2D>? progressBarTexture;
	private static Asset<Texture2D>? progressBarOutline;
	private static GameInterfaceLayer? progressBarLayer;

	public override void ClearWorld()
	{
		checkedRifts = false;
	}

	public override void PostUpdateWorld()
	{
		if (!checkedRifts && Main.ActiveNPCs.GetEnumerator().MoveNext())
		{
			if (ShouldRiftsSpawnInWorld(SubworldSystem.Current))
			{
				SpawnRifts();
			}

			checkedRifts = true;
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
		layers.Insert(Math.Max(0, layers.FindIndex(l => l.Name.Equals("Vanilla: Mouse Text")) - 1), progressBarLayer);
	}

	public static bool ShouldRiftsSpawnInWorld(Subworld world)
	{
		return world is MappingWorld and IExplorationWorld and not RavencrestSubworld;
	}

	/// <summary> Attempts to spawn conflux rifts, returns the amount created. </summary>
	public static int SpawnRifts()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) { return 0; }

		const int worldEdgeOffset = 40;
		const int freeSpaceInTiles = 10;
		const float minDistanceFromRifts = 2048f;
		const float minDistanceFromRiftsSqr = minDistanceFromRifts * minDistanceFromRifts;

		var placement = new SpawnPlacement
		{
			Area = new Rectangle(worldEdgeOffset, worldEdgeOffset, Main.maxTilesX - (worldEdgeOffset * 2), Main.maxTilesY - (worldEdgeOffset * 2)),
			CollisionSize = new(freeSpaceInTiles * TileUtils.TileSizeInPixels, freeSpaceInTiles * TileUtils.TileSizeInPixels),
			OnGround = true,
			MinDistanceFromEnemies = 0f,
			MinDistanceFromPlayers = 2048f,
			MaxSearchAttempts = 4096,
			SkippedLiquids = LiquidMask.All
		};

		WeightedRandom<int> targetRiftPool = new();
		targetRiftPool.Add(3, 0.1f + (MathF.Pow(MappingWorld.MapTier - 1.0f, 2.50f) * 0.3f));
		targetRiftPool.Add(2, 0.3f + (MathF.Pow(MappingWorld.MapTier - 1.0f, 2.05f) * 0.6f));
		targetRiftPool.Add(1, 0.7f);

		int targetRifts = targetRiftPool.Get();
		IEntitySource? source = Entity.GetSource_None();
		var rifts = new List<Projectile>(capacity: targetRifts);

		for (int i = 0; i < targetRifts; i++)
		{
			if (!EnemySpawning.TryFindingSpawnPosition(out Vector2 position, in placement))
			{
				break;
			}

			if (rifts.Any(p => p.DistanceSQ(position) < minDistanceFromRiftsSqr))
			{
				continue;
			}

			position.Y += 5 * TileUtils.PixelSizeInUnits;

			int type = (i % ((int)ConfluxRiftKind.Count)) switch
			{
				0 => ModContent.ProjectileType<GlacialRift>(),
				1 => ModContent.ProjectileType<InfernalRift>(),
				2 => ModContent.ProjectileType<CelestialRift>(),
				_ => throw new NotImplementedException(),
			};
			var rift = Projectile.NewProjectileDirect(source, position, Vector2.Zero, type, 0, 0f);

			rifts.Add(rift);
		}

		return rifts.Count;
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
