using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Common.Systems.ModPlayers;
internal class UniversalBuffingPlayer : ModPlayer
{
	public EntityModifier UniversalModifier;
	public AffixTooltipsHandler AffixTooltipHandler = new();

	public override void PostUpdateEquips()
	{
		if (!Player.inventory[0].IsAir)
		{
			PoTItemHelper.ApplyAffixes(Player.inventory[0], UniversalModifier, Player);
		}

		UniversalModifier.ApplyTo(Player);

		Player.statLifeMax = Math.Min(400, Player.statLifeMax2);
	}

	public override void ResetEffects()
	{
		UniversalModifier = new EntityModifier();
		AffixTooltipHandler.Reset();
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

	/// <summary>
	/// Compares all existing affix bonuses to what is on the <paramref name="item"/>, and adds the tooltip lines.
	/// </summary>
	/// <param name="tooltips">List to add to.</param>
	/// <param name="item">Item to compare to.</param>
	public void PrepareComparisonTooltips(List<TooltipLine> tooltips, Item item)
	{
		List<ItemAffix> affixes = item.GetInstanceData().Affixes;
		AffixTooltip.AffixSource source = AffixTooltipsHandler.DetermineItemSource(item);

		foreach (KeyValuePair<Type, AffixTooltip> line in AffixTooltipHandler.Tooltips)
		{
			if (!affixes.Any(x => x.GetType() == line.Key))
			{
				line.Value.ClearValues(source);
			}
		}

		AffixTooltipHandler.ModifyTooltips(tooltips, item);
	}

	public override void Unload()
	{
		AffixTooltipHandler.Reset();
		AffixTooltipHandler = null;
		UniversalModifier = null;
	}
}
