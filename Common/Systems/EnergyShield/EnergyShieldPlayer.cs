using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.EnergyShield;

internal interface IEnergyShieldItem
{
}

internal sealed class EnergyShieldPlayer : ModPlayer
{
	private const int BaseRechargeDelayTicks = 4 * 60;
	private const float BaseRechargeRatePerSecond = 0.33f;

	private float armorEnergyShield;
	private float globalFlatEnergyShield;
	private float globalIncreasedEnergyShield;
	private float rechargeRateIncrease;
	private float fasterRechargeStart;
	private int ticksSinceDamage = BaseRechargeDelayTicks;

	public int MaximumEnergyShield { get; private set; }
	public float CurrentEnergyShield { get; private set; }
	public int CurrentEnergyShieldRounded => CurrentEnergyShield <= 0 ? 0 : (int)MathF.Ceiling(CurrentEnergyShield);

	public override void ResetEffects()
	{
		armorEnergyShield = 0;
		globalFlatEnergyShield = 0;
		globalIncreasedEnergyShield = 0;
		rechargeRateIncrease = 0;
		fasterRechargeStart = 0;
	}

	public void AddArmorEnergyShield(Item item)
	{
		float flat = 0;
		float increased = 0;

		foreach (ItemAffix affix in item.GetInstanceData().Affixes)
		{
			switch (affix)
			{
				case MaximumEnergyShieldAffix:
					flat += affix.Value;
					break;
				case IncreasedEnergyShieldAffix:
					increased += affix.Value;
					break;
			}
		}

		armorEnergyShield += flat * (1 + increased / 100f);
	}

	public void AddGlobalEnergyShield(float value)
	{
		globalFlatEnergyShield += value;
	}

	public void AddGlobalIncreasedEnergyShield(float value)
	{
		globalIncreasedEnergyShield += value;
	}

	public void AddEnergyShieldRechargeRate(float value)
	{
		rechargeRateIncrease += value;
	}

	public void AddFasterEnergyShieldRechargeStart(float value)
	{
		fasterRechargeStart += value;
	}

	public override void PostUpdateEquips()
	{
		int oldMax = MaximumEnergyShield;
		MaximumEnergyShield = Math.Max(0, (int)MathF.Round((armorEnergyShield + globalFlatEnergyShield) * (1 + globalIncreasedEnergyShield / 100f)));

		if (MaximumEnergyShield <= 0)
		{
			CurrentEnergyShield = 0;
			return;
		}

		if (oldMax <= 0)
		{
			CurrentEnergyShield = MaximumEnergyShield;
		}
		else if (CurrentEnergyShield > MaximumEnergyShield)
		{
			CurrentEnergyShield = MaximumEnergyShield;
		}

		if (CurrentEnergyShield >= MaximumEnergyShield)
		{
			ticksSinceDamage++;
			return;
		}

		ticksSinceDamage++;

		float delay = BaseRechargeDelayTicks / MathF.Max(0.01f, 1 + fasterRechargeStart / 100f);
		if (ticksSinceDamage < delay)
		{
			return;
		}

		float rechargeRate = BaseRechargeRatePerSecond * (1 + rechargeRateIncrease / 100f);
		CurrentEnergyShield = Math.Min(MaximumEnergyShield, CurrentEnergyShield + MaximumEnergyShield * rechargeRate / 60f);
	}

	public override bool FreeDodge(Player.HurtInfo info)
	{
		if (info.Damage <= 0 || CurrentEnergyShield < info.Damage)
		{
			return false;
		}

		AbsorbEnergyShield(info.Damage);
		Player.AddImmuneTime(ImmunityCooldownID.General, Player.longInvince ? 80 : 40);
		Player.immune = true;
		Player.immuneNoBlink = false;
		return true;
	}

	public override void ModifyHurt(ref Player.HurtModifiers modifiers)
	{
		modifiers.ModifyHurtInfo += AbsorbPartialEnergyShield;
	}

	public override void OnHurt(Player.HurtInfo info)
	{
		if (MaximumEnergyShield > 0)
		{
			ResetRechargeDelay();
		}
	}

	private void AbsorbPartialEnergyShield(ref Player.HurtInfo info)
	{
		if (info.Damage <= 1 || CurrentEnergyShield <= 0)
		{
			return;
		}

		int absorbed = Math.Min((int)MathF.Floor(CurrentEnergyShield), info.Damage - 1);
		if (absorbed <= 0)
		{
			return;
		}

		AbsorbEnergyShield(absorbed);
		info.Damage -= absorbed;
	}

	private void AbsorbEnergyShield(int damage)
	{
		CurrentEnergyShield = Math.Max(0, CurrentEnergyShield - damage);
		ResetRechargeDelay();

		Rectangle textArea = Player.Hitbox;
		textArea.Offset(0, -36);
		CombatText.NewText(textArea, new Color(92, 210, 255), damage);
	}

	private void ResetRechargeDelay()
	{
		ticksSinceDamage = 0;
	}
}

internal static class EnergyShieldItem
{
	public static int GetLocalEnergyShield(Item item)
	{
		float flat = 0;
		float increased = 0;

		foreach (ItemAffix affix in item.GetInstanceData().Affixes)
		{
			switch (affix)
			{
				case MaximumEnergyShieldAffix:
					flat += affix.Value;
					break;
				case IncreasedEnergyShieldAffix:
					increased += affix.Value;
					break;
			}
		}

		return Math.Max(0, (int)MathF.Round(flat * (1 + increased / 100f)));
	}

	public static bool IsGlobalEnergyShieldSource(Item item)
	{
		ItemType type = item.GetInstanceData().ItemType;
		return (type & ItemType.Armor) == ItemType.None;
	}
}
