using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class AnticipatedStrikeMastery : Passive
{
	internal class AnticipatedStrikePlayer : ModPlayer
	{
		public override void ResetEffects()
		{
			bool hasNode = Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<AnticipatedStrikeMastery>(out float value);

			if (hasNode && Player.GetModPlayer<SkillCombatPlayer>().TicksSinceSkillLastUsed > value * 60)
			{
				Player.AddBuff(ModContent.BuffType<AnticipatedStrikeBuff>(), 2);
			}
		}
	}
}
