using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.NPCs.QuestMarkers;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.UI.Quests;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing;

public class QuestModPlayer : ModPlayer
{
	// ReSharper disable once InconsistentNaming
	internal static ModKeybind ToggleQuestUIKey;

	/// <summary>
	/// A non-synced list of every quest this player can have.
	/// </summary>
	public Dictionary<string, Quest> QuestsByName = [];

	public Dictionary<string, QuestMarkerType> MarkerTypeByLocation = [];

	/// <summary>
	/// A fully synced list of the quests this player currently has active.
	/// </summary>
	public readonly HashSet<string> EnabledQuestsByName = [];

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
		QuestsByName[name].Start(Player, step == -1 ? 0 : step);
		EnabledQuestsByName.Add(name);

		if (Main.myPlayer == Player.whoAmI && !fromLoad)
		{
			UIQuestPopupState.NewQuest = new UIQuestPopupState.PopupText(QuestsByName[name].DisplayName, 300, 1f, 1.2f);
			SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/QuestStart") { Volume = 0.5f });

			if (FirstQuest) // Only display first quest popup on first quest (wow!)
			{
				UIQuestPopupState.FlashQuestButton = 600;

				FirstQuest = false;
			}

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				SyncPlayerQuestActive.Send(name, true);
			}
		}
	}

	public override void OnEnterWorld()
	{
		foreach (Quest quest in QuestsByName.Values)
		{
			if (quest.Active)
			{
				EnabledQuestsByName.Add(quest.FullName);

				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					SyncPlayerQuestActive.Send(quest.FullName, true);
				}
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

			if (!quest.Completed)
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
	}

	internal void OnKillNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		foreach (Quest quest in QuestsByName.Values)
		{
			if (quest.Active)
			{
				quest.ActiveStep.OnKillNPC(Player, target, hit, damageDone);
			}
		}
	}

	internal void OnKillNPC(int targetType, int targetNetId, NPC.HitInfo hit, int damageDone)
	{
		NPC target = new()
		{
			type = targetType,
			netID = targetNetId
		};

		OnKillNPC(target, hit, damageDone);
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
