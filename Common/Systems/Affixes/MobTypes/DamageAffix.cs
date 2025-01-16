namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class DamageAffix : MobAffix
{
	public override void PostRarity(NPC npc)
	{
		npc.damage = (int)(npc.damage * 1.5f);
	}
}