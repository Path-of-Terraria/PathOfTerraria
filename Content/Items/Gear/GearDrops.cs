namespace PathOfTerraria.Content.Items.Gear;

internal class GearDrops : GlobalNPC
{
	public override void OnKill(NPC npc)
	{
		if (npc.lifeMax <= 5 || npc.SpawnedFromStatue || npc.boss)
			return;

		int rand = Main.rand.Next(100);

		if (rand < 25) // 10
			Gear.SpawnItem(npc.Center);
	}
}