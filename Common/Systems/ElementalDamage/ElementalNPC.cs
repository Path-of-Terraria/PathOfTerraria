using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.MobSystem;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

internal class ElementalNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;
	public ElementalDamage FireDamage { get; set; }
	public ElementalDamage ColdDamage { get; set; }
	public ElementalDamage LightningDamage { get; set; }

	/// <summary>
	/// Applies <see cref="ElementalDamage"/>s to this <paramref name="npc"/> from its associated <see cref="MobData.Damage"/> data. 
	/// <br/> If the <paramref name="entry"/> parameter is provided, it will apply the matching <see cref="MobEntry.DamageOverrides"/> instead. </summary>
	public void ApplyDamageTypes(NPC npc, MobEntry entry = null)
	{
		List<MobDamage> elementalDamages = null;

		// Apply entry overrides if evaluating an entry and overrides are available
		bool fromEntry = entry != null && entry.DamageOverrides != null;
		if (fromEntry)
		{
			elementalDamages = entry.DamageOverrides;
		}
		// Otherwise, try get the root (common) data for this type
		else if (MobRegistry.TryGetMobData(npc.type, out MobData mobData))
		{
			elementalDamages = mobData.Damage;
		}

		if (elementalDamages != null && elementalDamages.Count > 0)
		{
			int level = PoTItemHelper.PickItemLevel();
			foreach (MobDamage mobDamage in elementalDamages.OrderByDescending(d => d.MinLevel))
			{
				if (level >= mobDamage.MinLevel)
				{
					if (mobDamage.Fire != null)
					{
						if (!FireDamage.Valid)
						{
							FireDamage = new ElementalDamage(ElementType.Fire, mobDamage.Fire.Added ?? 0, mobDamage.Fire.Conversion ?? 0f);
						}
						else
						{
							FireDamage = FireDamage.ApplyOverride(mobDamage.Fire.Added, mobDamage.Fire.Conversion);
						}
					}

					if (mobDamage.Cold != null)
					{
						if (!ColdDamage.Valid)
						{
							ColdDamage = new ElementalDamage(ElementType.Cold, mobDamage.Cold.Added ?? 0, mobDamage.Cold.Conversion ?? 0f);
						}
						else
						{
							ColdDamage = ColdDamage.ApplyOverride(mobDamage.Cold.Added, mobDamage.Cold.Conversion);
						}
					}

					if (mobDamage.Lightning != null)
					{
						if (!LightningDamage.Valid)
						{
							LightningDamage = new ElementalDamage(ElementType.Lightning, mobDamage.Lightning.Added ?? 0, mobDamage.Lightning.Conversion ?? 0f);
						}
						else
						{
							LightningDamage = LightningDamage.ApplyOverride(mobDamage.Lightning.Added, mobDamage.Lightning.Conversion);
						}
					}

					break;
				}
			}
		}
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		bitWriter.WriteBit(FireDamage.Valid);
		bitWriter.WriteBit(ColdDamage.Valid);
		bitWriter.WriteBit(LightningDamage.Valid);

		if (FireDamage.Valid)
		{
			FireDamage.Write(binaryWriter);
		}

		if (ColdDamage.Valid)
		{
			ColdDamage.Write(binaryWriter);
		}

		if (LightningDamage.Valid)
		{
			LightningDamage.Write(binaryWriter);
		}
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		bool fire = bitReader.ReadBit();
		bool cold = bitReader.ReadBit();
		bool lightning = bitReader.ReadBit();

		if (fire)
		{
			FireDamage = ElementalDamage.Read(binaryReader);
		}

		if (cold)
		{
			ColdDamage = ElementalDamage.Read(binaryReader);
		}

		if (lightning)
		{
			ColdDamage = ElementalDamage.Read(binaryReader);
		}
	}
}