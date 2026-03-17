using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Dusts;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class BloodRushMastery : Passive
{
	internal class BloodRushPlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.life <= 0 && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<BloodRushMastery>(out float value) 
				&& Main.rand.NextFloat() < value)
			{
				Player.Heal((int)(Player.statLifeMax2 * (HealProportion / 100f)));

				for (int i = 0; i < 12; ++i)
				{
					SiphonDust.Spawn(target.position, Player.whoAmI);
				}
			}
		}
	}

	const float HealProportion = 25;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, HealProportion);
}
