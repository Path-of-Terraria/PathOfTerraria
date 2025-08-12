using PathOfTerraria.Core.UI;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.UI.Guide;

internal class TutorialSystem : ModSystem
{
	public bool FreeDay = true;

	public override void OnWorldUnload()
	{
		if (UIManager.Has("Tutorial UI"))
		{
			UIManager.Data.RemoveAll(x => x.Identifier == "Tutorial UI");
		}
	}

	public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
	{
		if (FreeDay)
		{
			timeRate /= 3;
		}
	}

	public override void PostUpdateGores()
	{
		if (!Main.dayTime)
		{
			FreeDay = false;
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add("freeDay", FreeDay);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		FreeDay = tag.GetBool("freeDay");
	}
}
