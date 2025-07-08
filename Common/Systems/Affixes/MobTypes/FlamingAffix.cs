using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.ElementalDamage;

namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class FlamingAffix : MobAffix
{
	public override ItemRarity MinimumRarity => ItemRarity.Magic;

	public override void PostRarity(NPC npc)
	{
		if (!npc.GetGlobalNPC<ElementalNPC>().FireDamage.Valid)
		{
			npc.GetGlobalNPC<ElementalNPC>().FireDamage = new ElementalDamage.ElementalDamage(ElementType.Fire, 10, 1);
		}
		else
		{
			npc.GetGlobalNPC<ElementalNPC>().FireDamage.ApplyOverride(10, 1f);
		}

		npc.color = Color.Lerp(npc.color == Color.Transparent ? Color.White : npc.color, new Color(255, 181, 141), 0.25f);
	}
}