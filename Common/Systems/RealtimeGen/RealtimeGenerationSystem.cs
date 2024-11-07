using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.RealtimeGen;

/// <summary>
/// Handles running and updating realtime generation.
/// </summary>
internal class RealtimeGenerationSystem : ModSystem
{
	public static RealtimeGenerationSystem Instance => ModContent.GetInstance<RealtimeGenerationSystem>();

	private readonly List<RealtimeGenerationAction> GenerationActions = [];

	/// <summary>
	/// Adds a <see cref="RealtimeGenerationAction"/> to the list of active generation actions.<br/>
	/// The action will be added and updated starting next frame.
	/// </summary>
	/// <param name="action">The action to add.</param>
	public static void AddAction(RealtimeGenerationAction action)
	{
		Instance.GenerationActions.Add(action);
	}

	public override void PreUpdateTime()
	{
		foreach (RealtimeGenerationAction action in GenerationActions)
		{
			action.Update();
		}

		GenerationActions.RemoveAll(x => x.Done);
	}
}
