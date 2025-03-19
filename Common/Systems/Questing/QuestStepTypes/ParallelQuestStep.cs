using PathOfTerraria.Common.UI.Elements;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

/// <summary>
/// Wraps around two or more steps to do in parallel. For example, getting 10 Iron Bars, killing the Eye and exploring the Jungle.
/// </summary>
/// <param name="stepsLists">The steps to run in parallel.</param>
internal class ParallelQuestStep(List<QuestStep> stepsLists) : QuestStep
{
	public override int LineCount => steps.Count + 2;

	readonly List<QuestStep> steps = stepsLists;

	public void FinishSubTask(int id)
	{
		steps[id].IsDone = true;

		if (steps.All(x => x.IsDone))
		{
			IsDone = true;
		}
	}

	public override void DrawQuestStep(Vector2 topLeft, out int uiHeight, StepCompletion currentStep)
	{
		Vector2 pos = topLeft;
		uiHeight = 0;

		for (int i = 0; i < steps.Count; i++)
		{
			steps[i].DrawQuestStep(pos, out int height, steps[i].IsDone ? StepCompletion.Completed : currentStep);
			pos.Y += height;
			uiHeight += height;
		}
	}

	public override bool Track(Player player)
	{
		for (int i = 0; i < steps.Count; i++)
		{
			if (!steps[i].IsDone && steps[i].Track(player))
			{
				FinishSubTask(i);
			};
		}

		return IsDone;
	}

	public override string DisplayString()
	{
		string s = "--\n";

		for (int i = 0; i < steps.Count; i++)
		{
			s += i + 1 + ": " + steps[i].DisplayString();

			if (i != steps.Count - 1)
			{
				s += "\n";
			}
		}

		return s + "\n--";
	}

	public override void OnKillNPC(Player player, NPC target, NPC.HitInfo hitInfo, int damageDone)
	{
		foreach (QuestStep step in steps)
		{
			if (!step.IsDone)
			{
				step.OnKillNPC(player, target, hitInfo, damageDone);
			}
		}
	}

	public override void Save(TagCompound tag)
	{
		List<TagCompound> subStepTags = [];
		foreach (QuestStep step in steps)
		{
			var newTag = new TagCompound();
			step.Save(newTag);
			subStepTags.Add(newTag);
		}

		tag.Add("subSteps", subStepTags);
	}

	public override void Load(TagCompound tag)
	{
		List<TagCompound> subStepTags = tag.Get<List<TagCompound>>("subSteps");
		for (int i = 0; i < subStepTags.Count; i++)
		{
			steps[i].Load(subStepTags[i]);
		}
	}
}