using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.Questing;

/// <summary>
/// Handles if quests are unlocked and what locations have a quest to accept.<br/>
/// Does not load on servers, as they do not have quests.
/// </summary>
[Autoload(Side = ModSide.Client)]
internal class QuestUnlockManager : ModSystem
{
	private readonly Dictionary<string, bool> IsAvailable = [];
	private readonly Dictionary<string, bool> LocationHasAvailable = [];

	public static bool CanStartQuest<T>() where T : Quest
	{
		return CanStartQuest(ModContent.GetInstance<T>().FullName);
	}

	public static bool CanStartQuest(string name)
	{
		return ModContent.GetInstance<QuestUnlockManager>().IsAvailable[name];
	}

	public static bool LoationHasQuest(string location)
	{
		return ModContent.GetInstance<QuestUnlockManager>().LocationHasAvailable[location];
	}

	public override void PreUpdateTime()
	{
		IEnumerable<Quest> quests = ModContent.GetContent<Quest>();

		foreach (string key in LocationHasAvailable.Keys)
		{
			LocationHasAvailable[key] = false;
		}

		foreach (Quest quest in quests)
		{
			IsAvailable[quest.FullName] = quest.CanBeStarted && quest.Available();

			if (IsAvailable[quest.FullName])
			{
				LocationHasAvailable[quest.MarkerLocation()] = true;
			}
		}
	}
}
