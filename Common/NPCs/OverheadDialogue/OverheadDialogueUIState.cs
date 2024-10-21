using PathOfTerraria.Core.UI.SmartUI;
using System.Collections.Generic;
using Terraria.UI;

namespace PathOfTerraria.Common.NPCs.OverheadDialogue;

internal class OverheadDialogueUIState : SmartUiState
{
	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: MP Player Names"));
	}

	public override void OnInitialize()
	{
		Visible = true;
		Scale = InterfaceScaleType.Game;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		var screen = new Rectangle((int)Main.screenPosition.X - 800, (int)Main.screenPosition.Y - 800, Main.screenWidth + 1600, Main.screenHeight + 1600);

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.Hitbox.Intersects(screen) && npc.ModNPC is IOverheadDialogueNPC bubbleNpc)
			{
				if (bubbleNpc.ShowDialogue() && bubbleNpc.CurrentDialogue is null)
				{
					bubbleNpc.CurrentDialogue = new OverheadDialogueInstance(bubbleNpc.GetDialogue());
				}

				bubbleNpc.CurrentDialogue?.Draw(npc.Center - Main.screenPosition - new Vector2(0, npc.height / 2 - npc.gfxOffY));

				if (bubbleNpc.CurrentDialogue is not null && bubbleNpc.CurrentDialogue.LifeTime >= bubbleNpc.CurrentDialogue.MaxLifeTime)
				{
					bubbleNpc.CurrentDialogue = null;
				}
			}
		}
	}
}
