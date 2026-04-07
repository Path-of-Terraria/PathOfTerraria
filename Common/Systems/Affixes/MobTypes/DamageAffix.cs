using PathOfTerraria.Common.Systems.MobSystem;

namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class DamageAffix : MobAffix
{
	public override void PostRarity(NPC npc)
	{
		npc.damage = (int)(npc.damage * MathHelper.Lerp(1.15f, 1.5f, PoTMobHelper.GetStatScaling()));
	}
}
