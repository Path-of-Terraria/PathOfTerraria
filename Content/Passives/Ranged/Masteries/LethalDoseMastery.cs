using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Ranged.Masteries;

internal class LethalDoseMastery : Passive
{
	internal class LethalDoseNPC : GlobalNPC
	{
		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
		{
			if (player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<LethalDoseMastery>(out float value))
			{
				AdditiveScalingModifier.ApplyAdditiveLikeScalingItem(player, item, ref modifiers, MathF.Min(npc.GetGlobalNPC<PoisonNPC>().Stacks.Length * value / 100f, MaxDamageBoost / 100f));
			}
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (projectile.TryGetOwner(out Player player) && projectile.friendly && player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<LethalDoseMastery>(out float value))
			{
				Player projOwner = Main.player[projectile.owner];
				AdditiveScalingModifier.ApplyAdditiveLikeScalingProjectile(projOwner, projectile, ref modifiers, MathF.Min(npc.GetGlobalNPC<PoisonNPC>().Stacks.Length * value / 100f, MaxDamageBoost / 100f));
			}
		}
	}
	
	const float MaxDamageBoost = 40;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, MaxDamageBoost);
}