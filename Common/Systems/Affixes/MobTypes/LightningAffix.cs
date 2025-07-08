using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.ElementalDamage;

namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class LightningAffix : MobAffix
{
	public override ItemRarity MinimumRarity => ItemRarity.Magic;

	public override void PostRarity(NPC npc)
	{
		if (!npc.GetGlobalNPC<ElementalNPC>().LightningDamage.Valid)
		{
			npc.GetGlobalNPC<ElementalNPC>().LightningDamage = new ElementalDamage.ElementalDamage(ElementType.Lightning, 0, 1);
		}
		else
		{
			npc.GetGlobalNPC<ElementalNPC>().LightningDamage.ApplyOverride(null, 1f);
		}

		npc.color = Color.Lerp(npc.color == Color.Transparent ? Color.White : npc.color, new Color(203, 235, 255), 0.25f);
	}
}