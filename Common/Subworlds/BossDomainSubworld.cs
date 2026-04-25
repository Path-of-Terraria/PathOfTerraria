using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// This is used to stop bosses from doing special death effects (like King Slime spawning the town slime, WoF spawning hardmode) automatically,<br/>
/// using a nicer loading screen dialogue, and setting <see cref="WorldGenerator.CurrentGenerationProgress"/> as it's not set by default.<br/>
/// The death effect system is in <see cref="BossTracker.CachedBossesDowned"/>.
/// </summary>
public abstract class BossDomainSubworld : MappingWorld
{
	private const string RavencrestStructureStateKey = "ravencrestStructureState";

	public override bool NoPlayerSaving => false;

	// We are going to first set the world to be completely flat so we can build on top of that
	public override List<GenPass> Tasks => [new FlatWorldPass()];

	public override void CopyMainWorldData()
	{
		base.CopyMainWorldData();
		CopyRavencrestStructureState();
	}

	public override void ReadCopiedMainWorldData()
	{
		base.ReadCopiedMainWorldData();
		ReadRavencrestStructureState();
	}

	public override void CopySubworldData()
	{
		base.CopySubworldData();
		CopyRavencrestStructureState();
	}

	public override void ReadCopiedSubworldData()
	{
		base.ReadCopiedSubworldData();
		ReadRavencrestStructureState();
	}

	private static void CopyRavencrestStructureState()
	{
		SubworldSystem.CopyWorldData(RavencrestStructureStateKey, RavencrestSystem.SaveStructureState());
	}

	private static void ReadRavencrestStructureState()
	{
		RavencrestSystem.LoadStructureState(SubworldSystem.ReadCopiedWorldData<TagCompound>(RavencrestStructureStateKey));
	}
}
