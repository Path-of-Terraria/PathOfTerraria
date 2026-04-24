using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class TitanicImpactMastery : Passive
{
	internal class TitanicImpactPlayer : GlobalNPC
	{
		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
		{
			if (item.CountsAsClass(DamageClass.Melee) && player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<TitanicImpactMastery>(out float value)
				&& Main.rand.NextFloat() < value / 100f)
			{
				modifiers.Knockback *= 8;
				modifiers.FinalDamage += DamageIncrease / 100f;
			}
		}
	}

	const float DamageIncrease = 300;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, DamageIncrease);
}
