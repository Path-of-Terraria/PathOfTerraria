using PathOfTerraria.Core.Graphics.Particles;
using PathOfTerraria.Core.Tween;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfTerraria.Content.Particles;
public class MagicExplosionParticle : ForegroundParticle
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Misc/MagicExplosion1";
	
	public MagicExplosionParticle(Vector2 position)
	{
		Position = position;
	}
	public virtual Color MainColor => Color.White;
	public virtual Color GlowColor => Color.White;
	Tween<float> ScaleTween;
	Tween<float> OpacityTween;
	
	public override void Create()
	{
		base.Create();

		ScaleTween = new Tween<float>(MathHelper.Lerp).TweenProperty([
			new(0,1,false,TweenEaseType.CubicInOut,28),
			new(1,1.25f,false,TweenEaseType.None,2)
			]);

		ScaleTween.OnFinsihed += (_) => Destroy();

		OpacityTween = new Tween<float>(MathHelper.Lerp).TweenProperty([
			new(0,.5f,false,TweenEaseType.CubicInOut,7),
			new(1f,0,false,TweenEaseType.CubicInOut,7)
			]);

		Color = MainColor;
	}

	public override bool IsBlendStateAddtive => true;

	public override void Draw()
	{

		Texture2D glowTexture = ModContent.Request<Texture2D>(Texture).Value;
		Vector2 position = Position - Main.screenPosition;
		Main.EntitySpriteDraw(glowTexture, position, null, GlowColor * Opacity, Rotation,glowTexture.Size()/2f,Scale* 1f,SpriteEffects.None);

		

	}
	public override void Update()
	{
		base.Update();

		Scale = ScaleTween.CurrentProgress;
		Opacity = OpacityTween.CurrentProgress;
	}
}
