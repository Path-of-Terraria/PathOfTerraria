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
	public override string Texture => $"{PoTMod.ModName}/Assets/Skills/ResistanceAura";

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
	private static readonly bool[] AuraActiveByPlayer = new bool[Main.maxPlayers];
	private static readonly bool[] AuraActiveByTeam = new bool[6];
	private static uint _lastAuraCacheUpdateTick;

	public override void PostUpdateEquips()
	{
		if (!IsAffectedByResistanceAura())
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

	private bool IsAffectedByResistanceAura()
	{
		UpdateAuraCache();

		if (AuraActiveByPlayer[Player.whoAmI])
		{
			return true;
		}

		return Player.team > (int)Team.None && AuraActiveByTeam[Player.team];
	}

	private static void UpdateAuraCache()
	{
		if (_lastAuraCacheUpdateTick == Main.GameUpdateCount)
		{
			return;
		}

		Array.Clear(AuraActiveByPlayer);
		Array.Clear(AuraActiveByTeam);

		foreach (Player source in Main.ActivePlayers)
		{
			if (!source.active || source.dead || !HasActiveAura(source))
			{
				continue;
			}

			AuraActiveByPlayer[source.whoAmI] = true;

			if (source.team > (int)Team.None)
			{
				AuraActiveByTeam[source.team] = true;
			}
		}

		_lastAuraCacheUpdateTick = Main.GameUpdateCount;
	}

	private static bool HasActiveAura(Player source)
	{
		SkillCombatPlayer skillCombatPlayer = source.GetModPlayer<SkillCombatPlayer>();
		return skillCombatPlayer.TryGetSkill<ResistanceAura>(out Skill skill) && skill.AuraToggled;
	}
}
