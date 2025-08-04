using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.NPCs.QuestMarkers;
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

	public Dictionary<string, QuestMarkerType> MarkerTypeByLocation = [];
	
	internal bool FirstQuest = true;
	/// <summary> The full name of this player's pinned quest. </summary>
	public string PinnedQuest;

	/// <inheritdoc cref="StartQuest(string, int, bool)"/>
	/// <typeparam name="T">Quest to start.</typeparam>
	public void StartQuest<T>() where T : Quest
	{
		StartQuest(ModContent.GetInstance<T>().FullName);
	}

	/// <summary>
	/// Starts the given quest.
	/// </summary>
	/// <param name="name">Full name of the quest (i.e. <see cref="Quest"/>.FullName).</param>
	/// <param name="step">The step to skip to. Defaults to -1, which (re)starts the quest.</param>
	/// <param name="fromLoad">Skips the quest popups &amp; sound effects if true.</param>
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
			Main.playerInventory = true;
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
		MarkerTypeByLocation.Clear();

		foreach (Quest quest in QuestsByName.Values)
		{
			if (!quest.Active)
			{
				continue;
			}

			quest.Update(Player);

			if (quest.Completed)
			{
				quest.Active = false;
			}
			else
			{
				// Update markers per area.
				// This uses all quests, which define their location and complete-ness already,
				// so it's pretty nicely automatic.

				QuestMarkerType marker = quest.Marker;

				if (!MarkerTypeByLocation.TryGetValue(quest.MarkerLocation(), out QuestMarkerType value))
				{
					MarkerTypeByLocation.Add(quest.MarkerLocation(), marker);
				}
				else if (marker == QuestMarkerType.QuestComplete)
				{
					MarkerTypeByLocation[quest.MarkerLocation()] = marker;
				}
			}
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.life <= 0)
		{
			foreach (Quest quest in QuestsByName.Values)
			{
				if (quest.Active)
				{
					quest.ActiveStep.OnKillNPC(Player, target, hit, damageDone); // Quests in enabledQuests are necessarily active
				}
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
