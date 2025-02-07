﻿using PathOfTerraria.Common.NPCs.QuestMarkers;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing;

public abstract class Quest : ModType, ILocalizedModType
{
	private static readonly Dictionary<string, Quest> QuestsByName = [];

	public abstract QuestTypes QuestType { get; }
	public abstract int NPCQuestGiver { get; }

	public LocalizedText DisplayName { get; private set; }
	public LocalizedText Description { get; private set; }

	public abstract List<QuestReward> QuestRewards { get; }
	public List<QuestStep> QuestSteps { get; protected set; } = null;

	/// <summary>
	/// Gets the quest marker type of this current quest. Assumes the quest is active;
	/// as such, <see cref="QuestMarkerType.HasQuest"/> can't be returned.
	/// </summary>
	public QuestMarkerType Marker => ActiveStep is not null && ActiveStep.CountsAsCompletedOnMarker ? QuestMarkerType.QuestComplete : QuestMarkerType.QuestPending;
	public bool CanBeStarted => !Completed && !Active;

	public string LocalizationCategory => $"Quests.Quest";

	public QuestStep ActiveStep => QuestSteps[CurrentStep];

	public int CurrentStep;
	public bool Completed;
	public bool Active = false;

	public sealed override void SetupContent()
	{
		SetStaticDefaults();
	}

	public override void SetStaticDefaults()
	{
		// Must be initialized here so that NPC types are populated properly.
		DisplayName = Language.GetOrRegister($"Mods.{PoTMod.ModName}.Quests.Quest.{GetType().Name}.Name", () => GetType().Name);
		Description = Language.GetOrRegister($"Mods.{PoTMod.ModName}.Quests.Quest.{GetType().Name}.Description", () => "");
		QuestSteps = SetSteps();
	}

	/// <summary>
	/// Defines the quest steps, in order, for a quest.<br/>
	/// See the <see cref="QuestStepTypes"/> folder/namespace for the steps to use.
	/// </summary>
	/// <returns>The ordered list of quest steps.</returns>
	public abstract List<QuestStep> SetSteps();

	/// <summary>
	/// The location associated with this quest. This is used to display the marker icon on the Arcane Obelisk UI.
	/// </summary>
	/// <returns></returns>
	public abstract string MarkerLocation();

	/// <summary>
	/// Determines if the quest is available or not. Runs only on clients.<br/>
	/// For example, returning true unconditionally will mean that this is available as soon as the player talks to the requisite NPC.<br/>
	/// By default: returns true.
	/// </summary>
	public virtual bool Available()
	{
		return true;
	}

	protected override void Register()
	{
		QuestsByName.Add(FullName, this);
		ModTypeLookup<Quest>.Register(this);
	}

	/// <summary>
	/// Gets the template singleton instance of the given quest. Do not modify this, as players will copy the singleton to use locally.
	/// </summary>
	/// <param name="name">Name of the quest.</param>
	/// <returns>The quest singleton.</returns>
	public static Quest GetSingleton(string name)
	{
		return QuestsByName[name];
	}

	/// <inheritdoc cref="GetSingleton(string)"/>
	/// <typeparam name="T">The type of the quest to get.</typeparam>
	public static Quest GetSingleton<T>() where T : Quest
	{
		return GetSingleton(ModContent.GetInstance<T>().FullName);
	}

	/// <summary>
	/// Gets the actual instance of the given quest on the local player.
	/// </summary>
	/// <param name="name"></param>
	/// <returns>The in-use quest instance for the local player.</returns>
	public static Quest GetLocalPlayerInstance(string name)
	{
		return Main.LocalPlayer.GetModPlayer<QuestModPlayer>().QuestsByName[name];
	}

	/// <inheritdoc cref="GetLocalPlayerInstance(string)"/>
	/// <typeparam name="T">The type of the quest to get.</typeparam>
	public static T GetLocalPlayerInstance<T>() where T : Quest
	{
		return Main.LocalPlayer.GetModPlayer<QuestModPlayer>().QuestsByName[ModContent.GetInstance<T>().FullName] as T;
	}

	public void StartQuest(Player player, int currentQuest = 0)
	{
		CurrentStep = currentQuest;

		if (CurrentStep >= QuestSteps.Count)
		{
			Completed = true;
			Active = false;
			QuestRewards.ForEach(qr => qr.GiveReward(player, player.Center));
			return;
		}

		Active = true;
	}

	public void Update(Player player)
	{
		if (ActiveStep.Track(player))
		{
			ActiveStep.OnComplete();
			StartQuest(player, CurrentStep + 1);
		}
	}

	public List<QuestStep> GetSteps()
	{
		return QuestSteps;
	}

	public string CurrentQuestString()
	{
		return ActiveStep.DisplayString();
	}

	public void Save(TagCompound tag)
	{
		tag.Add("completed", Completed);
		tag.Add("type", FullName); // This is not matched in Load, instead being checked in LoadFrom

		if (Completed)
		{
			return;
		}

		tag.Add("active", Active);

		if (!Active)
		{
			return;
		}

		tag.Add("currentQuest", CurrentStep);

		if (ActiveStep is null)
		{
			return;
		}

		var newTag = new TagCompound();
		ActiveStep.Save(newTag);
		tag.Add("currentQuestTag", newTag);
	}

	private void Load(TagCompound tag, Player player)
	{
		Reset();

		Completed = tag.GetBool("completed");

		if (Completed)
		{
			return;
		}

		Active = tag.GetBool("active");

		if (!Active)
		{
			return;
		}

		int step = tag.GetInt("currentQuest");
		StartQuest(player, step);

		for (int i = 0; i < step; ++i)
		{
			QuestSteps[i].IsDone = true;
		}

		ActiveStep.Load(tag.Get<TagCompound>("currentQuestTag"));
	}

	private void Reset()
	{
		Active = false;
		Completed = false;
		CurrentStep = 0;
		QuestSteps = SetSteps();
	}

	/// <summary>
	/// Loads a quest given the tag and player. This returns the quest, if found. If not found, returns the instance stored in <see cref="ModContent.GetInstance{T}"/>.
	/// </summary>
	/// <param name="tag">The tag data for the quest.</param>
	/// <param name="player">The player this is loading on.</param>
	/// <returns>If the quest was successfully loaded or not.</returns>
	public static bool LoadFrom(TagCompound tag, Player player, out Quest quest)
	{
		string name = tag.GetString("type");

		if (!ModContent.TryFind(name, out quest))
		{
			PoTMod.Instance.Logger.Error($"Could not load quest of {name}, was it removed?");
			return false;
		}

		quest = quest.Clone();
		quest.Load(tag, player);
		return true;
	}

	internal Quest Clone()
	{
		return (Quest)MemberwiseClone();
	}
}
