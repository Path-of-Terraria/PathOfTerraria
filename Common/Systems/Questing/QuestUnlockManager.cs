using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.Questing;

/// <summary>
/// Handles if quests are unlocked and what locations have a quest to accept.<br/>
/// Does not load on servers, as they do not have quests.
/// </summary>
[Autoload(Side = ModSide.Client)]
internal class QuestUnlockManager : ModSystem
{
	private readonly Dictionary<string, bool> isAvailable = [];
	private readonly Dictionary<string, bool> locationHasAvailable = [];

	public static bool CanStartQuest<T>() where T : Quest
	{
		return CanStartQuest(ModContent.GetInstance<T>().FullName);
	}

	public static bool CanStartQuest(string name)
	{
		return ModContent.GetInstance<QuestUnlockManager>().isAvailable[name];
	}

	public static bool LocationHasQuest(string location)
	{
		QuestUnlockManager qm = ModContent.GetInstance<QuestUnlockManager>();
		return qm.locationHasAvailable.ContainsKey(location) && qm.locationHasAvailable[location];
	}

	public override void PreUpdateTime()
	{
		IEnumerable<Quest> quests = ModContent.GetContent<Quest>();

		foreach (string key in locationHasAvailable.Keys)
		{
			locationHasAvailable[key] = false;
		}

		foreach (Quest quest in quests)
		{
			isAvailable[quest.FullName] = quest.CanBeStarted && quest.Available();

			if (isAvailable[quest.FullName])
			{
				locationHasAvailable[quest.MarkerLocation()] = true;
			}
		}
	}
}
