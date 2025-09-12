using PathOfTerraria.Common;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.SkillPassives.FlameSage;
using PathOfTerraria.Content.SkillTrees;
using Terraria.Localization;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class IntenseHeatBuff : ModBuff
{
	public override LocalizedText Description => base.Description.WithFormatArgs(MathUtils.Percent(IntenseHeat.DamageIncrease));
	public override void Update(Player player, ref int buffIndex)
	{
		int buff = player.GetPassiveStrength<FlameSageTree, ImprovedBurning>();

		ref ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container.FireDamageModifier;
		damage = damage.AddModifiers((int)(damage.DamageBonus * IntenseHeat.DamageIncrease * (1 + (buff * ImprovedBurning.DamageIncrease))), null);
	}
}