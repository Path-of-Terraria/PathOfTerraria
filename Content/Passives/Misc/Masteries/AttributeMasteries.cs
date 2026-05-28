using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc.Masteries;

internal class TitanMastery : Passive { }

internal class ColossusMastery : Passive { }

internal class ArcanistMastery : Passive { }

internal class ConjurerMastery : Passive { }

internal class HunterMastery : Passive { }

internal class DexterityMarksmanMastery : Passive { }

internal sealed class AttributeMasteryPlayer : ModPlayer
{
	public override void UpdateEquips()
	{
		PassiveTreePlayer passivePlayer = Player.GetModPlayer<PassiveTreePlayer>();
		AttributesPlayer attributes = Player.GetModPlayer<AttributesPlayer>();

		if (passivePlayer.TryGetCumulativeValue<TitanMastery>(out float strengthBonus))
		{
			attributes.Strength *= 1 + strengthBonus / 100f;
		}

		if (passivePlayer.TryGetCumulativeValue<ArcanistMastery>(out float intelligenceBonus))
		{
			attributes.Intelligence *= 1 + intelligenceBonus / 100f;
		}

		if (passivePlayer.TryGetCumulativeValue<HunterMastery>(out float dexterityBonus))
		{
			attributes.Dexterity *= 1 + dexterityBonus / 100f;
		}

		if (passivePlayer.TryGetCumulativeValue<ColossusMastery>(out _))
		{
			float knockbackBonus = attributes.Strength / 1000f;

			if (knockbackBonus > 0f)
			{
				Player.GetKnockback(DamageClass.Generic) += knockbackBonus;
			}
		}

		if (passivePlayer.TryGetCumulativeValue<ConjurerMastery>(out _))
		{
			float areaBonus = attributes.Intelligence / 1000f;

			if (areaBonus > 0f)
			{
				Player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.AreaOfEffect += areaBonus;
			}
		}

		if (passivePlayer.TryGetCumulativeValue<DexterityMarksmanMastery>(out _))
		{
			float speedBonus = attributes.Dexterity / 1000f;

			if (speedBonus > 0f)
			{
				Player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ProjectileSpeed += speedBonus;
			}
		}
	}
}
