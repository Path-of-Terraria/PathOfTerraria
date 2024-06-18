using PathOfTerraria.Content.Projectiles.Whip;
using PathOfTerraria.Helpers;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Whip;

internal class AltWhipHook : ILoadable
{
	public void Load(Mod mod)
	{
		On_Projectile.FillWhipControlPoints += HijackAltWhipPoints;
	}

	private void HijackAltWhipPoints(On_Projectile.orig_FillWhipControlPoints orig, Projectile proj, List<Vector2> controlPoints)
	{
		if (proj.ModProjectile is WhipProjectile projectile && projectile.AltUse)
		{
			Projectile.GetWhipSettings(proj, out float timeToFlyOut, out int segments, out float rangeMult);

			float baseFactor = projectile.Timer / timeToFlyOut;
			float factor = Easings.EaseCubicInOut.Ease(baseFactor);
			factor = Math.Abs(factor * 2f);
			float rotationFactor = (factor - 0.5f) * MathHelper.TwoPi;
			bool flip = false;

			if (factor > 1)
			{
				factor = 2 - factor;
				flip = true;
			}

			Vector2 origin = CalculateHandPosition(proj);
			Vector2 extendedPosition = GetExtendedPosition(proj, rangeMult, segments - 1, flip) * (1f - Math.Abs(factor - 0.5f) * 2);
			Vector2 midPoint = origin + extendedPosition.RotatedBy(rotationFactor * 0.25f) / 2;
			Vector2 target = origin + extendedPosition.RotatedBy(rotationFactor);

			for (int i = 0; i < segments; ++i)
			{
				float segmentFactor = i / (float)segments;
				controlPoints.Add(Vector2.Lerp(Vector2.Lerp(origin, midPoint, segmentFactor), Vector2.Lerp(midPoint, target, segmentFactor), segmentFactor));
			}
		}
		else
		{
			orig(proj, controlPoints);
		}
	}

	private Vector2 CalculateHandPosition(Projectile proj)
	{
		float rotationOffset = proj.frameCounter switch
		{
			>= 50 => MathHelper.Pi,
			>= 40 => 3 * MathHelper.PiOver4,
			>= 30 => MathHelper.PiOver2,
			_ => -MathHelper.PiOver2
		};
		
		Player owner = Main.player[proj.owner];
		// Calculate the arm's position and direction
		Vector2 armPosition = owner.Center;
		const float armLength = 10f;

		// Calculate the hand's position based on the arm's rotation
		Vector2 handOffset = new Vector2((float)Math.Cos(rotationOffset), (float)Math.Sin(rotationOffset)) * armLength;
		return armPosition + new Vector2(handOffset.X + 20, handOffset.Y + 28);
	}

	private static Vector2 GetExtendedPosition(Projectile proj, float rangeMult, int i, bool flip)
	{
		Vector2 vel = proj.velocity;

		if (flip)
		{
			vel *= -1;
		}

		return vel * i * 3.5f * rangeMult;
	}

	public void Unload()
	{
	}
}
