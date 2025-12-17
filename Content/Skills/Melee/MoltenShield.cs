using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Content.Skills.Melee;

public class MoltenShield : Skill
{
	public override int MaxLevel => 3;

    public override SkillTags Tags()
    {
        return SkillTags.Melee | SkillTags.Buff;
    }

    public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = 15 * 60;
		ResourceCost = 10 - Math.Max(2, (int)Level);
		Duration =  (5 + 2 * Level) * 60;
		WeaponType = ItemType.Melee;
	}

	protected override void InternalUseSkill(Player player)
	{
		// Level to the strength of all MoltenShellAffixes
		LevelTo((byte)player.GetModPlayer<AffixPlayer>().StrengthOf<MoltenShellAffix>());
		player.GetModPlayer<MoltenShieldBuff.MoltenShieldPlayer>().SetBuff(Level, TotalDuration);
	}

	public override bool CanUseSkill(Player player, ref SkillFailure failReason, bool justChecking)
	{
		if (!player.HeldItem.CountsAsClass(DamageClass.Melee))
		{
			failReason = new SkillFailure(SkillFailReason.NeedsMelee);
			return false;
		}

		return base.CanUseSkill(player, ref failReason, true);
	}

	protected override bool ProtectedCanEquip(Player player, ref SkillFailure failReason)
	{
		if (player.GetModPlayer<AffixPlayer>().StrengthOf<MoltenShellAffix>() <= 0)
		{
			failReason = new SkillFailure(SkillFailReason.Other, "MissingAffix", DisplayName.Value);
			return false;
		}

		return true;
	}
}