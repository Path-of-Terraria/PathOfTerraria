using PathOfTerraria.Core.UI;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Misc;

internal class OnlineModPopupPlayer : ModPlayer
{
	public bool DismissedPopup { get; set; }

	public override void SaveData(TagCompound tag)
	{
		if (DismissedPopup)
		{
			tag["dismissedOnlineModPopup"] = true;
		}
	}

	public override void LoadData(TagCompound tag)
	{
		DismissedPopup = tag.GetBool("dismissedOnlineModPopup");
	}

	public override void OnEnterWorld()
	{
		if (Main.dedServ || Main.myPlayer != Player.whoAmI || DismissedPopup || ModLoader.HasMod("PathOfTerrariaOnline"))
		{
			return;
		}

		UIManager.TryToggleOrRegister(OnlineModPopupUI.Identifier, "Vanilla: Mouse Text", new OnlineModPopupUI(), 0, InterfaceScaleType.UI);
	}
}
