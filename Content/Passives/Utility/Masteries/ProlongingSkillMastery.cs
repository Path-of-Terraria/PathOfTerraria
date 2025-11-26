using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class ProlongingSkillMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.Duration += Value / 100f;
	}
}
