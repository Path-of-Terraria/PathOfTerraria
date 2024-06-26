using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes.WeaponAffixes;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.ModPlayers;
internal class UniversalBuffingPlayer : ModPlayer
{
	public EntityModifier UniversalModifier;

	public override void PostUpdateEquips()
	{
		(Player.inventory[0].ModItem as Gear)?.ApplyAffixes(UniversalModifier);

		UniversalModifier.ApplyTo(Player);

		Player.statLifeMax = Math.Min(400, Player.statLifeMax2);
	}

	public override void ResetEffects()
	{
		UniversalModifier = new EntityModifier();
	}
	
	/// <summary>
	/// Used to apply on hit effects for affixes that have them
	/// </summary>
	/// <param name="target"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		foreach (KeyValuePair<int, Dictionary<int, StatModifier>> buff in UniversalModifier.Buffer)
		{
			int id = buff.Key;
			int time = 0;

			float roll = Main.rand.NextFloat();

			foreach (KeyValuePair<int, StatModifier> instance in buff.Value)
			{
				if (roll <= (instance.Value.ApplyTo(1f) - 1f))
				{
					time = Math.Max(time, instance.Key);
				}
			}

			if (time > 0)
			{
				target.AddBuff(id, time);
			}
		}
	}
}
