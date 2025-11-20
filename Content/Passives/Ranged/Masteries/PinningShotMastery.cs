using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class PinningShotMastery : Passive
{
	internal class PinningShotPlayer : ModPlayer
	{
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (proj.CountsAsClass(DamageClass.Ranged) && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<PinningShotMastery>(out float value)
				&& target.HasBuff<RootedDebuff>())
			{
				modifiers.FinalDamage += value / 30f;
			}
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (proj.CountsAsClass(DamageClass.Ranged) && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<PinningShotMastery>(out float value)
				&& Main.rand.NextFloat() < value / 100f)
			{
				target.AddBuff(ModContent.BuffType<RootedDebuff>(), 3 * 60);
			}
		}
	}

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, Value * 3);
}