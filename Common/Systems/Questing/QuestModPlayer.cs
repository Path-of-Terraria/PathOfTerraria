using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.UI.Quests;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing;

public class QuestModPlayer : ModPlayer
{
	// ReSharper disable once InconsistentNaming
	internal static ModKeybind ToggleQuestUIKey;

	public Dictionary<string, Quest> QuestsByName = [];
	
	internal bool FirstQuest = true;

	public void StartQuest(string name, int step = -1, bool fromLoad = false)
	{
		QuestsByName[name].StartQuest(Player, step == -1 ? 0 : step);

		if (Main.myPlayer == Player.whoAmI && !fromLoad)
		{
			UIQuestPopupState.NewQuest = new UIQuestPopupState.PopupText(QuestsByName[name].DisplayName, 300, 1f, 1.2f);
			SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/QuestStart") { Volume = 0.5f });

			if (FirstQuest) // Only display first quest popup on first quest (wow!)
			{
				UIQuestPopupState.FlashQuestButton = 600;

				FirstQuest = false;
			}
		}
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

		foreach (Quest quest in QuestsByName.Values)
		{
			var newTag = new TagCompound();
			quest.Save(newTag);
			questTags.Add(newTag);
		}

		tag.Add("questTags", questTags);

		if (FirstQuest)
		{
			tag.Add("firstQuest", false);
		}
	}
	
	public override void LoadData(TagCompound tag)
	{
		FirstQuest = tag.ContainsKey("firstQuest"); // If we have the tag, the first quest is true
		List<TagCompound> questTags = tag.Get<List<TagCompound>>("questTags");

		QuestsByName.Clear();
		
		foreach (TagCompound questTag in questTags)
		{
			if (Quest.LoadFrom(questTag, Player, out Quest quest))
			{
				QuestsByName.Add(quest.FullName, quest);
			}
		}

		// Catch all quests not already loaded and clone them so they exist in the dictionary
		foreach (Quest quest in ModContent.GetContent<Quest>())
		{
			if (!QuestsByName.ContainsKey(quest.FullName))
			{
				QuestsByName.Add(quest.FullName, quest.Clone());
			}
		}
	}

	public override void PostUpdateMiscEffects()
	{
		foreach (Quest quest in QuestsByName.Values)
		{
			if (!quest.Active)
			{
				continue;
			}

			quest.Update(Player); // Quests in enabledQuests are necessarily active

			if (quest.Completed)
			{
				quest.Active = false;
			}
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.life <= 0)
		{
			foreach (Quest quest in QuestsByName.Values)
			{
				quest.ActiveStep.OnKillNPC(Player, target, hit, damageDone); // Quests in enabledQuests are necessarily active
			}
		}
	}

	/// <summary>
	/// Get the number of active quests for the current player.
	/// </summary>
	/// <returns></returns>
	public int GetQuestCount()
	{
		return QuestsByName.Count(x => x.Value.Active);
	}
}
