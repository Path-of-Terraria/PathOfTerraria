using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Questing;
abstract class Quest
{
	public abstract QuestTypes QuestType { get; }
	protected abstract List<QuestStep> _subQuests { get; }
	private QuestStep _activeQuest;
	private int _currentQuest;
	public bool Completed;
	public abstract int NPCQuestGiver { get; }
	public virtual string Name => "";
	public abstract List<QuestReward> QuestRewards { get; }

	public void StartQuest(Player player, int currentQuest = 0)
	{
		_currentQuest = currentQuest;

		if (_currentQuest >= _subQuests.Count)
		{
			Completed = true;
			QuestRewards.ForEach(qr => qr.GiveReward(player, player.Center));
			return;
		}

		_activeQuest = _subQuests[_currentQuest];

		_activeQuest.Track(player, () =>
		{
			_activeQuest.UnTrack();
			StartQuest(player, currentQuest + 1);
		});
	}
	
	public List<QuestStep> GetSteps()
	{
		return _subQuests;
	}

	public string CurrentQuestString()
	{
		return _activeQuest.QuestString();
	}

	public string AllQuestStrings()
	{
		string s = "";
		
		for (int i = 0; i < _currentQuest; i++)
		{
			s += _subQuests[i].QuestCompleteString() + "\n";
		}

		return s + _activeQuest.QuestString();
	}

	public void Save(TagCompound tag)
	{
		tag.Add("type", GetType().FullName);
		tag.Add("completed", Completed);
		tag.Add("currentQuest", _currentQuest);

		var newTag = new TagCompound();
		_activeQuest.Save(newTag);
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
		_activeQuest.Load(tag.Get<TagCompound>("currentQuestTag"));
	}

	public static Quest LoadFrom(TagCompound tag, Player player)
	{
		Type t = typeof(Quest).Assembly.GetType(tag.GetString("type"));

		if (t is null)
		{
			PathOfTerraria.Instance.Logger.Error($"Could not load quest of {tag.GetString("type")}, was it removed?");
			return null;
		}

		Quest quest = (Quest)Activator.CreateInstance(t);

		quest.Load(tag, player);

		return quest;
	}
}