namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class LifeAffixes
{
	internal class DoubleLife : MobAffix
	{
		public override void PostRarity(NPC npc)
		{
			npc.lifeMax *= 2;
			npc.life = npc.lifeMax;
		}
	}
}