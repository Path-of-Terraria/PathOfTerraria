using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing;

public abstract class Quest
{
	public abstract QuestTypes QuestType { get; }
	public abstract int NPCQuestGiver { get; }
	public virtual string Name => "";
	public virtual string Description => "";

	public abstract List<QuestReward> QuestRewards { get; }
	public abstract List<QuestStep> QuestSteps { get; }
	public QuestStep ActiveStep => QuestSteps[CurrentQuest];

	public int CurrentQuest;
	public bool Completed;

	public void StartQuest(Player player, int currentQuest = 0)
	{
		CurrentQuest = currentQuest;

		if (CurrentQuest >= QuestSteps.Count)
		{
			Completed = true;
			QuestRewards.ForEach(qr => qr.GiveReward(player, player.Center));
			return;
		}
	}

	public void Update(Player player)
	{
		if (ActiveStep.Track(player))
		{
			ActiveStep.OnComplete();
			StartQuest(player, CurrentQuest + 1);
		}
	}

	public List<QuestStep> GetSteps()
	{
		return QuestSteps;
	}

	public string CurrentQuestString()
	{
		return ActiveStep.QuestString();
	}

	public string AllQuestStrings()
	{
		string s = "";

		for (int i = 0; i < CurrentQuest; i++)
		{
			s += QuestSteps[i].QuestCompleteString() + "\n";
		}

		return s + ActiveStep.QuestString();
	}

	public void Save(TagCompound tag)
	{
		tag.Add("type", GetType().FullName);
		tag.Add("completed", Completed);
		tag.Add("currentQuest", CurrentQuest);

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

	public static Quest LoadFrom(TagCompound tag, Player player)
	{
		Type t = typeof(Quest).Assembly.GetType(tag.GetString("type"));

		if (t is null)
		{
			PoTMod.Instance.Logger.Error($"Could not load quest of {tag.GetString("type")}, was it removed?");
			return null;
		}

		Quest quest = (Quest)Activator.CreateInstance(t);

		quest.Load(tag, player);

		return quest;
	}
}