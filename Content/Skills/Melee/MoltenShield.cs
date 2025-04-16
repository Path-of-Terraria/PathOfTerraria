using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Content.Skills.Melee;

public class MoltenShield : Skill
{
	public override int MaxLevel => 3;
	public override List<SkillPassive> Passives => [];

	public override void LevelTo(byte level)
	{
		Level = level;
		MaxCooldown = 15 * 60;
		Timer = 0;
		ManaCost = 10 - Math.Max(2, (int)Level);
		Duration =  (5 + 2 * Level) * 60;
		WeaponType = ItemType.Sword;
	}

	public override void UseSkill(Player player)
	{
		// Level to the strength of all MoltenShellAffixes
		LevelTo((byte)player.GetModPlayer<AffixPlayer>().StrengthOf<MoltenShellAffix>());

		player.statMana -= ManaCost;
		player.GetModPlayer<MoltenShieldBuff.MoltenShieldPlayer>().SetBuff(Level, Duration);
		Timer = MaxCooldown;
	}

	public override bool CanEquipSkill(Player player)
	{
		// TODO: If this needs to be equippable without the affix, figure out that system
		return player.GetModPlayer<AffixPlayer>().StrengthOf<MoltenShellAffix>() > 0;
	}
}