using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Questing.QuestStepTypes;
internal class ParallelQuestStep(List<QuestStep> quests) : QuestStep
{
	private List<bool> _completed;

	private Action _onCompletion;
	readonly List<QuestStep> postTrackQuests = [];

	public void FinishSubTask(int id)
	{
		_completed[id] = true;

		if (_completed.All((completed) => completed))
		{
			_onCompletion();
		}
	}

	public override void Track(Player player, Action onCompletion)
	{
		_completed = []; // or load data if there is any

		_onCompletion = onCompletion;

		for (int i = 0; i < quests.Count; i++) 
		{
			if (_completed.Count > i && _completed[i])
			{
				continue;
			}

			_completed.Add(false); // or skip the track if its completed from loaded data
			int _i = i;

			QuestStep temp = quests[_i];
			temp.Track(player, () =>
			{
				FinishSubTask(_i); // if we dont do this it just keeps a reference to i and calls this with Count+1.
				temp.UnTrack();
			});

			postTrackQuests.Add(temp);
		}
	}

	public override string QuestString()
	{
		string s = "--\n";

		for (int i = 0; i < quests.Count; i++)
		{
			if (_completed != null && _completed[i])
			{
				s += quests[i].QuestCompleteString() + "\n";
			}
			else
			{
				s += quests[i].QuestString() + "\n";
			}
		}

		return s + "\n--";
	}

	public override string QuestCompleteString()
	{
		string s = "--\n";

		for (int i = 0; i < quests.Count; i++)
		{
			s += quests[i].QuestCompleteString() + "\n";
		}

		return s + "\n--";
	}

	public override void Save(TagCompound tag)
	{
		tag.Add("completed", _completed);

		List<TagCompound> subStepTags = [];
		foreach (QuestStep step in postTrackQuests)
		{
			var newTag = new TagCompound();
			step.Save(newTag);
			subStepTags.Add(newTag);
		}

		tag.Add("subSteps", subStepTags);
	}

	public override void Load(TagCompound tag)
	{
		_completed = tag.Get<List<bool>>("completed");

		List<TagCompound> subStepTags = tag.Get<List<TagCompound>>("subSteps");
		for (int i = 0; i < subStepTags.Count; i++)
		{
			quests[i].Load(subStepTags[i]);
		}
	}
}