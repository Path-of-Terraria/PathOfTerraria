using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Core.Systems.SkillSystem;

namespace PathOfTerraria.Content.Skills.Melee;

public class Berserk : Skill
{
	public override int MaxLevel => 3;

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = (60 - 5 * Level) * 60;
		Timer = 0;
		ManaCost = 10 + 5 * level;
		Duration = (15 + 5 * Level) * 60;
		WeaponType = Core.ItemType.Sword;
	}

	public override void UseSkill(Player player)
	{
		if (!CanUseSkill(player))
		{
			return;
		}

		player.statMana -= ManaCost;
		player.AddBuff(ModContent.BuffType<CustomRage>(), Duration);
		Timer = Cooldown;
	}
}