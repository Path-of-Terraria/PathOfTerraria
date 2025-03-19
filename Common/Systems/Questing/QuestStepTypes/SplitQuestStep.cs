using System.Collections.Generic;
using Terraria.GameContent;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

/// <summary>
/// Wraps around two or more steps to do, finishing any one task finishes the <see cref="SplitQuestSteps"/>. 
/// For example, getting 10 Iron Bars or killing the Moon Lord.
/// </summary>
/// <param name="stepsLists">The steps to run in parallel.</param>
internal class SplitQuestSteps(List<QuestStep> stepsLists) : QuestStep
{
	public override int LineCount => steps.Count + 2;

	readonly List<QuestStep> steps = stepsLists;

	public override bool Track(Player player)
	{
		for (int i = 0; i < steps.Count; i++)
		{
			if (!steps[i].IsDone && steps[i].Track(player))
			{
				steps[i].IsDone = true;
				return true;
			};
		}

		return IsDone;
	}

	public override void DrawQuestStep(Vector2 topLeft, out int uiHeight, StepCompletion currentStep)
	{
		ReLogic.Graphics.DynamicSpriteFont font = FontAssets.ItemStack.Value;
		Color col = StepColor(currentStep);
		string[] texts = DisplayString().Split('\n');

		for (int i = 0; i < texts.Length; ++i)
		{
			Vector2 pos = topLeft + new Vector2(0, i * 20);
			Color color = col;

			DrawString(texts[i], pos, color, currentStep);
		}

		uiHeight = texts.Length * 22;
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

	//public override void Save(TagCompound tag)
	//{
	//	List<TagCompound> subStepTags = [];
	//	foreach (QuestStep step in steps)
	//	{
	//		var newTag = new TagCompound();
	//		step.Save(newTag);
	//		subStepTags.Add(newTag);
	//	}

	//	tag.Add("subSteps", subStepTags);
	//}

	//public override void Load(TagCompound tag)
	//{
	//	List<TagCompound> subStepTags = tag.Get<List<TagCompound>>("subSteps");
	//	for (int i = 0; i < subStepTags.Count; i++)
	//	{
	//		steps[i].Load(subStepTags[i]);
	//	}
	//}
}