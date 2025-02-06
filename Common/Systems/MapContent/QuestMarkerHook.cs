using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.NPCs.QuestMarkers;
using PathOfTerraria.Common.NPCs.QuestMarkers.Vanilla;
using PathOfTerraria.Common.Systems.Questing;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Graphics.Renderers;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.MapContent;

internal class QuestMarkerHook : ILoadable
{
	public static Dictionary<int, IQuestMarkerNPC> VanillaQuestMarkerNPCs = new()
	{
		{ NPCID.OldMan, new OldManMarkerNPC() },
		{ NPCID.Clothier, new OldManMarkerNPC() }
	};

	private static Asset<Texture2D> _markers = null;

	public void Load(Mod mod)
	{
		_markers = mod.Assets.Request<Texture2D>("Assets/UI/QuestMarkers");

		On_NPCHeadRenderer.DrawWithOutlines += Draw;
		IL_Main.DrawMap += HideOldManText;
	}

	private void HideOldManText(ILContext il)
	{
		ILCursor cursor = new(il);

		if (!cursor.TryGotoNext(MoveType.After, x => x.MatchCall<Main>("DrawNPCHeadFriendly"))) // Skip the first DrawNPCHeadFriendly, which doesn't show text
		{
			return;
		}

		if (!cursor.TryGotoNext(MoveType.After, x => x.MatchCall<Main>("DrawNPCHeadFriendly")))
		{
			return;
		}

		EmitModifyOldManName(cursor);

		if (!cursor.TryGotoNext(MoveType.After, x => x.MatchCall<Main>("DrawNPCHeadFriendly")))
		{
			return;
		}

		EmitModifyOldManName(cursor);
	}

	private static void EmitModifyOldManName(ILCursor cursor)
	{
		cursor.Emit(OpCodes.Ldloca_S, (byte)0);
		cursor.Emit(OpCodes.Ldloc_S, (byte)77);
		cursor.EmitDelegate(HideName);
	}

	public static void HideName(ref string text, int npcSlot)
	{
		NPC npc = Main.npc[npcSlot];
		Point mapPos = npc.Center.ToTileCoordinates();
		bool revealed = Main.Map.IsRevealed(mapPos.X, mapPos.Y);

		if (!revealed && npc.type == NPCID.OldMan)
		{
			text = string.Empty;
		}
	}

	private void Draw(On_NPCHeadRenderer.orig_DrawWithOutlines orig, NPCHeadRenderer self, Entity entity, int headId, Vector2 position, 
		Color color, float rotation, float scale, SpriteEffects effects)
	{
		Point mapPos = entity.Center.ToTileCoordinates();
		bool revealed = Main.Map.IsRevealed(mapPos.X, mapPos.Y);

		if (entity is NPC oldMan && oldMan.type == NPCID.OldMan && !revealed)
		{
			return; // Hide Old Man head icon unless the area's been explored already, better fitting vanilla's functionality & not giving away the dungeon immediately
		}

		orig(self, entity, headId, position, color, rotation, scale, effects);

		if (entity is NPC npc && (npc.ModNPC is IQuestMarkerNPC questNPC || VanillaQuestMarkerNPCs.TryGetValue(npc.type, out questNPC)) && revealed)
		{
			QuestMarkerType markerType = DetermineMarker(questNPC);

			if (markerType == QuestMarkerType.None)
			{
				return;
			}

			Rectangle source = new(0, (sbyte)markerType * 32, 32, 32);
			Main.spriteBatch.Draw(_markers.Value, position - new Vector2(0, 36 * scale), source, color, 0f, source.Size() / 2f, scale, SpriteEffects.None, 0);
		}
	}

	public static QuestMarkerType DetermineMarker(IQuestMarkerNPC questNPC)
	{
		if (!questNPC.HasQuestMarker(out Quest quest))
		{
			return QuestMarkerType.None;
		}

		QuestMarkerType markerType = QuestMarkerType.HasQuest; // Create default conditions for what the marker should be

		if (quest.ActiveStep is not null && quest.ActiveStep.CountsAsCompletedOnMarker)
		{
			markerType = QuestMarkerType.QuestComplete;
		}
		else if (quest.Active)
		{
			markerType = QuestMarkerType.QuestPending;
		}

		if (questNPC.OverrideQuestMarker(markerType, out QuestMarkerType newType)) // Override quest marker if desired
		{
			markerType = newType;
		}

		return markerType;
	}

	public void Unload()
	{
	}
}
