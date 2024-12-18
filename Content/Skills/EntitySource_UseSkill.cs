using PathOfTerraria.Common.Mechanics;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Skills;

internal class EntitySource_UseSkill(Player parent, Skill skill, string context = null) : EntitySource_Parent(parent, context)
{
	public readonly Skill UsedSkill = skill;
}
