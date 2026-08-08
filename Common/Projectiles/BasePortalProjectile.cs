using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using System.Collections.Generic;
using Terraria.DataStructures;

#nullable enable

namespace PathOfTerraria.Common.Projectiles;

/// <summary>Client-local visual for an active map device portal.</summary>
public sealed class BasePortalProjectile : ModProjectile
{
	private const int ClosingDuration = 14;
	private const float DrawScale = 0.275f;

	private readonly Tween<Vector2> scaleTween = new(Vector2.Lerp);
	private readonly Tween<float> flashTween = new(MathHelper.Lerp);
	private Vector2 spawnPosition;

	public int MapDeviceEntityId => (int)Projectile.ai[1];

	public override string Texture => $"{PoTMod.ModName}/Assets/Misc/VFX/PortalParts/PortalBase";

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(64f);
		Projectile.hide = true;
		Projectile.netImportant = false;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void OnSpawn(IEntitySource source)
	{
		spawnPosition = Projectile.Center;
		scaleTween.Start(
			new(new Vector2(2f, 0f), new Vector2(0.5f, 1f), TweenEaseType.CubicInOut, 8),
			new(new Vector2(0.5f, 1f), Vector2.One, TweenEaseType.CubicInOut, 7));
		flashTween.Start(
			new(1f, 1f, TweenEaseType.CubicInOut, 7),
			new(1f, 0.5f, TweenEaseType.CubicInOut, 4),
			new(0.5f, 0f, TweenEaseType.CubicInOut, 3));
	}

	public override void AI()
	{
		if (Projectile.ai[0] == 1f)
		{
			BeginClosing();
		}

		scaleTween.Update();
		flashTween.Update();

		Projectile.rotation -= MathHelper.TwoPi / 129f;
		Projectile.Center = new Vector2(
			MathF.Floor(spawnPosition.X * 2f) / 2f,
			MathF.Floor(spawnPosition.Y * 2f) / 2f - 1f + MathF.Sin(Main.GameUpdateCount * 0.1f));

		if (Projectile.ai[0] == 2f && scaleTween.IsComplete)
		{
			Projectile.Kill();
		}
	}

	private void BeginClosing()
	{
		scaleTween.Start(
			new(scaleTween.Value, new Vector2(0.5f, 1f), TweenEaseType.CubicInOut, 7),
			new(new Vector2(0.5f, 1f), new Vector2(0f, 2f), TweenEaseType.CubicInOut, 7));
		flashTween.Start(
			new(flashTween.Value, 1f, TweenEaseType.CubicInOut, 7),
			new(1f, 1f, TweenEaseType.Linear, 7));
		Projectile.ai[0] = 2f;
		Projectile.timeLeft = ClosingDuration + 2;
	}

	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
		behindNPCsAndTiles.Add(index);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Effect effect = AssetUtils.ImmediateValue<Effect>($"{PoTMod.ModName}/Assets/Effects/Portal");
		Texture2D portalTexture = AssetUtils.ImmediateValue<Texture2D>(Texture);
		Texture2D paletteTexture = AssetUtils.ImmediateValue<Texture2D>($"{PoTMod.ModName}/Assets/Palettes/Portal0Palette");
		Texture2D ditherTexture = AssetUtils.ImmediateValue<Texture2D>($"{PoTMod.ModName}/Assets/Misc/BayerMatrix8x8");

		Vector2 animationScale = scaleTween.Value;
		float pixelSize = portalTexture.Size().Length() / (animationScale.Length() / DrawScale) / 2f;
		GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;

		using SpriteBatchOverride _ = Main.spriteBatch.Override(new(
			SpriteSortMode.Immediate,
			BlendState.AlphaBlend,
			Main.DefaultSamplerState,
			DepthStencilState.None,
			Main.Rasterizer,
			effect,
			Main.GameViewMatrix.TransformationMatrix));

		graphicsDevice.Textures[1] = paletteTexture;
		graphicsDevice.Textures[2] = ditherTexture;
		effect.Parameters["data"].SetValue(new Vector4(1f, flashTween.Value, 0.96f, 0.1f));
		effect.Parameters["rotation"].SetValue(Projectile.rotation);
		effect.Parameters["pixelSize"].SetValue(pixelSize);
		effect.Parameters["transform"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
		effect.Parameters["uTime"].SetValue(Main.GameUpdateCount);
		effect.Parameters["paletteColorsAmount"].SetValue(8f);
		effect.Parameters["ditherSize"].SetValue(8f);

		Main.EntitySpriteDraw(
			portalTexture,
			Projectile.Center - Main.screenPosition,
			null,
			Color.White,
			0f,
			portalTexture.Size() / 2f,
			animationScale * DrawScale,
			SpriteEffects.None);

		return false;
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White;
	}
}