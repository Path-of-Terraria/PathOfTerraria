using Humanizer;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class LifeRegenRatePassive : Passive
{
	public sealed class LifeRegenRatePassivePlayer : ModPlayer
	{
		public override void NaturalLifeRegen(ref float regen)
		{
			float level = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<LifeRegenRatePassive>();
			regen *= 1 + level / 100f;
		}
	}
}

internal class LifeRegenCountPassive : Passive
{
	public override string DisplayTooltip => Language.GetTextValue($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").FormatWith(MathUtils.Percent(Value));

	public override void BuffPlayer(Player player)
	{
		player.lifeRegen += Value * 2 * Level;
	}
}

internal class LifeOnKillPassive : Passive
{
	public const float Chance = 0.1f;
	public const float Amount = 0.05f;

	public sealed class LifeOnKillPassivePlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.life <= 0 && !target.SpawnedFromStatue && target.value != 0)
			{
				float level = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<LifeOnKillPassive>();
				if (Main.rand.NextFloat() < (Chance * level))
				{
					Player.Heal(Math.Max((int)(Player.statLifeMax2 * Amount), 2));
				}
			}
		}
	}

	public override string DisplayTooltip => Language.GetTextValue($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").FormatWith(MathUtils.Percent(Chance), MathUtils.Percent(Amount));
}