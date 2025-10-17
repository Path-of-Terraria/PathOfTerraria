using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Content.SkillAugments;
using System.Linq;
using Terraria.ID;

namespace PathOfTerraria.Common.Mechanics;

/// <summary> Encapsulates a skill used to summon an NPC. </summary>
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

				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					NPC npc = SentryNPC.Spawn(type, player, worldCoords, TotalDuration);
					npc.damage = GetTotalDamage(npc.damage);
				}
				else
				{
					ushort damage = (ushort)GetTotalDamage(ContentSamples.NpcsByNetId[type].damage);
					ModContent.GetInstance<SpawnSentryNPCHandler>().Send((ushort)type, (byte)player.whoAmI, worldCoords, (ushort)TotalDuration, damage);
				}
			}
		}
	}
}