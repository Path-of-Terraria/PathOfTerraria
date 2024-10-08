using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing;

public abstract class Quest : ModType
{
	private static readonly Dictionary<string, Quest> QuestsByName = [];

	public abstract QuestTypes QuestType { get; }
	public abstract int NPCQuestGiver { get; }

	public LocalizedText DisplayName { get; private set; } 
	public LocalizedText Description { get; private set; } 

	public abstract List<QuestReward> QuestRewards { get; }
	public List<QuestStep> QuestSteps { get; } = null;

	public QuestStep ActiveStep = null;

	public int CurrentStep;
	public bool Completed;
	public bool Active = false;

	public Quest()
	{
		QuestSteps = SetSteps();

		DisplayName = Language.GetOrRegister($"Mods.{PoTMod.ModName}.Quests.Quest.{GetType().Name}.Name", () => GetType().Name);
		Description = Language.GetOrRegister($"Mods.{PoTMod.ModName}.Quests.Quest.{GetType().Name}.Description", () => "");
	}

	public abstract List<QuestStep> SetSteps();

	protected override void Register()
	{
		QuestsByName.Add(FullName, this);
	}

	public static Quest GetQuest(string name)
	{
		return QuestsByName[name];
	}

	public static LocalizedText Localize(string postfix)
	{
		return Language.GetText($"Mods.{PoTMod.ModName}.Quests." + postfix);
	}

	public static string LocalizeValue(string postfix)
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

		StartQuest(player, tag.GetInt("currentQuest"));
		ActiveStep.Load(tag.Get<TagCompound>("currentQuestTag"));
	}

	public static string LoadFrom(TagCompound tag, Player player)
	{
		Type t = typeof(Quest).Assembly.GetType(tag.GetString("type"));

		if (t is null)
		{
			PoTMod.Instance.Logger.Error($"Could not load quest of {tag.GetString("type")}, was it removed?");
			return null;
		}

		string fullName = tag.GetString("type");
		GetQuest(fullName).Load(tag, player);
		return fullName;
	}
}

public class QuestUpdater : ModSystem
{
	public override void PostUpdateEverything()
	{
	}
}