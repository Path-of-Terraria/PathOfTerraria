using Humanizer;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc;

internal class ReducedSkillCooldownPassive : Passive
{
	private const float CooldownBuff = 0.95f;

	public override string DisplayTooltip => base.DisplayTooltip.FormatWith((1 - CooldownBuff) * 100);

	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.Cooldown *= CooldownBuff;
	}
}
