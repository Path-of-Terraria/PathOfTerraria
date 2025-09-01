using PathOfTerraria.Common.Enums;

namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class StalwartAffix : MobAffix
{
	public override ItemRarity MinimumRarity => ItemRarity.Magic;

	public override bool CanApplyTo(NPC npc)
	{
		return npc.knockBackResist > 0;
	}

	public override void PostRarity(NPC npc)
	{
		npc.knockBackResist = 0;
	}
}