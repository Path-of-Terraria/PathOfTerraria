using System.Linq;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Pickups;

internal class PotionPickupDropper : GlobalNPC
{
	public override void HitEffect(NPC npc, NPC.HitInfo hit)
	{
		if (IsNPCABoss(npc) && Main.netMode != NetmodeID.MultiplayerClient)
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
							Main.item[index].velocity = Vector2.UnitX.RotatedBy(i / (float)amount * MathHelper.TwoPi).RotatedByRandom(0.4f) * 10;
						}
						else if (Main.netMode == NetmodeID.Server)
						{
							// On servers, spawn an item for each player that has interacted.
							int count = npc.playerInteraction.Count(x => x);

							for (int j = 0; j < count; ++j)
							{
								int index = Item.NewItem(npc.GetSource_FromThis(), npc.position + new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height)), ModContent.ItemType<HealingPotionPickup>());
								Main.item[index].velocity = Vector2.UnitX.RotatedBy(i / (float)amount * MathHelper.TwoPi + j * MathHelper.PiOver4) * 10;

								NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index);
							}
						}
					}
				}
			}
		}
	}

	private static bool IsNPCABoss(NPC npc)
	{
		return npc.boss || npc.type == NPCID.GolemHead;
	}

	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		if (NPCID.Sets.ProjectileNPC[npc.type] || npc.CountsAsACritter || npc.friendly)
		{
			return;
		}

		LeadingConditionRule isBoss = new(new Conditions.LegacyHack_IsABoss());
		isBoss.OnSuccess(ItemDropRule.Common(ModContent.ItemType<HealingPotionPickup>(), 1, 1, 1), true);
		isBoss.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ManaPotionPickup>(), 1, 1, 1), true);

		isBoss.OnFailedConditions(ItemDropRule.Common(ModContent.ItemType<HealingPotionPickup>(), 7, 1, 1), true);
		isBoss.OnFailedConditions(ItemDropRule.Common(ModContent.ItemType<ManaPotionPickup>(), 7, 1, 1), true);
		
		npcLoot.Add(isBoss);
	}
}