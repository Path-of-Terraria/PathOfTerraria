using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Content.Skills.Melee;

public class Berserk : Skill
{
	public override int MaxLevel => 3;
	public override List<SkillPassive> Passives => [];

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = (60 - 5 * Level) * 60;
		Timer = 0;
		ManaCost = 10 + 5 * level;
		Duration = (15 + 5 * Level) * 60;
		WeaponType = ItemType.Sword;
	}

	public override void UseSkill(Player player)
	{
		player.statMana -= ManaCost;
		player.AddBuff(ModContent.BuffType<RageBuff>(), Duration);
		Timer = Cooldown;
	}
}