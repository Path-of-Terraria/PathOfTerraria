using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class ElectrostaticAttractionMastery : Passive
{
	internal class ElectrostaticPlayer : ModPlayer, ElementalPlayerHooks.IElementalOnHitPlayer
	{
		public void ElementalOnHitNPC(NPC target, ElementInstance ele, ElementalContainer con, ElementalContainer other, int finalDamage, NPC.HitInfo hitInfo, Item item = null)
		{
			if (target.life > 0 && ele.Type == ElementType.Lightning)
			{
				foreach (NPC npc in Main.ActiveNPCs)
				{
					float dist = npc.Distance(target.Center);
					if (npc.whoAmI != target.whoAmI && npc.CanBeChasedBy() && dist < 800)
					{
						npc.velocity += (target.Center - npc.Center) * 0.015f * npc.knockBackResist;
					}
				}
			}
		}
	}
}
