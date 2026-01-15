using PathOfTerraria.Content.Buffs;
using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;

internal class SwampPlayer : ModPlayer
{
	public override void PostUpdateEquips()
	{
		if (SubworldSystem.Current is SwampArea && Player.wet)
		{
			Player.AddBuff(ModContent.BuffType<SwampAlgaeBuff>(), 2);
		}
		else if (SubworldSystem.Current is not SwampArea)
		{
			Player.ClearBuff(ModContent.BuffType<SwampAlgaeBuff>());
		}
	}
}
