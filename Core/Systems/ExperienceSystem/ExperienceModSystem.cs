using PathOfTerraria.Core.Systems.Networking.Handlers;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Exp;

// ReSharper disable once ClassNeverInstantiated.Global
public class ExperienceModSystem : ModSystem
{
	private static List<Experience> _trackedExp;

	public override void OnWorldLoad()
	{
		_trackedExp = new(1000);
	}

	public override void PostUpdateNPCs()
	{
		foreach (Experience t in _trackedExp)
		{
			t?.Update();
		}
	}

	public override void PostDrawTiles()
	{
		_trackedExp.RemoveAll(x => x is null || !x.Active || x.Collected);

		SpriteBatch batch = Main.spriteBatch;
		batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		//Draw the orbs
		Texture2D texture = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Experience").Value;
		foreach (Experience xp in _trackedExp)
		{
			Vector2 size = xp.GetSize();
			Rectangle source = xp.GetSourceRectangle();

			batch.Draw(texture, xp.Center - Main.screenPosition, source, Color.White, xp.Rotation, size / 2f, 1f, SpriteEffects.None, 0);
		}

		batch.End();

		//Draw the trails - TBD
		//foreach (Experience t in _trackedExp)
		//{
		//	t?.DrawTrail();
		//}
	}

	/// <summary>
	/// Spawns experience for the local player and sends a <see cref="SpawnExpOrbModule"/> to sync it to other clients.
	/// </summary>
	/// <param name="xp">The amount of (in value) to spawn. This will automatically be split into many smaller orbs as necessary.</param>
	/// <param name="location">Location of the orb(s).</param>
	/// <param name="baseVelocity">The base velocity of the exp. Rotated deterministically per orb for variety.</param>
	/// <param name="targetPlayer">The player they're aiming for.</param>
	/// <param name="fromNet">Whether the orb was spawned from net, such to skip another <see cref="ExperienceHandler.SendExperience(byte, int, Vector2, Vector2, bool)"/>.</param>
	/// <returns></returns>
	public static int[] SpawnExperience(int xp, Vector2 location, Vector2 baseVelocity, int targetPlayer, bool fromNet = false)
	{
		if (xp <= 0)
		{
			return [];
		}

		List<Experience> spawned = [];
		int totalLeft = xp;

		while (totalLeft > 0)
		{
			int toSpawn;

			switch (totalLeft)
			{
				case >= Experience.Sizes.OrbLargeBlue:
					toSpawn = Experience.Sizes.OrbLargeBlue;
					totalLeft -= Experience.Sizes.OrbLargeBlue;
					break;
				case >= Experience.Sizes.OrbLargeGreen:
					toSpawn = Experience.Sizes.OrbLargeGreen;
					totalLeft -= Experience.Sizes.OrbLargeGreen;
					break;
				case >= Experience.Sizes.OrbLargeYellow:
					toSpawn = Experience.Sizes.OrbLargeYellow;
					totalLeft -= Experience.Sizes.OrbLargeYellow;
					break;
				case >= Experience.Sizes.OrbMediumBlue:
					toSpawn = Experience.Sizes.OrbMediumBlue;
					totalLeft -= Experience.Sizes.OrbMediumBlue;
					break;
				case >= Experience.Sizes.OrbMediumGreen:
					toSpawn = Experience.Sizes.OrbMediumGreen;
					totalLeft -= Experience.Sizes.OrbMediumGreen;
					break;
				case >= Experience.Sizes.OrbMediumYellow:
					toSpawn = Experience.Sizes.OrbMediumYellow;
					totalLeft -= Experience.Sizes.OrbMediumYellow;
					break;
				case >= Experience.Sizes.OrbSmallBlue:
					toSpawn = Experience.Sizes.OrbSmallBlue;
					totalLeft -= Experience.Sizes.OrbSmallBlue;
					break;
				case >= Experience.Sizes.OrbSmallGreen:
					toSpawn = Experience.Sizes.OrbSmallGreen;
					totalLeft -= Experience.Sizes.OrbSmallGreen;
					break;
				default:
					toSpawn = Experience.Sizes.OrbSmallYellow;
					totalLeft--;
					break;
			}

			var thing = new Experience(toSpawn, location, baseVelocity.RotatedBy(totalLeft * MathHelper.PiOver2 * 1.22f), targetPlayer);

			// Only add the exp if the size is valid.
			// At the moment there's no way for this to happen, so this is just futureproofing.
			if (thing.GetSize() != Vector2.Zero)
			{
				spawned.Add(thing);
			}
		}

		int[] indices = new int[spawned.Count];
		for (int i = 0; i < indices.Length; i++)
		{
			indices[i] = InsertExperience(spawned[i]);
		}

		if (Main.netMode != NetmodeID.SinglePlayer && !fromNet) // Syncs the spawn of all orbs - only does so if not from the server
		{
			ExperienceHandler.SendExperience((byte)targetPlayer, xp, location, baseVelocity, false);
		}

		return indices;
	}

	private static int InsertExperience(Experience expNew)
	{
		_trackedExp.Add(expNew);
		return _trackedExp.Count - 1;
	}
}