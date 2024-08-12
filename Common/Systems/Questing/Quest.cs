using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing;

public abstract class Quest : ModType
{
	private static readonly Dictionary<string, Quest> QuestsByName = [];

	public abstract QuestTypes QuestType { get; }
	public abstract int NPCQuestGiver { get; }
	public virtual string Description => "";

	public abstract List<QuestReward> QuestRewards { get; }
	public abstract List<QuestStep> QuestSteps { get; }

	public QuestStep ActiveStep = null;

	public int CurrentStep;
	public bool Completed;
	public bool Active = false;

	protected override void Register()
	{
		ModTypeLookup<Quest>.Register(this);
		QuestsByName.Add(FullName, this);
	}

	public void StartQuest(Player player, int currentQuest = 0)
	{
		CurrentStep = currentQuest;
		ActiveStep = QuestSteps[CurrentStep];
		Active = true;

		if (CurrentStep >= QuestSteps.Count)
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
			StartQuest(player, CurrentStep + 1);
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

		for (int i = 0; i < CurrentStep; i++)
		{
			s += QuestSteps[i].QuestCompleteString() + "\n";
		}

		return s + ActiveStep.QuestString();
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
		ModContent.Find<Quest>(fullName).Load(tag, player);
		return fullName;
	}
}

public class QuestUpdater : ModSystem
{
	public override void PostUpdateEverything()
	{
	}
}