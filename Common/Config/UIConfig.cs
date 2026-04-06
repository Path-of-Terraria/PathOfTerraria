using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.Looting.VirtualBagUI;
using System.ComponentModel;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace PathOfTerraria.Common.Config;

public sealed class UIConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[Header("UI")]
	[DefaultValue(true)]
	public bool DisplayRichTooltipsInHotbar { get; set; } = true;

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
			PlayerUseSackOfHoldingHandler.Send(UseVirtualBag);
		}
	}
}