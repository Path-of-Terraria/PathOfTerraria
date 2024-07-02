using PathOfTerraria.Core.Systems.Questing.Quests.TestQuest;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Questing;
internal class QuestModPlayer : ModPlayer
{
	// need a list of what npcs start what quests
	private readonly List<Quest> _enabledQuests = [];

	public void RestartQuestTest()
	{
		StartQuest<TestQuest>(true);
	}

	public void StartQuest(Quest quest, bool clearQuests = false)
	{
		if (clearQuests)
		{
			_enabledQuests.Clear();
		}

		quest.StartQuest(Player);
		_enabledQuests.Add(quest);
	}

	public void StartQuest<T>(bool clearQuests = false) where T : Quest
	{
		StartQuest(Activator.CreateInstance(typeof(T)) as Quest, clearQuests);
	}

	public override void PostUpdate()
	{
		// _enabledQuests.ForEach(q => Console.WriteLine(q.CurrentQuestString()));
	}

	public override void SaveData(TagCompound tag)
	{
		List<TagCompound> questTags = [];
		foreach (Quest quest in _enabledQuests)
		{
			var newTag = new TagCompound();
			quest.Save(newTag);
			questTags.Add(newTag);
		}

		tag.Add("questTags", questTags);
	}
	
	public override void LoadData(TagCompound tag)
	{
		List<TagCompound> questTags = tag.Get<List<TagCompound>>("questTags");

		questTags.ForEach(tag => { Quest q = Quest.LoadFrom(tag, Player); if (q is not null) { _enabledQuests.Add(q); } });
	}
	
	public List<Quest> GetCompletedQuests()
	{
		return _enabledQuests.FindAll(q => q.Completed);
	}
	
	public List<Quest> GetIncompleteQuests()
	{
		return _enabledQuests.FindAll(q => !q.Completed);
	}
}
