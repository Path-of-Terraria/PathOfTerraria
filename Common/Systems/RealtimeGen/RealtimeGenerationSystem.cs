using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.RealtimeGen;

internal class RealtimeGenerationSystem : ModSystem
{
	public static RealtimeGenerationSystem Instance => ModContent.GetInstance<RealtimeGenerationSystem>();

	private readonly List<RealtimeGenerationAction> GenerationActions = [];

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
	}
}
