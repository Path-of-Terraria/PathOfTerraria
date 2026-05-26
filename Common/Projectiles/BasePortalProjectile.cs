using PathOfTerraria.Core.Tween;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Utilities.Terraria;

#nullable enable

namespace PathOfTerraria.Common.Projectiles;

public class BasePortalProjectile : ModProjectile
{

	private Vector2 spawnPosition = Vector2.Zero;

	//animation tweens
	private Tween<Vector2> tweenScale = new(Vector2.Lerp);
	private Tween<float> tweenFlash = new(MathHelper.Lerp);

	public override string Texture => $"{PoTMod.ModName}/Assets/Misc/VFX/PortalParts/PortalBase";

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = int.MaxValue;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(64, 64);
		Projectile.netImportant = true;
		Projectile.hide = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void OnSpawn(IEntitySource source)
	{
		spawnPosition = Projectile.Center;
		tweenScale = new Tween<Vector2>(Vector2.Lerp).Start(
			[
			new(new Vector2(2,0),new Vector2(.5f,1),false,TweenEaseType.CubicInOut,8),
			new(new Vector2(.5f,1),new Vector2(1,1),false,TweenEaseType.CubicInOut,7),

			]);
		tweenFlash = new Tween<float>(MathHelper.Lerp).Start(
			[
			new(1,1,false,TweenEaseType.CubicInOut,7),
			new(1,.5f,false,TweenEaseType.CubicInOut,4),
			new(.5f,0f,false,TweenEaseType.CubicInOut,3)
			]);
	}

	public override void AI()
	{
		Projectile.rotation -= MathHelper.TwoPi / 129;
		Projectile.Center = new Vector2(MathF.Floor(spawnPosition.X * 2f) / 2f, MathF.Floor(spawnPosition.Y * 2f) / 2f - 1); // the minus one here is important
		Projectile.Center += new Vector2(0, MathF.Sin(Main.GameUpdateCount * 0.1f) * 1); // the minus one here is important
		Projectile.velocity *= 0.89f;

		if (Projectile.ai[0] == 1)
		{
			tweenScale = new Tween<Vector2>(Vector2.Lerp).Start(
			[
				new(new Vector2(1,1),new Vector2(.5f,1),false,TweenEaseType.CubicInOut,7),
				new(new Vector2(.5f,1),new Vector2(0,2),false,TweenEaseType.CubicInOut,7),

			]);
			tweenScale.OnFinished += (_) => Projectile.Kill();
			tweenFlash = new Tween<float>(MathHelper.Lerp).Start(
			[
				new(0,1,false,TweenEaseType.CubicInOut,7),
				new(1,1,false,TweenEaseType.CubicInOut,7),
			]);
			Projectile.ai[0] = 2;
		}
	}

	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
		Effect effectBase = AssetUtils.ImmediateValue<Effect>($"{PoTMod.ModName}/Assets/Effects/Portal");
		Texture2D portalBaseTexture = AssetUtils.ImmediateValue<Texture2D>($"{PoTMod.ModName}/Assets/Misc/VFX/PortalParts/PortalBase");
		Texture2D portalVortexTexture = AssetUtils.ImmediateValue<Texture2D>($"{PoTMod.ModName}/Assets/Misc/VFX/PortalParts/PortalVortex");
		Texture2D paletteTexture = AssetUtils.ImmediateValue<Texture2D>($"{PoTMod.ModName}/Assets/Palettes/Portal0Palette");
		Texture2D ditherTexture = AssetUtils.ImmediateValue<Texture2D>($"{PoTMod.ModName}/Assets/Misc/BayerMatrix8x8");

		Vector2 animScale = tweenScale.CurrentProgress;
		float customScale = 0.275f;
		float vanillaPixelScale = 2;
		float pixelSize = portalVortexTexture.Size().Length() / (animScale.Length() / customScale) / vanillaPixelScale;
		GraphicsDevice gd = Main.graphics.GraphicsDevice;
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		gd.Textures[1] = paletteTexture;
		gd.Textures[2] = ditherTexture;
		effectBase.Parameters["data"].SetValue(new Vector4(1, tweenFlash.CurrentProgress, 0.96f, 0.1f));
		effectBase.Parameters["rotation"].SetValue(Projectile.rotation);
		effectBase.Parameters["canvasSize"].SetValue(portalVortexTexture.Size());
		effectBase.Parameters["pixelSize"].SetValue(pixelSize);
		effectBase.Parameters["transform"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
		effectBase.Parameters["uTime"].SetValue((Main.GameUpdateCount));
		effectBase.Parameters["screenRes"].SetValue(Main.ScreenSize.ToVector2());
		effectBase.Parameters["paletteColorsAmount"].SetValue(8f);
		effectBase.Parameters["ditherSize"].SetValue(8);
		effectBase.Parameters["additionalScale"].SetValue(animScale.Length());
		effectBase.CurrentTechnique.Passes[0].Apply();

		Main.EntitySpriteDraw(portalBaseTexture, Projectile.Center - Main.screenPosition, null, Color.White, 0, portalBaseTexture.Size() / 2f, animScale * customScale, SpriteEffects.None);
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

	}
	public override bool PreDraw(ref Color lightColor)
	{
		return false;
	}
	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White;
	}
}
