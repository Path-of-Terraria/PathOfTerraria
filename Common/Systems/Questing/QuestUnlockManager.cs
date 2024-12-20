using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.Questing;

[Autoload(Side = ModSide.Client)]
internal class QuestUnlockManager : ModSystem
{
	private readonly Dictionary<string, bool> IsAvailable = [];

	public static bool CanStartQuest(string name)
	{
		return ModContent.GetInstance<QuestUnlockManager>().IsAvailable[name];
	}

	public override void PreUpdateTime()
	{
		IEnumerable<Quest> quests = ModContent.GetContent<Quest>();

		foreach (Quest quest in quests)
		{
			IsAvailable[quest.FullName] = quest.CanBeStarted && quest.Available();
		}
	}
}
