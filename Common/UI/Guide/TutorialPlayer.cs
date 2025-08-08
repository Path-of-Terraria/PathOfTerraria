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
	FreeDayGone,

	/// <summary>
	/// Used to not re-award stuff, such as the free level, when restarting the tutorial.
	/// </summary>
	RestartedTutorial,
}

/// <summary>
/// Handles functionality for the <see cref="TutorialUIState"/> and associated checking flags.
/// </summary>
internal class TutorialPlayer : ModPlayer
{
	/// <summary> Whether the player has completed the tutorial (<see cref="TutorialCheck.FinishedTutorial"/>). </summary>
	public bool CompletedTutorial => TutorialChecks.Contains(TutorialCheck.FinishedTutorial);
	public bool HasFreeDay => !TutorialChecks.Contains(TutorialCheck.FreeDayGone);
	public bool Restarted => TutorialChecks.Contains(TutorialCheck.RestartedTutorial);

	public HashSet<TutorialCheck> TutorialChecks = [];
	public byte TutorialStep = 0;

	public override void OnEnterWorld()
	{
		TutorialUIState.StoredStep = TutorialStep - 1;
		TutorialUIState.FromLoad = true;

		if (!CompletedTutorial)
		{
			UIManager.Register("Tutorial UI", "Vanilla: Player Chat", new TutorialUIState(), 0, Terraria.UI.InterfaceScaleType.UI);
		}
	}

	public override void UpdateEquips()
	{
		if (!Main.dayTime)
		{
			TutorialChecks.Add(TutorialCheck.FreeDayGone);
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
		TutorialChecks = [.. tag.GetByteArray("checks").Select(x => (TutorialCheck)x)];
		TutorialStep = tag.GetByte("step");
	}
}
