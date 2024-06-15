using Microsoft.Build.Construction;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Questing.Quests.TestQuest.SubQuests;
using ReLogic.Threading;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Questing;
abstract class Quest
{
	public abstract QuestTypes QuestType { get; }
	protected abstract List<QuestStep> _subQuests { get; }
	private QuestStep _activeQuest;
	private int _currentQuest;
	private bool _completed = false;
	public abstract int NPCQuestGiver { get; }
	public abstract List<QuestReward> QuestRewards { get; }

	public void StartQuest(Player player, int currentQuest = 0)
	{
		_currentQuest = currentQuest;

		_activeQuest = _subQuests[_currentQuest];

		_activeQuest.Track(player, () =>
		{
			_activeQuest.UnTrack();
			StartQuest(player, currentQuest + 1);
		});
	}

	public string CurrentQuestString()
	{
		return _activeQuest.QuestString();
	}

	public void Save(TagCompound tag)
	{
		tag.Add("type", GetType().FullName);
		tag.Add("completed", _completed);
		tag.Add("currentQuest", _currentQuest);

		var newTag = new TagCompound();
		_activeQuest.Save(newTag);
		tag.Add("currentQuestTag", newTag);
	}

	public void Load(TagCompound tag, Player player)
	{
		if (_completed)
		{
			_completed = true;
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