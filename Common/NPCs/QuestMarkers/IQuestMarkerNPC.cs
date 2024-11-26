using PathOfTerraria.Common.Systems.Questing;

namespace PathOfTerraria.Common.NPCs.QuestMarkers;

internal interface IQuestMarkerNPC
{
	/// <summary>
	/// Whether the current NPC has a quest marker or not.
	/// </summary>
	/// <param name="quest">The quest associated with this NPC.</param>
	/// <returns>Whether the quest marker should show on the current NPC.</returns>
	public bool HasQuestMarker(out Quest quest);

	/// <summary>
	/// Allows you to override the marker type by returning true and changing <paramref name="markerType"/>. By default doesn't override anything.
	/// </summary>
	/// <param name="currentType">The default marker type value.</param>
	/// <param name="markerType">The new value for the marker type if this method returns true.</param>
	/// <returns>Whether to use the new <paramref name="markerType"/> value or not.</returns>
	public bool OverrideQuestMarker(QuestMarkerType currentType, out QuestMarkerType markerType)
	{
		markerType = QuestMarkerType.None;
		return false;
	}
}

public enum QuestMarkerType : sbyte
{
	None = -1,
	HasQuest,
	QuestComplete,
	QuestPending,
}
