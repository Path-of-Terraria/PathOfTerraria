using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives;

internal class ConcurrentBlasts(SkillTree tree) : SkillPassive(tree)
{
	/// <summary> A damage multiplier applied to targets based on <see cref="ConcurrentNPC.Vulnerable"/>. </summary>
	public const float BonusDamage = 1.15f;

	public override string DisplayTooltip => base.DisplayTooltip.FormatWith(Round(BonusDamage));
	private static int Round(float value)
	{
		return (int)Math.Round((value - 1) * 100);
	}
}

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