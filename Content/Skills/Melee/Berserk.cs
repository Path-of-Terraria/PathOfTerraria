using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.SkillSystem;

namespace PathOfTerraria.Content.Skills.Melee;

public class Berserk : Skill
{
	public Berserk(int duration, int timer, int maxCooldown, int cooldown, int manaCost, GearType weaponType) : base(duration, timer, maxCooldown, cooldown, manaCost, weaponType)
	{
		Duration = duration;
		Cooldown = cooldown;
		MaxCooldown = maxCooldown;
		ManaCost = manaCost;
	}

	public Berserk()
	{
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

	public override string GetDescription(Player player)
	{
		return "Deal 150% more Damage while taking 5 more damage each time hit";
	}
}