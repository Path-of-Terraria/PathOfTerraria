namespace PathOfTerraria.Core.Systems.Affixes.Affixes.Mob;

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