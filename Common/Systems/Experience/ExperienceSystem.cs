using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Experience;

// ReSharper disable once ClassNeverInstantiated.Global
public class ExperienceTracker : ModSystem
{
	private static Mechanics.Experience[] _trackedExp;

	public override void OnWorldLoad()
	{
		_trackedExp = new Mechanics.Experience[1000];
	}

	public override void PostUpdateNPCs()
	{
		foreach (Mechanics.Experience t in _trackedExp)
		{
			t?.Update();
		}
	}

	public override void PostDrawTiles()
	{
		SpriteBatch batch = Main.spriteBatch;
		batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		//Draw the orbs
		Texture2D texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Experience").Value;
		foreach (Mechanics.Experience xp in _trackedExp)
		{
			if (xp is null || !xp.Active)
			{
				continue;
			}

			Vector2 size = xp.GetSize();

			if (size == Vector2.Zero || xp.Collected)
			{
				continue;
			}

			Rectangle source = xp.GetSourceRectangle();

			batch.Draw(texture, xp.Center - Main.screenPosition, source, Color.White, xp.Rotation, size / 2f, 1f, SpriteEffects.None, 0);
		}

		batch.End();

		//Draw the trails
		foreach (Mechanics.Experience t in _trackedExp)
		{
			t?.DrawTrail();
		}
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

		if (Main.netMode == NetmodeID.Server && !fromNet) // Syncs the spawn of all orbs - only does so if not from the server
		{
			//ExperienceHandler.SendExperience(targetPlayer, xp, location, baseVelocity, true);
			//return [];
		}

		List<Mechanics.Experience> spawned = [];
		int totalLeft = xp;

		while (totalLeft > 0)
		{
			int toSpawn;

			switch (totalLeft)
			{
				case >= Mechanics.Experience.Sizes.OrbLargeBlue:
					toSpawn = Mechanics.Experience.Sizes.OrbLargeBlue;
					totalLeft -= Mechanics.Experience.Sizes.OrbLargeBlue;
					break;
				case >= Mechanics.Experience.Sizes.OrbLargeGreen:
					toSpawn = Mechanics.Experience.Sizes.OrbLargeGreen;
					totalLeft -= Mechanics.Experience.Sizes.OrbLargeGreen;
					break;
				case >= Mechanics.Experience.Sizes.OrbLargeYellow:
					toSpawn = Mechanics.Experience.Sizes.OrbLargeYellow;
					totalLeft -= Mechanics.Experience.Sizes.OrbLargeYellow;
					break;
				case >= Mechanics.Experience.Sizes.OrbMediumBlue:
					toSpawn = Mechanics.Experience.Sizes.OrbMediumBlue;
					totalLeft -= Mechanics.Experience.Sizes.OrbMediumBlue;
					break;
				case >= Mechanics.Experience.Sizes.OrbMediumGreen:
					toSpawn = Mechanics.Experience.Sizes.OrbMediumGreen;
					totalLeft -= Mechanics.Experience.Sizes.OrbMediumGreen;
					break;
				case >= Mechanics.Experience.Sizes.OrbMediumYellow:
					toSpawn = Mechanics.Experience.Sizes.OrbMediumYellow;
					totalLeft -= Mechanics.Experience.Sizes.OrbMediumYellow;
					break;
				case >= Mechanics.Experience.Sizes.OrbSmallBlue:
					toSpawn = Mechanics.Experience.Sizes.OrbSmallBlue;
					totalLeft -= Mechanics.Experience.Sizes.OrbSmallBlue;
					break;
				case >= Mechanics.Experience.Sizes.OrbSmallGreen:
					toSpawn = Mechanics.Experience.Sizes.OrbSmallGreen;
					totalLeft -= Mechanics.Experience.Sizes.OrbSmallGreen;
					break;
				default:
					toSpawn = Mechanics.Experience.Sizes.OrbSmallYellow;
					totalLeft--;
					break;
			}

			var thing = new Mechanics.Experience(toSpawn, location, baseVelocity.RotatedBy(totalLeft * MathHelper.PiOver2 * 1.22f), targetPlayer);
			spawned.Add(thing);
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

	private static int InsertExperience(Mechanics.Experience expNew)
	{
		for (int i = 0; i < _trackedExp.Length; i++)
		{
			Mechanics.Experience exp = _trackedExp[i];

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