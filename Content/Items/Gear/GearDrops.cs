namespace FunnyExperience.Content.Items.Gear
{
	internal class GearDrops : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			Gear.SpawnArmor(npc.Center);
		}
	}
}
