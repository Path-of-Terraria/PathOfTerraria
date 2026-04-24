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
	private int _currentDuration = 15;
	private Tween<float> _ScaleTween;
	private Tween<float> _OpacityTween;
	private readonly Vector2 _startingVelocity = Vector2.Zero;
	private readonly int _duration = 15;
	private readonly Vector2 _moveVelocityToTarget = Vector2.Zero;
	private readonly float _customScale = 0;
	private readonly float _velocityStretch = 15;
	public override string Texture => $"{PoTMod.ModName}/Assets/Misc/MagicExplosion1";
	
	public MagicParticleGlows(Vector2 position, Vector2 velocity = default, int duration = 15, Color? color = null, Vector2? moveToTarget = null, float? startingScale = null, float? velocityStretch = null)
	{
		Position = position;
		_startingVelocity = velocity;
		this.Velocity = velocity;
		this._duration = duration;
		_currentDuration = duration;
		if (color.HasValue)
		{
			Color = color.Value;
		}
		if (moveToTarget.HasValue)
		{
			_moveVelocityToTarget = moveToTarget.Value;
		}
		if (startingScale.HasValue)
		{
			_customScale = startingScale.Value;
		}
		if (velocityStretch.HasValue) 
		{
			this._velocityStretch = velocityStretch.Value;
		}
	}
	
	public override void Create()
	{
		base.Create();
		
		_ScaleTween = new Tween<float>(MathHelper.Lerp).TweenProperty([
			new(0,1,false,TweenEaseType.CubicInOut,_duration / 2),
			new(1,0f,false,TweenEaseType.None,_duration / 2)
			]);

		_ScaleTween.OnFinsihed += (_) => Destroy();

		_OpacityTween = new Tween<float>(MathHelper.Lerp).TweenProperty([
			new(4,1f,false,TweenEaseType.None,_duration / 2),
			new(1f,1,false,TweenEaseType.CubicInOut,_duration / 2)
			]);
		Rotation = Velocity.ToRotation();
	}

	public override bool IsBlendStateAddtive => true;

	public override void Draw()
	{

		Texture2D glowTexture = ModContent.Request<Texture2D>(Texture).Value;
		Vector2 position = Position - Main.screenPosition;
		Main.EntitySpriteDraw(glowTexture, position, null, Color * Opacity, Rotation,glowTexture.Size()/2f,new Vector2(MathHelper.Lerp(1,_velocityStretch, Velocity.Length() / _startingVelocity.Length()),1) * (Scale * _customScale) * 0.075f * new Vector2(1f, 0.5f), SpriteEffects.None);
		Main.EntitySpriteDraw(glowTexture, position, null, Color * Opacity, Rotation,glowTexture.Size()/2f,new Vector2(MathHelper.Lerp(1,_velocityStretch, Velocity.Length() / _startingVelocity.Length()),1) * (Scale * _customScale) * 0.0125f * new Vector2(1f,0.5f),SpriteEffects.None);
		

	}
	public override void Update()
	{
		base.Update();
		if(_moveVelocityToTarget == Vector2.Zero)
		{
			Velocity = Vector2.Lerp(Vector2.Zero, _startingVelocity * 2, (MathHelper.Clamp((float)(_currentDuration * 2) / (_duration * 2), 0, 1)));
		}
		else
		{
			Velocity = Velocity.MoveTowards(Position.DirectionTo(_moveVelocityToTarget) * _startingVelocity.Length() * 10, 1f);
		}
		Rotation = Velocity.ToRotation();
		Scale = _ScaleTween.CurrentProgress * 0.25f;
		Opacity = _OpacityTween.CurrentProgress;
		_currentDuration--;
	}
}
