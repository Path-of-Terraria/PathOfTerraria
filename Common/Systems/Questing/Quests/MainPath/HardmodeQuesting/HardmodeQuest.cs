using PathOfTerraria.Common.Subworlds;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

internal abstract class HardmodeQuest(int tier) : Quest
{
	public readonly int QuestTier = tier;
	public override void OnCompleted()
	{
		ModContent.GetInstance<MappingDomainSystem>().Tracker.ClearHigherCompletions(QuestTier);
	}
}
