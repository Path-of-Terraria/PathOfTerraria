using PathOfTerraria.Common.Systems.ModPlayers;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Projectiles;

/// <summary>
/// Allows a projectile to act as a "solid top" entity, similar to tiles with "solid tops". This means players and entities can walk on and fall through these projectiles.<br/>
/// Code adapted from a similar implementation in SpiritReforged: https://github.com/GabeHasWon/SpiritReforged/blob/master/Content/Savanna/Tiles/AcaciaTree/TreetopPlatform.cs
/// </summary>
internal interface ISolidTopProjectile
{
	internal class SolidTopProjectileHooks : ModPlayer
	{
		/// <summary>
		/// Defines the offset from the top of the projectile that the player stands at. Defaults to 2. The value should be between 1 and the entity's height.
		/// </summary>
		public readonly static Dictionary<int, float> SolidTopOffsets = [];

		public override void Load()
		{
			On_NPC.UpdateCollision += CheckNPCCollision;
		}

		private static void CheckNPCCollision(On_NPC.orig_UpdateCollision orig, NPC self)
		{
			if (!self.noGravity)
			{
				foreach (Projectile p in Main.ActiveProjectiles)
				{
					if (p.ModProjectile is ISolidTopProjectile top && self.getRect().Intersects(p.Hitbox) && top.CanStandOn(self, p) && NPCLoader.CanFallThroughPlatforms(self) != false)
					{
						top.UpdateSolidTop(self, p);
						break;
					}
				}
			}

			orig(self);
		}

		public override void PreUpdateMovement()
		{
			Rectangle lowRect = Player.getRect() with { Height = Player.height / 2, Y = (int)Player.position.Y + Player.height / 2 };

			foreach (Projectile p in Main.ActiveProjectiles)
			{
				if (p.ModProjectile is ISolidTopProjectile top && lowRect.Intersects(p.Hitbox) && top.CanStandOn(Player, p) && !Player.GetModPlayer<FallThroughPlayer>().FallThrough())
				{
					top.UpdateSolidTop(Player, p);

					if (Player.controlDown)
					{
						Player.GetModPlayer<FallThroughPlayer>().FallThroughPlatform = true;
					}

					break; // It would be redundant to check for other platforms when the player is already on one
				}
			}
		}
	}

	public void UpdateSolidTop(Entity entity, Projectile projectile)
	{
		Vector2 pos = projectile.position;
		entity.velocity.Y = 0;
		float offset = SolidTopProjectileHooks.SolidTopOffsets.TryGetValue(projectile.type, out float value) ? value : 2;
		var newPos = new Vector2(entity.position.X, projectile.Hitbox.Top + offset - entity.height); // Top needs to be adjusted by at least one pixel down to account for hitbox intersection

		if (!Collision.SolidCollision(newPos, entity.width, entity.height))
		{
			entity.position = newPos;
		}
	}

	public bool CanStandOn(Entity entity, Projectile projectile)
	{
		float offset = SolidTopProjectileHooks.SolidTopOffsets.TryGetValue(projectile.type, out float value) ? value : 2;
		return entity.velocity.Y >= 0 && entity.Bottom.Y < projectile.Hitbox.Y + 10 + offset;
	}
}
