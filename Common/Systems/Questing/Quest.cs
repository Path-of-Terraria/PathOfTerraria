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

	public QuestStep ActiveStep = null;

	public int CurrentStep;
	public bool Completed;
	public bool Active = false;

	public sealed override void SetupContent()
	{
		SetStaticDefaults();
	}

	public override void SetStaticDefaults()
	{
		QuestSteps = SetSteps();

		DisplayName = Language.GetOrRegister($"Mods.{PoTMod.ModName}.Quests.Quest.{GetType().Name}.Name", () => GetType().Name);
		Description = Language.GetOrRegister($"Mods.{PoTMod.ModName}.Quests.Quest.{GetType().Name}.Description", () => "");
	}

	public abstract List<QuestStep> SetSteps();

	protected override void Register()
	{
		QuestsByName.Add(FullName, this);
		ModTypeLookup<Quest>.Register(this);
	}

	public override void SetupContent()
	{
		SetStaticDefaults();
	}

	public override void SetStaticDefaults()
	{
		// Must be initialized here so that NPC types are populated properly.
		QuestSteps = SetSteps();
	}

	public static Quest GetQuest(string name)
	{
		return QuestsByName[name];
	}

	public static LocalizedText QuestLocalization(string postfix)
	{
		return Language.GetText($"Mods.{PoTMod.ModName}.Quests." + postfix);
	}

	public static string QuestLocalizationValue(string postfix)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Quests." + postfix);
	}

	public void StartQuest(Player player, int currentQuest = 0)
	{
		CurrentStep = currentQuest;

		if (CurrentStep >= QuestSteps.Count)
		{
			Completed = true;
			QuestRewards.ForEach(qr => qr.GiveReward(player, player.Center));
			return;
		}

		ActiveStep = QuestSteps[CurrentStep];
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
		tag.Add("type", FullName);
		tag.Add("completed", Completed);
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
		if (tag.GetBool("completed"))
		{
			Completed = true;
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

	/// <summary>
	/// Loads a quest given the tag and player. This returns the singleton instance of the quest for convenience, if found.
	/// </summary>
	/// <param name="tag">The tag data for the quest.</param>
	/// <param name="player">The player this is loading on.</param>
	/// <returns>The quest singleton, if it was found.</returns>
	public static Quest LoadFrom(TagCompound tag, Player player)
	{
		string name = tag.GetString("type");

		if (!ModContent.TryFind(name, out Quest quest))
		{
			PoTMod.Instance.Logger.Error($"Could not load quest of {name}, was it removed?");
			return null;
		}

		quest.Load(tag, player);
		return quest;
	}
}
