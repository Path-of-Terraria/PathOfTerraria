namespace PathOfTerraria.Content.SkillPassives.NovaTree;

internal class ConcurrentNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public bool Vulnerable => _blastTimer > 0;
	private int _blastTimer;

	public void ApplyBlastTimer(int time = 30)
	{
		_blastTimer = time;
	}

	public override void PostAI(NPC npc)
	{
		if (_blastTimer > 0)
		{
			_blastTimer--;
		}
	}
}