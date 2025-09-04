using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Core.Items;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

/// <summary>
/// Stores elemental info for NPCs, namely their individual <see cref="ElementalContainer"/> instance.
/// </summary>
internal class ElementalNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public ElementalContainer Container { get; private set; } = new();

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
						Container.FireDamageModifier = Container.FireDamageModifier.ApplyOverride(mobDamage.Fire.Added, mobDamage.Fire.Conversion);
					}

					if (mobDamage.Cold != null)
					{
						Container.ColdDamageModifier = Container.ColdDamageModifier.ApplyOverride(mobDamage.Cold.Added, mobDamage.Cold.Conversion);
					}

					if (mobDamage.Lightning != null)
					{
						Container.LightningDamageModifier = Container.LightningDamageModifier.ApplyOverride(mobDamage.Lightning.Added, mobDamage.Lightning.Conversion);
					}

					break;
				}
			}
		}
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		Container.WriteTo(bitWriter, binaryWriter);
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		Container.ReadFrom(bitReader, binaryReader);
	}
}