using System.Collections.Generic;
using System.Reflection.Emit;
using MonoMod.Cil;
using PathOfTerraria.Content.GUI.UIItemSlots;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Helpers;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

internal class NewArmorPage : SmartUIState
{
	public override bool Visible => Main.playerInventory && Main.EquipPage == 0;
	
	public override void Load()
	{
		IL_Main.DrawInventory += static il =>
		{
			var cursor = new ILCursor(il);
			
			var label = cursor.DefineLabel();

			cursor.GotoNext(MoveType.After, i => i.MatchLdsfld<Main>(nameof(Main.EquipPage)), i => i.MatchBrtrue(out label));

			cursor.EmitBr(label);
		};
		
		MonoModHooks.Modify(typeof(AccessorySlotLoader).GetMethod(nameof(AccessorySlotLoader.DrawAccSlots)), il => new ILCursor(il).EmitRet());
	}

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
	}

	public override void OnInitialize()
	{
		var player = Main.CurrentPlayer;
	}
}
