using System.Collections.Generic;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ExperienceSystem;

// ReSharper disable once ClassNeverInstantiated.Global
public class ExperienceTracker : ModSystem
{
	private static Experience[] _trackedExp;

	public override void OnWorldLoad()
	{
		_trackedExp = new Experience[1000];
	}

	public override void PostUpdateNPCs()
	{
		for (int i = 0; i < _trackedExp.Length; i++)
		{
			Experience t = _trackedExp[i];
			t?.Update(i, _trackedExp);
		}
	}

	public override void PostDrawTiles()
	{
		SpriteBatch batch = Main.spriteBatch;
		batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		//Draw the orbs
		Texture2D texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Experience").Value;

		foreach (Experience xp in _trackedExp)
		{
			xp?.Draw(batch, texture);
		}

		batch.End();
	}

	/// <summary>
	/// Spawns experience for the local player and sends a <see cref="SpawnExpOrbModule"/> to sync it to other clients.
	/// </summary>
	/// <param name="xp">The amount of (in value) to spawn. This will automatically be split into many smaller orbs as necessary.</param>
	/// <param name="location">Location of the orb(s).</param>
	/// <param name="baseVelocity">The base velocity of the exp. Rotated deterministically per orb for variety.</param>
	/// <param name="targetPlayer">The player they're aiming for.</param>
	/// <param name="fromNet">Whether the orb was spawned from net, such to skip sending another <see cref="SpawnExpOrbModule"/>.</param>
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
			spawned.Add(thing);
		}

		int[] indices = new int[spawned.Count];
		for (int i = 0; i < indices.Length; i++)
		{
			indices[i] = InsertExperience(spawned[i]);
		}

		if (Main.netMode != NetmodeID.SinglePlayer && !fromNet) // Syncs the spawn of all orbs - only does so if not from the server
		{
			ModContent.GetInstance<ExperienceHandler>().Send((byte)targetPlayer, xp, location, baseVelocity, false);
		}

		return indices;
	}

	private static int InsertExperience(Experience expNew)
	{
		for (int i = 0; i < _trackedExp.Length; i++)
		{
			Experience exp = _trackedExp[i];

			if (exp is not null && exp.Active)
			{
				continue;
			}

			_trackedExp[i] = expNew;
			return i;
		}

		int index = _trackedExp.Length;
		Array.Resize(ref _trackedExp, _trackedExp.Length * 2);

		_trackedExp[index] = expNew;
		return index;
	}
}