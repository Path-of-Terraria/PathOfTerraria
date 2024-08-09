using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.Systems.Questing.Quests.TestQuest;
using PathOfTerraria.Common.UI.Quests;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.GameInput;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing;

internal class QuestModPlayer : ModPlayer
{
	// ReSharper disable once InconsistentNaming
	private static ModKeybind ToggleQuestUIKey;
	
	// need a list of what npcs start what quests
	private readonly HashSet<string> _enabledQuests = [];

	public void StartQuest(string name)
	{
		_enabledQuests.Clear();

		var quest = Quest.GetQuest(name);
		quest.StartQuest(Player);
		_enabledQuests.Add(quest.Name);
	}

	public void RestartQuestTest()
	{
		_enabledQuests.Clear();

	}
	
	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		ToggleQuestUIKey = KeybindLoader.RegisterKeybind(Mod, "QuestUIKey", Keys.L);
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (ToggleQuestUIKey.JustPressed)
		{
			SmartUiLoader.GetUiState<QuestsUIState>().Toggle();
		}
	}
	
	///// <summary>
	///// Returns the quest string with return spacing for each quest step completed
	///// </summary>
	///// <param name="name"></param>
	///// <returns></returns>
	//public string GetQuestSteps(string name)
	//{
	//	return _enabledQuests[name].AllQuestStrings();
	//}

	//public string GetQuestStep(string name)
	//{
	//	return _enabledQuests[name].CurrentQuestString();
	//}

	public override void SaveData(TagCompound tag)
	{
		List<TagCompound> questTags = [];

		foreach (string quest in _enabledQuests)
		{
			var newTag = new TagCompound();
			Quest.GetQuest(quest).Save(newTag);
			questTags.Add(newTag);
		}

		tag.Add("questTags", questTags);
	}
	
	public override void LoadData(TagCompound tag)
	{
		List<TagCompound> questTags = tag.Get<List<TagCompound>>("questTags");

		questTags.ForEach(tag => 
		{ 
			var q = Quest.LoadFrom(tag, Player); 
			
			if (q is not null) 
			{ 
				_enabledQuests.Add(q.Name); 
			} 
		});
	}

	public override void PostUpdateMiscEffects()
	{
		foreach (string quest in _enabledQuests)
		{
			Quest.GetQuest(quest).Update(Player);
		}
	}

	public List<string> GetQuests()
	{
		return [.. _enabledQuests];
	}

	/// <summary>
	/// Get the number of quests for the player
	/// </summary>
	/// <returns></returns>
	public int GetQuestCount()
	{
		return _enabledQuests.Count;
	}

	/// <summary>
	/// Gets all quests the player for the player
	/// </summary>
	/// <returns></returns>
	public HashSet<string> GetAllQuests()
	{
		return _enabledQuests;
	}
	
	///// <summary>
	///// Gets all completed quests for the player
	///// </summary>
	///// <returns></returns>
	//public List<Quest> GetCompletedQuests()
	//{
	//	return _enabledQuests.Values.ToList().FindAll(q => q.Completed);
	//}
	
	///// <summary>
	///// Gets all incomplete quests for the player
	///// </summary>
	///// <returns></returns>
	//public List<Quest> GetIncompleteQuests()
	//{
	//	return _enabledQuests.Values.ToList().FindAll(q => !q.Completed);
	//}
	
	///// <summary>
	///// Get a quest by name
	///// </summary>
	///// <param name="name"></param>
	///// <returns></returns>
	//public Quest GetQuest(string name)
	//{
	//	return _enabledQuests[name];
	//}
}
