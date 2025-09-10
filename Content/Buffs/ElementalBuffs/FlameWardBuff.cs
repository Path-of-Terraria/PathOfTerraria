using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.SkillPassives.FlameSage;
using Terraria.Localization;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class FlameWardBuff : ModBuff
{
	public override LocalizedText Description => base.Description.WithFormatArgs(MathUtils.Percent(FlameWard.ResistanceIncrease));
	public override void Update(Player player, ref int buffIndex)
	{
		player.GetModPlayer<ElementalPlayer>().Container.FireResistance += FlameWard.ResistanceIncrease;
	}
}