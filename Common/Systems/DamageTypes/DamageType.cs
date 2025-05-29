namespace PathOfTerraria.Common.Systems.DamageTypes;
internal abstract class DamageType
{
	public int DamageBonus { get; init; }	
	public float DamageConversion { get; init; }	

	/// <summary> The (de)buff type to apply. Return 0 for none. </summary>
	public abstract int GetBuffType();

	/// <summary> The duration of the <see cref="GetBuffType()"/> applied (in ticks) </summary>
	public abstract int GetBuffDuration();

	/// <summary>
	/// Get the chance of applying the <see cref="GetBuffType()"/> 
	/// </summary>
	/// <param name="damagePercent"> The percentage of elemental damage done relative to the target's maxHealth </param>
	/// <returns> The chance of applying the debuff on hit </returns>
	public virtual float GetDebuffChance(float damagePercent)
	{
		float chance;
		// No chance if damage < 2% of target's max health
		if (damagePercent < 0.02f)
		{
			chance = 0f;
		}
		// Cap chance at 20% once damage percent >= 50%
		else if (damagePercent >= 0.5f)
		{
			chance = 0.2f;
		}
		// Linearly increase chance from 0% -> 20% as damage goes from 2% -> 50% of max health
		else
		{
			chance = damagePercent * (0.2f / 0.5f);
		}

		return chance;
	}
}
