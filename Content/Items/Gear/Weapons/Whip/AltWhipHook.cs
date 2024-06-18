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

			Player plr = Main.player[proj.owner];

			Vector2 origin = plr.Center - new Vector2(0, 8);
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
