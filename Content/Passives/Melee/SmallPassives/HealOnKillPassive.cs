using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class HealOnKillPassive : Passive
{
	public sealed class HealOnKillPlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.life <= 0 && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<HealOnKillPassive>(out float value)
				&& Main.rand.NextFloat() < DefaultChance)
			{
				Player.Heal((int)value);
			}
		}
	}

	public const float DefaultChance = 0.1f;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, DefaultChance * 100);
}
