using Daybreak.Common.Features.Hooks;
using Humanizer.DateTimeHumanizeStrategy;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Passives.Magic.Masteries;
using PathOfTerraria.Core.Graphics.Particles;
using PathOfTerraria.Core.Tween;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
	Vector2 moveVelocityToTarget = Vector2.Zero;
	readonly float customScale = 0;
	readonly float velocityStretch = 15;
	public override string Texture => $"{PoTMod.ModName}/Assets/Misc/MagicExplosion1";
	
	public MagicParticleGlows(Vector2 position, Vector2 velocity = default, int duration = 15, Color? color = null, Vector2? moveToTarget = null, float? startingScale = null, float? velocityStretch = null)
	{
		Position = position;
		startingVelocity = velocity;
		this.Velocity = velocity;
		this.duration = duration;
		currentDuration = duration;
		if (color.HasValue)
		{
			Color = color.Value;
		}
		if (moveToTarget.HasValue)
		{
			moveVelocityToTarget = moveToTarget.Value;
		}
		if (startingScale.HasValue)
		{
			customScale = startingScale.Value;
		}
		if (velocityStretch.HasValue) 
		{
			this.velocityStretch = velocityStretch.Value;
		}
	}
	
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
	}

	public override bool IsBlendStateAddtive => true;

	public override void Draw()
	{

		Texture2D glowTexture = ModContent.Request<Texture2D>(Texture).Value;
		Vector2 position = Position - Main.screenPosition;
		Main.EntitySpriteDraw(glowTexture, position, null, Color * Opacity, Rotation,glowTexture.Size()/2f,new Vector2(MathHelper.Lerp(1,velocityStretch, Velocity.Length() / startingVelocity.Length()),1) * (Scale * customScale) * 0.075f * new Vector2(1f, 0.5f), SpriteEffects.None);
		Main.EntitySpriteDraw(glowTexture, position, null, Color * Opacity, Rotation,glowTexture.Size()/2f,new Vector2(MathHelper.Lerp(1,velocityStretch, Velocity.Length() / startingVelocity.Length()),1) * (Scale * customScale) * 0.0125f * new Vector2(1f,0.5f),SpriteEffects.None);
		

	}
	public override void Update()
	{
		base.Update();
		if(moveVelocityToTarget == Vector2.Zero)
		{
			Velocity = Vector2.Lerp(Vector2.Zero, startingVelocity * 2, (MathHelper.Clamp((float)(currentDuration * 2) / (duration * 2), 0, 1)));

		}
		else
		{
			Velocity = Velocity.MoveTowards(Position.DirectionTo(moveVelocityToTarget) * startingVelocity.Length() * 10, 1f);
		}
		Rotation = Velocity.ToRotation();
		Scale = ScaleTween.CurrentProgress * 0.25f;
		Opacity = OpacityTween.CurrentProgress;
		currentDuration--;
	}
}
