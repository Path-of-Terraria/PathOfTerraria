using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Core.Items;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

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
						if (!Container.FireDamageModifier.Valid)
						{
							Container.FireDamageModifier = new ElementalDamage(ElementType.Fire, mobDamage.Fire.Added ?? 0, mobDamage.Fire.Conversion ?? 0f);
						}
						else
						{
							Container.FireDamageModifier = Container.FireDamageModifier.ApplyOverride(mobDamage.Fire.Added, mobDamage.Fire.Conversion);
						}
					}

					if (mobDamage.Cold != null)
					{
						if (!Container.ColdDamageModifier.Valid)
						{
							Container.ColdDamageModifier = new ElementalDamage(ElementType.Cold, mobDamage.Cold.Added ?? 0, mobDamage.Cold.Conversion ?? 0f);
						}
						else
						{
							Container.ColdDamageModifier = Container.ColdDamageModifier.ApplyOverride(mobDamage.Cold.Added, mobDamage.Cold.Conversion);
						}
					}

					if (mobDamage.Lightning != null)
					{
						if (!Container.LightningDamageModifier.Valid)
						{
							Container.LightningDamageModifier = new ElementalDamage(ElementType.Lightning, mobDamage.Lightning.Added ?? 0, mobDamage.Lightning.Conversion ?? 0f);
						}
						else
						{
							Container.LightningDamageModifier = Container.LightningDamageModifier.ApplyOverride(mobDamage.Lightning.Added, mobDamage.Lightning.Conversion);
						}
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