namespace PathOfTerraria.Common.Subworlds.MappingAreas.DesertAreaContent;

internal class DesertPlayer : ModPlayer
{
	public override void PreUpdate()
	{
		Player.ZoneDesert = true;
	}
}
