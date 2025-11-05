using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class ElectrostaticAttractionMastery : Passive
{
	internal class ElectrostaticPlayer : ModPlayer, ElementalPlayerHooks.IElementalOnHitPlayer
	{
		private const int Duration = 50;

		internal int StackCount => stacks.Count;

		private readonly List<int> stacks = [];

		public override void ResetEffects()
		{
			for (int i = 0; i < stacks.Count; ++i)
			{
				stacks[i]--;
			}

			stacks.RemoveAll(x => x <= 0);

			Player.GetModPlayer<ElementalPlayer>().Container[ElementType.Lightning].Multiplier += StackCount / 20f;
		}

		public void ElementalOnHitNPC(NPC target, ElementInstance ele, ElementalContainer con, ElementalContainer other, int finalDamage, NPC.HitInfo hitInfo, Item item = null)
		{
			if (ele.Type == ElementType.Lightning && Player.GetModPlayer<PassiveTreePlayer>().HasNode<ElectrostaticAttractionMastery>())
			{
				Player.AddBuff(ModContent.BuffType<ElectrostaticBuff>(), Duration);
				stacks.Add(Duration);

				if (stacks.Count > 10)
				{
					stacks.RemoveAt(0);
				}
			}
		}
	}
}
