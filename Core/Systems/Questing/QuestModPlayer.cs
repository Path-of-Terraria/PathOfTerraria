using PathOfTerraria.Core.Systems.Questing.Quests.TestQuest;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Questing;
internal class QuestModPlayer : ModPlayer
{
	// need a list of what npcs start what quests
	public readonly List<Quest> EnabledQuests = [];

	public void RestartQuestTest()
	{
		EnabledQuests.Clear();
		Quest quest = new TestQuest();

		quest.StartQuest(Player);

		EnabledQuests.Add(quest);
	}

	public override void PostUpdate()
	{
		// _enabledQuests.ForEach(q => Console.WriteLine(q.CurrentQuestString()));
	}

	public override void SaveData(TagCompound tag)
	{
		List<TagCompound> questTags = [];
		foreach (Quest quest in EnabledQuests)
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

		questTags.ForEach(tag => { Quest q = Quest.LoadFrom(tag, Player); if (q is not null) { EnabledQuests.Add(q); } });
	}
}
