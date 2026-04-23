using Microsoft.Xna.Framework.Graphics;
using MonoMod.Core.Platforms;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.Systems.MapContent;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Particles;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Core.Graphics;
using PathOfTerraria.Core.Graphics.Particles;
using PathOfTerraria.Core.Tween;
using ReLogic.Content;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Projectiles;
public class BasePortalProjectile : ModProjectile
{
	private static Asset<Effect> portalTailEffect = null;
	private static Asset<Texture2D> portalBaseTexture = null;
	private static Asset<Texture2D> portalVortexTexture = null;
	private static Asset<Texture2D> portalTailTexture = null;
	private static Asset<Texture2D> paletteTexture = null;
	private static Asset<Texture2D> ditherTexture = null;
	private Vector2 spawnPosition = Vector2.Zero;

	//animation tweens
	private Tween<Vector2> tweenScale;
	private Tween<float> tweenFlash;

	public override string Texture => $"{PoTMod.ModName}/Assets/Misc/VFX/PortalParts/PortalBase";
	public override void SetStaticDefaults()
	{
		portalTailEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/Portal");
		portalBaseTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/VFX/PortalParts/PortalBase");
		portalVortexTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/VFX/PortalParts/PortalVortex");
		portalTailTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/VFX/PortalParts/PortalTail");
		paletteTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Palettes/Portal0Palette");
		ditherTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/BayerMatrix512x512");

	}
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
		tweenScale = new Tween<Vector2>(Vector2.Lerp).TweenProperty(
			[
			new(new Vector2(2,0),new Vector2(.5f,1),false,TweenEaseType.CubicInOut,8),
			new(new Vector2(.5f,1),new Vector2(1,1),false,TweenEaseType.CubicInOut,7),

			]);
		tweenFlash = new Tween<float>(MathHelper.Lerp).TweenProperty(
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
		Projectile.Center += new Vector2(0,MathF.Sin(Main.GameUpdateCount * 0.1f) * 1); // the minus one here is important
		Projectile.velocity *= 0.89f;
		//for (int i = 0; i < 1; i++)
		//{
		//	Vector2 pos = Projectile.Center + new Vector2(125, 0).RotatedByRandom(MathHelper.TwoPi);
		//	MagicParticleGlows p = new(pos, pos.DirectionTo(Projectile.Center) * 5,6, Color.Turquoise);
		//	ParticleSystem.Create(p);
		//}

		if (Projectile.ai[0] == 1)
		{
			tweenScale = new Tween<Vector2>(Vector2.Lerp).TweenProperty(
			[
				new(new Vector2(1,1),new Vector2(.5f,1),false,TweenEaseType.CubicInOut,7),
				new(new Vector2(.5f,1),new Vector2(0,2),false,TweenEaseType.CubicInOut,7),

			]);
			tweenScale.OnFinsihed += (_) => Projectile.Kill();
			tweenFlash = new Tween<float>(MathHelper.Lerp).TweenProperty(
			[
				new(0,1,false,TweenEaseType.CubicInOut,7),
				new(1,1,false,TweenEaseType.CubicInOut,7),
			]);
			Projectile.ai[0] = 2;
		}

	}
	private static GraphicsDevice gd => Main.graphics.GraphicsDevice;
	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
		Vector2 additionalScale = tweenScale.CurrentProgress * 0.5f;

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
		Vector2 scaleForPixelCalc = Vector2.Zero;
		Effect effectBase = portalTailEffect.Value;

		float pixelSize = 256 / 4f;
		gd.Textures[1] = paletteTexture.Value;
		gd.Textures[2] = ditherTexture.Value;
		effectBase.Parameters["data"].SetValue(new Vector4(1, tweenFlash.CurrentProgress, 0.96f, 0.1f));
		effectBase.Parameters["rotation"].SetValue(Projectile.rotation);
		effectBase.Parameters["canvasSize"].SetValue(portalVortexTexture.Size());
		effectBase.Parameters["pixelSize"].SetValue(pixelSize);
		effectBase.Parameters["transform"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
		effectBase.Parameters["uTime"].SetValue((Main.GameUpdateCount));
		effectBase.Parameters["screenRes"].SetValue(Main.ScreenSize.ToVector2());
		effectBase.Parameters["paletteColorsAmount"].SetValue(8f);

		effectBase.Parameters["additionalScale"].SetValue(additionalScale.Length());

		effectBase.CurrentTechnique.Passes[0].Apply();
		Main.EntitySpriteDraw(portalBaseTexture.Value, Projectile.Center - Main.screenPosition, null, Color.White, 0, portalBaseTexture.Size() / 2f, additionalScale / 2f, SpriteEffects.None);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

	}
	public override bool PreDraw(ref Color lightColor)
	{
		return false;
	}
	public override void PostDraw(Color lightColor)
	{

	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White;
	}
}
