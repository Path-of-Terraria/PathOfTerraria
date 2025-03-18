using PathOfTerraria.Core.UI;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.UI.Guide;

public enum TutorialCheck : byte
{
	AllocatedPassive,
	DeallocatedPassive,
	SelectedSkill,
	UsedASkill,
	SwappedWeapon,
	OpenedCharSheet,
	OpenedQuestBook,
	FinishedTutorial,
}

/// <summary>
/// Handles functionality for the <see cref="TutorialUIState"/> and associated checking flags.
/// </summary>
internal class TutorialPlayer : ModPlayer
{
	public HashSet<TutorialCheck> TutorialChecks = [];
	public byte TutorialStep = 0;

	public override void OnEnterWorld()
	{
		if (!TutorialChecks.Contains(TutorialCheck.FinishedTutorial))
		{
			UIManager.Register("Tutorial UI", "Vanilla: Player Chat", new TutorialUIState());
		}
	}

	public override void SaveData(TagCompound tag)
	{
		tag.Add("checks", TutorialChecks.Select(x => (byte)x).ToArray());
		tag.Add("step", TutorialStep);
	}

	public override void LoadData(TagCompound tag)
	{
		TutorialChecks.Clear();
		TutorialChecks = new HashSet<TutorialCheck>(tag.GetByteArray("checks").Select(x => (TutorialCheck)x));
		TutorialStep = tag.GetByte("step");

		TutorialUIState.StoredStep = TutorialStep;
	}
}
