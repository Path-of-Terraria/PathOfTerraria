using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Swamp;
using SubworldLibrary;

namespace PathOfTerraria.Content.Swamp;

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
