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
		{ NPCID.OldMan, new OldManMarkerNPC() }
	};

	private static Asset<Texture2D> _markers = null;

	public void Load(Mod mod)
	{
		_markers = mod.Assets.Request<Texture2D>("Assets/UI/QuestMarkers");

		On_NPCHeadRenderer.DrawWithOutlines += Draw;
	}

	private void Draw(On_NPCHeadRenderer.orig_DrawWithOutlines orig, NPCHeadRenderer self, Entity entity, int headId, Vector2 position, 
		Color color, float rotation, float scale, SpriteEffects effects)
	{
		orig(self, entity, headId, position, color, rotation, scale, effects);

		if (entity is NPC npc && (npc.ModNPC is IQuestMarkerNPC questNPC || VanillaQuestMarkerNPCs.TryGetValue(npc.type, out questNPC)))
		{
			if (!questNPC.HasQuestMarker(out Quest quest))
			{
				return;
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

			Rectangle source = new(0, (sbyte)markerType * 32, 32, 32);
			Main.spriteBatch.Draw(_markers.Value, position - new Vector2(0, 36 * scale), source, color, 0f, source.Size() / 2f, scale, SpriteEffects.None, 0);
		}
	}

	public void Unload()
	{
	}
}
