using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Core.Items;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

internal class ElementalNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public int Level;
	public ElementalContainer Container { get; private set; } = new();

	private bool _initialized = false;


	// On Spawn assign the element 
	public override void OnSpawn(NPC npc, IEntitySource source)
	{
		// Prevent double initialization (important for weird spawns)
		if (_initialized)
			return;

		_initialized = true;

		// Reset container once
		Container.Reset(false);

		// Roll level 
		Level = PoTItemHelper.PickItemLevel();

		// Apply elemental data
		ApplyDamageTypes(npc);
	}

	// =========================
	// ELEMENTAL APPLICATION
	// =========================
	public void ApplyDamageTypes(NPC npc, MobEntry entry = null)
	{
		List<MobDamage> elementalDamages = null;

		// Entry override (affixes etc.)
		if (entry != null && entry.DamageOverrides != null)
		{
			elementalDamages = entry.DamageOverrides;
		}
		// Base mob data
		else if (MobRegistry.TryGetMobData(npc.type, out MobData mobData))
		{
			elementalDamages = mobData.Damage;
		}

		if (elementalDamages == null || elementalDamages.Count == 0)
			return;
		

		foreach (MobDamage mobDamage in elementalDamages.OrderByDescending(d => d.MinLevel))
		{
			if (Level >= mobDamage.MinLevel)
			{
				if (mobDamage.Fire != null)
				{
					ref ElementalDamage dmg = ref Container[ElementType.Fire].DamageModifier;
					dmg = dmg.ApplyOverride(mobDamage.Fire.Added, mobDamage.Fire.Conversion);
				}

				if (mobDamage.Cold != null)
				{
					ref ElementalDamage dmg = ref Container[ElementType.Cold].DamageModifier;
					dmg = dmg.ApplyOverride(mobDamage.Cold.Added, mobDamage.Cold.Conversion);
				}

				if (mobDamage.Lightning != null)
				{
					ref ElementalDamage dmg = ref Container[ElementType.Lightning].DamageModifier;
					dmg = dmg.ApplyOverride(mobDamage.Lightning.Added, mobDamage.Lightning.Conversion);
				}

				if (mobDamage.Chaos != null)
				{
					ref ElementalDamage dmg = ref Container[ElementType.Chaos].DamageModifier;
					dmg = dmg.ApplyOverride(mobDamage.Chaos.Added, mobDamage.Chaos.Conversion);
				}

				break;
			}
		}
	}
	
	// Multiplayer Sync? 
	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		binaryWriter.Write(Level);
		Container.WriteTo(bitWriter, binaryWriter);
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		Level = binaryReader.ReadInt32();
		Container.ReadFrom(bitReader, binaryReader);
	}
}