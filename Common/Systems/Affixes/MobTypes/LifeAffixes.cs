using PathOfTerraria.Common.Systems.MobSystem;

namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class DoubleLife : MobAffix
{
	public override void PostRarity(NPC npc)
	{
		npc.lifeMax = (int)(npc.lifeMax * MathHelper.Lerp(1.25f, 2f, PoTMobHelper.GetStatScaling()));
		npc.life = npc.lifeMax;
	}
}
