using Humanizer;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class WhipCritDamageMastery : Passive
{
	internal sealed class BrandedNPC : GlobalNPC
	{
		public const int HitDurationMax = 60 * 3;
		public override bool InstancePerEntity => true;

		public int HitDuration;

		public override void PostAI(NPC npc)
		{
			if (HitDuration > 0)
			{
				HitDuration--;
			}
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (ProjectileID.Sets.IsAWhip[projectile.type] && projectile.TryGetOwner(out Player owner) && owner.GetModPlayer<PassiveTreePlayer>().HasNode<WhipCritDamageMastery>())
			{
				HitDuration = HitDurationMax;
			}
			else if (projectile.minion && HitDuration > 0)
			{
				modifiers.CritDamage *= 1 + BonusCritDamage;
			}
		}
	}

	public const float BonusCritDamage = 0.2f;

	public override string DisplayTooltip => Language.GetTextValue($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").FormatWith(MathUtils.Percent(BonusCritDamage));
}