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
		_enabledQuests.Clear();
		Quest quest = new TestQuest();
		Quest quest2 = new TestQuestTwo();

		quest.StartQuest(Player);
		quest2.StartQuest(Player);

		_enabledQuests.Add(quest);
		_enabledQuests.Add(quest2);
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
	
	public List<Quest> GetAllQuests()
	{
		return _enabledQuests;
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
