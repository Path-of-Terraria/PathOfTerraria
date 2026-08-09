using System.Collections.Generic;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class UniversalBuffingPlayer : ModPlayer
{
	public EntityModifier UniversalModifier;

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
	{
		EntityModifier modifier = Player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier;
		float critMultiplier = modifier.CriticalMultiplier.ApplyTo(1f);
		
		modifiers.CritDamage = modifiers.CritDamage.CombineWith(modifier.CriticalDamage);
		modifiers.CritDamage *= critMultiplier;
	}

	public override void UpdateEquips()
	{
		int mainItem = Main.mouseItem.IsAir || Main.mouseItem.damage <= 0 ? 0 : 58;

		if (!Player.inventory[mainItem].IsAir)
		{
			PoTItemHelper.ApplyAffixes(Player.inventory[mainItem], UniversalModifier, Player);
		}

		// Apply universal stat modifiers during equip updates so max life/mana are available
		// before vanilla clamps current resources for the tick.
		UniversalModifier.ApplyTo(Player);

		Player.statLifeMax = Math.Min(400, Player.statLifeMax2);
	}

	public override void ResetEffects()
	{
		UniversalModifier = new EntityModifier();
	}
	
	/// <summary>
	/// Used to apply on hit effects for affixes that have them.
	/// <inheritdoc/>
	/// </summary>
	/// <param name="target"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		foreach (KeyValuePair<int, OnHitDeBuffer.DebufferInstance> buff in UniversalModifier.Buffer)
		{
			int id = buff.Key;
			int time = 0;

			float roll = Main.rand.NextFloat();

			foreach (KeyValuePair<int, StatModifier> instance in buff.Value.ModifiersByDuration)
			{
				if (roll <= (instance.Value.ApplyTo(1f) - 1f))
				{
					time = Math.Max(time, instance.Key);
				}
			}

			if (time > 0)
			{
				if (buff.Value.OnApplyOverride is not null)
				{
					buff.Value.OnApplyOverride(Player, target, hit, damageDone, time);
				}
				else
				{
					target.AddBuff(id, time);
				}
			}
		}
	}

	public override void Unload()
	{
		UniversalModifier = null;
	}
}
