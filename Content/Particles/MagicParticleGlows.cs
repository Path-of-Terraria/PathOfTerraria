using Daybreak.Common.Features.Hooks;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Core.Graphics.Particles;
using PathOfTerraria.Core.Tween;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PathOfTerraria.Content.Particles;
public class MagicParticleGlows : ForegroundParticle
{
	Tween<float> ScaleTween;
	Tween<float> OpacityTween;
	Vector2 startingVelocity = Vector2.Zero;
	readonly int duration = 15;
	int currentDuration = 15;
	public override string Texture => $"{PoTMod.ModName}/Assets/Misc/MagicExplosion1";
	
	public MagicParticleGlows(Vector2 position, Vector2 velocity = default, int duration = 15)
	{
		Position = position;
		startingVelocity = velocity;
		this.Velocity = velocity;
		this.duration = duration;
		currentDuration = duration;
	}
	public virtual Color MainColor => Color.White;
	public virtual Color GlowColor => Color.White;
	
	public override void Create()
	{
		base.Create();

		ScaleTween = new Tween<float>(MathHelper.Lerp).TweenProperty([
			new(0,1,false,TweenEaseType.CubicInOut,duration / 2),
			new(1,0f,false,TweenEaseType.None,duration / 2)
			]);

		ScaleTween.OnFinsihed += (_) => Destroy();

		OpacityTween = new Tween<float>(MathHelper.Lerp).TweenProperty([
			new(4,1f,false,TweenEaseType.None,duration / 2),
			new(1f,1,false,TweenEaseType.CubicInOut,duration / 2)
			]);
		Rotation = Velocity.ToRotation();
		Color = MainColor;
	}

	public override bool IsBlendStateAddtive => true;

	public override void Draw()
	{

		Texture2D glowTexture = ModContent.Request<Texture2D>(Texture).Value;
		Vector2 position = Position - Main.screenPosition;
		Main.EntitySpriteDraw(glowTexture, position, null, GlowColor * Opacity, Rotation,glowTexture.Size()/2f,new Vector2(MathHelper.Lerp(1,15, Velocity.Length() / startingVelocity.Length()),1) * Scale * 0.075f * new Vector2(1f, 0.5f), SpriteEffects.None);
		Main.EntitySpriteDraw(glowTexture, position, null, GlowColor * Opacity, Rotation,glowTexture.Size()/2f,new Vector2(MathHelper.Lerp(1,15, Velocity.Length() / startingVelocity.Length()),1) * Scale * 0.0125f * new Vector2(1f,0.5f),SpriteEffects.None);
		

	}
	public override void Update()
	{
		base.Update();
		Velocity = Vector2.Lerp(Vector2.Zero, startingVelocity * 2, (MathHelper.Clamp((float)(currentDuration * 2) / (duration * 2),0,1)));
		Scale = ScaleTween.CurrentProgress * 0.25f;
		Opacity = OpacityTween.CurrentProgress;
		currentDuration--;
	}
}
