using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;

internal class ActiveSkillPlayer : ModPlayer
{
	private readonly float[] _resourceTimers = [0, 0, 0];
	private readonly float[] _staticTimers = [0, 0, 0];

	public override void PostUpdateEquips()
	{
		if (Main.myPlayer != Player.whoAmI)
		{
			return;
		}

		Skill[] skills = Player.GetModPlayer<SkillCombatPlayer>().HotbarSkills;
		float proportionManaCost = 0;
		float proportionLifeCost = 0;

		for (int i = 0; i < skills.Length; i++)
		{
			Skill skill = skills[i];

			if (skill.AuraToggled)
			{
				_resourceTimers[i]++;
				_staticTimers[i]++;

				if (_resourceTimers[i] > 60)
				{
					if (skill.Functionality.Cost == SkillCost.ManaDrainPerSecond)
					{
						Player.manaRegenDelay = 80;

						if (!Player.CheckMana(skill.TotalResourceCost, true))
						{
							ToggleSkillOff(i);
						}
					}
					else if (skill.Functionality.Cost == SkillCost.LifeDrainPerSecond && Player.statLife < skill.TotalResourceCost)
					{
						ToggleSkillOff(i);
					}

					_resourceTimers[i] = 0;
				}

				if (skill.Functionality.Cost == SkillCost.ManaReserve)
				{
					proportionManaCost += skill.TotalResourceCost / 100f;
				}
				else if (skill.Functionality.Cost == SkillCost.HealthReserve)
				{
					proportionLifeCost += skill.TotalResourceCost / 100f;
				}
			}
			else
			{
				_resourceTimers[i] = 0;
				_staticTimers[i] = 0;
			}

			if (skill.AuraToggled && (skill.Functionality.ToggleAlwaysOn || Main.mouseLeft))
			{
				skill.ActiveUse(Player, ref _resourceTimers[i], _staticTimers[i]);
			}
		}

		Player.statLife = (int)MathF.Min(Player.statLife, Player.statLifeMax2 * (1 - proportionLifeCost));
		Player.statMana = (int)MathF.Min(Player.statMana, Player.statManaMax2 * (1 - proportionManaCost));
	}

	public void ToggleSkillOff(int i)
	{
		Skill skill = Player.GetModPlayer<SkillCombatPlayer>().HotbarSkills[i];

		if (skill.AuraToggled)
		{
			skill.UseSkill(Player);
		}
	}
}
