using System.Linq;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Pickups;

internal class PotionPickupDropper : GlobalNPC
{
	public override void HitEffect(NPC npc, NPC.HitInfo hit)
	{
		if (npc.boss && Main.netMode != NetmodeID.MultiplayerClient)
		{
			for (int k = 0; k < 10; k++)
			{
				int gate = (int)(npc.lifeMax * (k / 10f));

				if (npc.life >= gate && (npc.life - hit.Damage) < gate)
				{
					int amount = Main.rand.Next(2, 6);

					for (int i = 0; i < amount; i++)
					{
						if (Main.netMode == NetmodeID.SinglePlayer)
						{
							int index = Item.NewItem(npc.GetSource_FromThis(), npc.Hitbox, ModContent.ItemType<HealingPotionPickup>());
							Main.item[index].velocity = Vector2.UnitX.RotatedBy(i / (float)amount * MathHelper.TwoPi) * 10;
						}
						else if (Main.netMode == NetmodeID.Server)
						{
							// On servers, spawn an item for each player that has interacted.
							int count = npc.playerInteraction.Count(x => x);

							for (int j = 0; j < count; ++j)
							{
								int index = Item.NewItem(npc.GetSource_FromThis(), npc.Hitbox, ModContent.ItemType<HealingPotionPickup>());
								Main.item[index].velocity = Vector2.UnitX.RotatedBy(j / (float)amount * MathHelper.TwoPi) * 10;
							}
						}
					}
				}
			}
		}
	}

	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		LeadingConditionRule notBoss = new LeadingConditionRule(new Conditions.LegacyHack_IsABoss());
		notBoss.OnSuccess(ItemDropRule.Common(ModContent.ItemType<HealingPotionPickup>(), 20, 1, 1));
		notBoss.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ManaPotionPickup>(), 20, 1, 1));
	}
}