using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds;

internal class MappingNPC : GlobalNPC
{
	public override void SetDefaults(NPC entity)
	{
		if (SubworldSystem.Current is MappingWorld && MappingWorld.Affixes is not null)
		{
			foreach (MapAffix affix in MappingWorld.Affixes)
			{
				affix.ModifyNewNPC(entity);
			}

			if (MappingWorld.AreaLevel > 50 && entity.lifeMax > 5)
			{
				float modifier = 1 + (MappingWorld.AreaLevel - 50) / 20f;
				entity.life = entity.lifeMax = (int)(entity.lifeMax * modifier);
				entity.defDamage = entity.damage = (int)(entity.damage * modifier);
			}
		}
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		if (SubworldSystem.Current is not MappingWorld)
		{
			return;
		}

		binaryWriter.Write(npc.lifeMax);
		binaryWriter.Write(npc.life);
		binaryWriter.Write(npc.defDamage);
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		if (SubworldSystem.Current is not MappingWorld)
		{
			return;
		}

		npc.lifeMax = binaryReader.ReadInt32();
		npc.life = binaryReader.ReadInt32();
		npc.defDamage = binaryReader.ReadInt32();
	}

	public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
	{
		if (SubworldSystem.Current is MappingWorld && MappingWorld.Affixes is not null)
		{
			foreach (MapAffix affix in MappingWorld.Affixes)
			{
				affix.ModifyHitPlayer(npc, target, ref modifiers);
			}
		}
	}

	public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
	{
		if (SubworldSystem.Current is MappingWorld && MappingWorld.Affixes is not null)
		{
			foreach (MapAffix affix in MappingWorld.Affixes)
			{
				affix.OnHitPlayer(npc, target, hurtInfo);
			}
		}
	}

	public override bool PreAI(NPC npc)
	{
		if (SubworldSystem.Current is MappingWorld && MappingWorld.Affixes is not null)
		{
			foreach (MapAffix affix in MappingWorld.Affixes)
			{
				affix.PreAI(npc);
			}
		}

		return true;
	}

	public override void OnKill(NPC npc)
	{
		if (npc.boss && SubworldSystem.Current is MappingWorld world and not BossDomainSubworld && Main.hardMode && PoTItemHelper.PickItemLevel() >= 45)
		{
			MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;

			if (DownedBossForTier())
			{
				tracker.AddCompletion(MappingWorld.MapTier);

				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					ModPacket packet = Networking.GetPacket(Networking.Message.SendMappingTierDown, 3);
					packet.Write((short)MappingWorld.MapTier);
					Networking.SendPacketToMainServer(packet);
				}
			}

			if (TierPassed(1) && !NPC.downedQueenSlime)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<QueenSlimeMap>());
			}
			
			if (TierPassed(2) && NPC.downedQueenSlime && !NPC.downedMechBoss2)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<TwinsMap>());
			}
			
			if (TierPassed(3) && NPC.downedMechBoss2 && !NPC.downedMechBoss1)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<DestroyerMap>());
			}

			if (TierPassed(4) && NPC.downedMechBoss1 && !NPC.downedMechBoss3)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<PrimeMap>());
			}

			if (TierPassed(5) && NPC.downedMechBoss3 && !NPC.downedPlantBoss)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<PlanteraMap>());
			}

			if (TierPassed(6) && NPC.downedPlantBoss && !NPC.downedGolemBoss)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<GolemMap>());
			}

			if (TierPassed(7) && NPC.downedGolemBoss && !NPC.downedFishron)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<FishronMap>());
			}

			if (TierPassed(8) && NPC.downedFishron && !NPC.downedEmpressOfLight)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<EoLMap>());
			}

			if (TierPassed(9) && NPC.downedEmpressOfLight && !NPC.downedAncientCultist)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<CultistMap>());
			}

			if (TierPassed(10) && NPC.downedMoonlord)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<MoonMap>());
			}

			return;

			bool TierPassed(int tier)
			{
				return tracker.CompletionsAtOrAboveTier(tier) >= MappingDomainSystem.RequiredCompletionsPerTier;
			}
		}
	}
}
