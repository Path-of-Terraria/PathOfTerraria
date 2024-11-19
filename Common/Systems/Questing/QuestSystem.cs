namespace PathOfTerraria.Common.Systems.Questing;

internal class QuestSystem : ModSystem
{
	public override void ClearWorld()
	{
		foreach (Quest quest in ModContent.GetContent<Quest>())
		{
			quest.Active = false;
			quest.Completed = false;
		}
	}
}
