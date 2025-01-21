namespace PathOfTerraria.Common.Systems;

/// <summary>
/// Used to make NPCs move slower by using the <see cref="SpeedModifier"/> value.<br/>
/// Does not modify behaviour speed; as such, enemies like the Eye of Cthulhu which do an action for a given amount of time will not be modified,<br/>
/// but their speed in doing said action (like charging) will be reduced. Functionally, this means things like charges will be much shorter.<br/>
/// If you want to speed up an NPC, use <see cref="SpeedUpNPC"/> instead. These can be used in conjunction without issue.
/// </summary>
internal class SlowDownNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public float SpeedModifier = 0;

	public override void ResetEffects(NPC npc)
	{
		SpeedModifier = 0;
	}

	public override void AI(NPC npc)
	{
		SpeedModifier = Math.Clamp(SpeedModifier, 0, 1);
		npc.position -= npc.velocity * SpeedModifier;
	}
}
