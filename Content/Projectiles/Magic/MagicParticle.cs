using Daybreak.Common.Features.Hooks;
using PathOfTerraria.Core.Graphics.Particles;
using PathOfTerraria.Core.Tween;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfTerraria.Content.Projectiles.Magic;
public class MagicParticle : ForegroundParticle
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Misc/MagicExplosion1";
	public virtual string GlowTexture => $"{PoTMod.ModName}/Assets/Misc/MagicExplosion2";
	
	public MagicParticle(Vector2 position)
	{
		Position = position;
	}
	public virtual Color MainColor => Color.Turquoise;
	public virtual Color GlowColor => Color.Orange;
	Tween<float> ScaleTween;
	Tween<float> OpacityTween;
	
	public override void Create()
	{
		base.Create();

		ScaleTween = new Tween<float>(MathHelper.Lerp).TweenProperty([
			new(0,1,false,TweenEaseType.CubicInOut,30),
			new(1,1.25f,false,TweenEaseType.None,30)
			]);

		ScaleTween.OnFinsihed += (_) => Destroy();

		OpacityTween = new Tween<float>(MathHelper.Lerp).TweenProperty([
			new(0,1,false,TweenEaseType.CubicInOut,30),
			new(1,0,false,TweenEaseType.CubicInOut,30)
			]);

		Color = MainColor;
	}
	public override void Draw()
	{

		Texture2D glowTexture = ModContent.Request<Texture2D>(GlowTexture).Value;
		Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
		Vector2 position = Position - Main.screenPosition;
		Main.EntitySpriteDraw(texture, position, null, Color * Opacity, Rotation, texture.Size() / 2f, Scale, SpriteEffects.None);
		Main.EntitySpriteDraw(glowTexture, position, null,GlowColor,Rotation,glowTexture.Size()/2f,Scale,SpriteEffects.None);
		

	}
	public override void Update()
	{
		base.Update();

		Scale = ScaleTween.CurrentProgress * 0.25f;
		Opacity = OpacityTween.CurrentProgress;
	}
}
