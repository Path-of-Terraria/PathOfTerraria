namespace PathOfTerraria.Common.Systems;

/// <summary>
/// Used to make NPCs behave faster by using the <see cref="ExtraAISpeed"/> value.<br/>
/// This necessarily includes both behaviour speed (how fast the NPC does an action) and movement speed, which are inexorably tied.<br/>
/// If you want to slow down an NPC, use <see cref="SlowDownNPC"/> instead. These can be used in conjunction without issue.
/// </summary>
public class SpeedUpNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public float ExtraAISpeed = 0f;

	private float _extraAITimer = 0;
	private bool _boosting = false;

	public override void ResetEffects(NPC npc)
	{
		_boosting = false;
		ExtraAISpeed = 0;
	}

	public override void PostAI(NPC npc)
	{
		if (_boosting)
		{
			return;
		}

		if (ExtraAISpeed == 0)
		{
			_extraAITimer = 0;
			return;
		}

		_extraAITimer += ExtraAISpeed;
		Vector2 oldPosition = npc.position;

		while (_extraAITimer >= 1f)
		{
			_boosting = true;
			npc.AI();
			_boosting = false;
			_extraAITimer--;
		}

		npc.position = oldPosition;
	}
}
