using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

internal class ElementalProjectile : GlobalProjectile
{
	public override bool InstancePerEntity => true;

	public ElementalContainer Container = new();
	
	public int SourceItem { get; private set; } = ItemID.None;

	public override void OnSpawn(Projectile projectile, IEntitySource source)
	{
		if (source is EntitySource_Parent parent && parent.Entity is NPC npc && npc.TryGetGlobalNPC(out ElementalNPC elemNPC) && Main.netMode != NetmodeID.MultiplayerClient)
		{
			Container = elemNPC.Container.Clone();
			projectile.netUpdate = true;
		}

		if (source is EntitySource_ItemUse_WithAmmo itemSource)
		{
			//Keeping track of the original weapon for elemental debuff purposes
			Item item = itemSource.Item;
			SourceItem = item.type;

			if (ElementalWeaponSets.GetElementalProportions(item.type, out var value))
			{
				foreach (KeyValuePair<ElementType, float> pair in value)
				{
					Container[pair.Key].DamageModifier.AddModifiers(null, pair.Value);
				}
			}
		}
	}

	public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		Container.WriteTo(bitWriter, binaryWriter);
	}

	public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
	{
		Container.ReadFrom(bitReader, binaryReader);
	}
}
