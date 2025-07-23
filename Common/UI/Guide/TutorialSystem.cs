using PathOfTerraria.Core.UI;

namespace PathOfTerraria.Common.UI.Guide;

internal class TutorialSystem : ModSystem
{
	public override void OnWorldUnload()
	{
		if (UIManager.Has("Tutorial UI"))
		{
			UIManager.Data.RemoveAll(x => x.Identifier == "Tutorial UI");
		}
	}

	public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
	{
		if (Main.LocalPlayer.TryGetModPlayer(out TutorialPlayer plr) && !plr.CompletedTutorial && plr.HasFreeDay)
		{
			timeRate /= 3;
		}
	}
}
