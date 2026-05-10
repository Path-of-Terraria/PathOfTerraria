using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using Terraria.Enums;

namespace PathOfTerraria.Content.Skills.Magic;

public class ResistanceAura : Skill
{
	public const float ResistanceBonus = 0.2f;

	public override int MaxLevel => 1;
	public override SkillFunctionalityInfo Functionality => new(true, true, SkillCost.None);
	public override string Texture => $"{PoTMod.ModName}/Assets/Skills/Fireball";

	public override SkillTags Tags()
	{
		return SkillTags.Magic | SkillTags.Buff;
	}

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = 0;
		ResourceCost = 0;
		Duration = 0;
	}
}

internal sealed class ResistanceAuraPlayer : ModPlayer
{
	public override void PostUpdateEquips()
	{
		if (!IsAffectedByAnyResistanceAura())
		{
			return;
		}

		var elementalPlayer = Player.GetModPlayer<ElementalPlayer>();

		foreach (ElementInstance element in elementalPlayer.Container)
		{
			if (element.IsGeneric)
			{
				element.Resistance += ResistanceAura.ResistanceBonus;
			}
		}
	}

	private bool IsAffectedByAnyResistanceAura()
	{
		foreach (Player source in Main.ActivePlayers)
		{
			if (ProvidesAuraTo(source, Player))
			{
				return true;
			}
		}

		return false;
	}

	private static bool ProvidesAuraTo(Player source, Player target)
	{
		if (!source.active || source.dead || !HasActiveAura(source))
		{
			return false;
		}

		if (source.whoAmI == target.whoAmI)
		{
			return true;
		}

		return source.team != (int)Team.None && source.team == target.team;
	}

	private static bool HasActiveAura(Player source)
	{
		SkillCombatPlayer skillCombatPlayer = source.GetModPlayer<SkillCombatPlayer>();
		return skillCombatPlayer.TryGetSkill<ResistanceAura>(out Skill skill) && skill.AuraToggled;
	}
}
