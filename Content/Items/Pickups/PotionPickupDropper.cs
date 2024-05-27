using Terraria.GameContent.ItemDropRules;

namespace PathOfTerraria.Content.Items.Pickups;

internal class PotionPickupDropper : GlobalNPC
{
	public override void HitEffect(NPC npc, NPC.HitInfo hit)
	{
		if (npc.boss)
		{
			for (int k = 0; k < 10; k++)
			{
				int gate = (int)(npc.lifeMax * (k / 10f));

				if (npc.life >= gate && (npc.life - hit.Damage) < gate)
				{
					int amount = Main.rand.Next(2, 6);

					for (int i = 0; i < amount; i++)
					{
						int index = Item.NewItem(npc.GetSource_FromThis(), npc.Hitbox, ModContent.ItemType<HealingPotionPickup>());
						Main.item[index].velocity = Vector2.UnitX.RotatedBy(i / (float)amount * 6.28f) * 10;
					}
				}
			}
		}
	}

	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		if (!npc.boss)
		{
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HealingPotionPickup>(), 20, 1, 1));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ManaPotionPickup>(), 20, 1, 1));
		}
	}
}