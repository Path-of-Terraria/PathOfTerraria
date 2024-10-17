using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
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
		var quest = Quest.GetQuest(name);
		quest.StartQuest(Player);
		_enabledQuests.Add(quest.FullName);
	}

	public void RestartQuestTest()
	{
		_enabledQuests.Clear();

		StartQuest("PathOfTerraria/TestQuest");
		StartQuest("PathOfTerraria/TestQuestTwo");
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
			string quest = Quest.LoadFrom(tag, Player); 
			
			if (quest is not null) 
			{ 
				_enabledQuests.Add(quest); 
			} 
		});
	}

	public override void PostUpdateMiscEffects()
	{
		HashSet<string> removals = [];

		foreach (string q in _enabledQuests)
		{
			var quest = Quest.GetQuest(q);
			quest.Update(Player); // Quests in enabledQuests are necessarily active

			if (quest.Completed)
			{
				removals.Add(q);
			}
		}

		foreach (string quest in removals)
		{
			_enabledQuests.Remove(quest);
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.life <= 0)
		{
			foreach (string q in _enabledQuests)
			{
				var quest = Quest.GetQuest(q);
				quest.ActiveStep.OnKillNPC(Player, target, hit, damageDone); // Quests in enabledQuests are necessarily active
			}
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
}
