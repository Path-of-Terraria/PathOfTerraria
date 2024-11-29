using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.UI.Quests;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing;

internal class QuestModPlayer : ModPlayer
{
	private readonly record struct CachedQuest(TagCompound tag);

	// ReSharper disable once InconsistentNaming
	public static ModKeybind ToggleQuestUIKey;
	
	// need a list of what npcs start what quests
	private readonly HashSet<string> _enabledQuests = [];
	private readonly List<CachedQuest> _questsToEnable = [];

	private bool _firstQuest = true;

	public void StartQuest(string name, int step = -1, bool fromLoad = false)
	{
		var quest = Quest.GetQuest(name);
		quest.StartQuest(Player, step == -1 ? 0 : step);
		_enabledQuests.Add(quest.FullName);

		if (Main.myPlayer == Player.whoAmI && !fromLoad)
		{
			UIQuestPopupState.NewQuest = new UIQuestPopupState.PopupText(Quest.GetQuest(name).DisplayName, 300, 1f, 1.2f);

			SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/QuestStart") { Volume = 0.5f });

			if (_firstQuest) // Only display first quest popup on first quest (wow!)
			{
				UIQuestPopupState.FlashQuestButton = 600;

				_firstQuest = false;
			}
		}
	}

	public void RestartQuestTest()
	{
		_enabledQuests.Clear();

		StartQuest("PathOfTerraria/TestQuest");
		StartQuest("PathOfTerraria/TestQuestTwo");
	}

	public override void OnEnterWorld()
	{
		foreach (CachedQuest cachedQuest in _questsToEnable)
		{
			var quest = Quest.LoadFrom(cachedQuest.tag, Player);

			if (quest is not null)
			{
				StartQuest(quest.FullName, quest.CurrentStep, true);
			}
		}

		_questsToEnable.Clear();
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
		IEnumerable<Quest> quests = ModContent.GetContent<Quest>();

		foreach (Quest quest in quests)
		{
			if (quest.Active)
			{
				var newTag = new TagCompound();
				quest.Save(newTag);
				questTags.Add(newTag);
			}
		}

		tag.Add("questTags", questTags);

		if (!_firstQuest)
		{
			tag.Add("firstQuest", false);
		}
	}
	
	public override void LoadData(TagCompound tag)
	{
		_firstQuest = !tag.ContainsKey("firstQuest"); // If we have the tag, the first quest is false
		List<TagCompound> questTags = tag.Get<List<TagCompound>>("questTags");

		// We can't enable quests here; that'd set it to Active and mess up all save data for every other player.
		// Instead, we cache the quests that need to be loaded later,
		// as the only time the player can save is in-world.
		questTags.ForEach(tag => _questsToEnable.Add(new CachedQuest(tag)));
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
