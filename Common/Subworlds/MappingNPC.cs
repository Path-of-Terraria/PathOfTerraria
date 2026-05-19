using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.MobSystem;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using System.IO;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds;

internal class MappingNPC : GlobalNPC
{
	public override void SetDefaults(NPC entity)
	{
		// Capture before our scaling below, in case this hook runs before ArpgNPC.SetDefaults.
		entity.GetGlobalNPC<ArpgNPC>().CaptureBaseLifeMax(entity);

		if (SubworldSystem.Current is MappingWorld && MappingWorld.Affixes is not null)
		{
			foreach (MapAffix affix in MappingWorld.Affixes)
			{
				affix.ModifyNewNPC(entity);
			}

			if (MappingWorld.AreaLevel > Content.Items.Consumables.Maps.Map.MaxOverworldLevel && entity.lifeMax > 5)
			{
				float modifier = 1 + (MappingWorld.AreaLevel - Content.Items.Consumables.Maps.Map.MaxOverworldLevel) / 20f;
				entity.life = entity.lifeMax = (int)(entity.lifeMax * modifier);
				entity.defDamage = entity.damage = (int)(entity.damage * modifier);
			}
		}
	}

	// This and RecieveExtraAI are both placeholders, just a way to sync the stats shown here
	// since incoming players don't have the affixes yet, so their clients spawn the unmodified version
	// and this just fixes that.
	// TODO: Do this better
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

}
