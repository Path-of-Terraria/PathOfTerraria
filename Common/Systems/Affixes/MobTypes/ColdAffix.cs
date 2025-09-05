using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.ElementalDamage;

namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class ColdAffix : MobAffix
{
	public override ItemRarity MinimumRarity => ItemRarity.Magic;

	public override void PostRarity(NPC npc)
	{
		npc.GetGlobalNPC<ElementalNPC>().Container.ColdDamageModifier.AddModifiers(10, 1f);
		npc.color = Color.Lerp(npc.color == Color.Transparent ? Color.White : npc.color, new Color(72, 139, 179), 0.25f);
	}
}