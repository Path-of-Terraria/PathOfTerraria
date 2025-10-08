using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.UI.VirtualBagUI;
using System.ComponentModel;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace PathOfTerraria.Common.Config;

public sealed class UIConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[Header("UI")]
	[DefaultValue(false)]
	public bool PreventExpBarClicking { get; set; } = false;

	[DefaultValue(true)]
	public bool UseVirtualBag { get; set; } = true;

	public override void OnChanged()
	{
		if (Main.gameMenu)
		{
			return;
		}

		Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag = UseVirtualBag;

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			ModContent.GetInstance<PlayerUseSackOfHoldingHandler>().Send((byte)Main.myPlayer, UseVirtualBag);
		}
	}
}