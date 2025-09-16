using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Content.SkillAugments;
using System.Linq;

namespace PathOfTerraria.Common.Mechanics;

public abstract class SummonSkill : Skill
{
	public abstract int SummonNPCType { get; }

	public override bool CanUseSkill(Player player, ref SkillFailure failReason, bool justChecking = true)
	{
		if (!justChecking && !SentryNPC.FindRestingSpot(player, out _, new Vector2(0, -20)))
		{
			failReason = new(SkillFailReason.Other);
			return false;
		}

		return base.CanUseSkill(player, ref failReason, justChecking);
	}

	public override void UseSkill(Player player)
	{
		base.UseSkill(player);

		bool hasDuplicate = Tree.Augments.Any(x => x.Augment is Duplicate);
		for (int i = 0; i < (hasDuplicate ? 2 : 1); i++)
		{
			Vector2 offset = new(0, -20);
			if (hasDuplicate)
			{
				offset.X = (i == 0) ? -25 : 25;
			}

			if (SentryNPC.FindRestingSpot(player, out Vector2 worldCoords, offset))
			{
				SentryNPC.TryDestroyOldest(player);

				int type = SummonNPCType;

				NPC npc = SentryNPC.Spawn(type, player, worldCoords, TotalDuration);
				npc.damage = GetTotalDamage(npc.damage);
			}
		}
	}
}