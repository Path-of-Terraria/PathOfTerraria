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
}
