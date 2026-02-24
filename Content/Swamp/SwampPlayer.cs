using PathOfTerraria.Common.Systems.ModPlayers.LivesSystem;
using PathOfTerraria.Content.Buffs;
using SubworldLibrary;

namespace PathOfTerraria.Content.Swamp;

internal class SwampPlayer : ModPlayer, IPreDomainRespawnPlayer
{
	public void OnDomainRespawn()
	{
		Player.breath = Player.breathMax;
	}

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
