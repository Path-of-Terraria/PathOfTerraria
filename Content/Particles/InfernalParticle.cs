using Daybreak.Common.Features.Hooks;
using PathOfTerraria.Core.Graphics.Particles;
using PathOfTerraria.Core.Tween;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfTerraria.Content.Particles;
public class InfernalParticle(Vector2 position, Vector2 velocity = default, int duration = 15, Color? color = null, Vector2? moveToTarget = null, float? startingScale = null, float? velocityStectch = null) : MagicParticleGlows(position, velocity, duration, color, moveToTarget,startingScale, velocityStectch)
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Particles/InfernalParticle";
	public override bool IsBlendStateAddtive => true;
	public override void Update()
	{
		Rotation += 0.25f;
		base.Update();
	}
}
