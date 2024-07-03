using NetEasy;
using PathOfTerraria.Core.Systems.Experience;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Networking.Modules;

/// <summary>
/// Spawns an orb on all clients but <paramref name="target"/>. 
/// Generally, this should only be called by <see cref="ExperienceTracker.SpawnExperience(int, Vector2, Vector2, int, bool)"/>.
/// </summary>
/// <param name="target">Target for the exp to aim for.</param>
/// <param name="xp">Amount of XP to spawn.</param>
/// <param name="spawn">Where it spawns.</param>
/// <param name="velocity">The base velocity of the spawned exp.</param>
[Serializable]
public class SpawnExpOrbModule(byte target, int xp, Vector2 spawn, Vector2 velocity) : Module
{
	protected readonly byte Target = target;
	protected readonly int Xp = xp;
	protected readonly Vector2 Spawn = spawn;
	protected readonly Vector2 Velocity = velocity;

	protected override void Receive()
	{
		if (Main.netMode == NetmodeID.Server)
		{
			Send(ignoreClient: Target, runLocally: false);
		}
		else
		{
			ExperienceTracker.SpawnExperience(Xp, Spawn, Velocity, Target, true);
		}
	}
}
