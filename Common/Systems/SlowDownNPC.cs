namespace PathOfTerraria.Common.Systems;

internal class SlowDownNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public float SlowDown = 0;

	public override void ResetEffects(NPC npc)
	{
		SlowDown = 0;
	}

	public override void AI(NPC npc)
	{
		SlowDown = Math.Clamp(SlowDown, 0, 1);
		npc.position -= npc.velocity * SlowDown;
	}
}
