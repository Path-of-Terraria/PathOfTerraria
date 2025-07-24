using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.NovaTree;

internal class ConcurrentBlasts(SkillTree tree) : SkillPassive(tree)
{
	/// <summary> A damage multiplier applied to targets based on <see cref="ConcurrentNPC.Vulnerable"/>. </summary>
	public const float BonusDamage = 1.15f;
	public override string DisplayTooltip => base.DisplayTooltip.FormatWith(MathUtils.Percent(BonusDamage - 1));
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