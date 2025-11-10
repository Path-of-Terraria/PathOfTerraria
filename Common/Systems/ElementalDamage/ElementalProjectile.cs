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
		if (source is EntitySource_Parent parent)
		{
			if (parent.Entity is NPC npc && npc.TryGetGlobalNPC(out ElementalNPC elemNPC) && Main.netMode != NetmodeID.MultiplayerClient)
			{
				Container = elemNPC.Container.Clone();
				projectile.netUpdate = true;
			}

			if (parent.Entity is Player player && player.TryGetModPlayer(out ElementalPlayer elemPlayer) && Main.myPlayer == projectile.owner)
			{
				Container = elemPlayer.Container.Clone();
				projectile.netUpdate = true;
			}

			if (parent.Entity is Projectile proj && proj.TryGetGlobalProjectile(out ElementalProjectile elemProj))
			{
				Container = elemProj.Container.Clone();
				projectile.netUpdate = true;
			}
		}

		if (source is EntitySource_ItemUse_WithAmmo { Item: Item item })
		{
			//Keeping track of the original weapon for elemental debuff purposes
			SourceItem = item.type;

			//if (ElementalWeaponSets.GetElementalProportions(item.type, out Dictionary<ElementType, float> value))
			//{
			//	foreach (KeyValuePair<ElementType, float> pair in value)
			//	{
			//		ref ElementalDamage mod = ref Container[pair.Key].DamageModifier;
			//		mod = mod.AddModifiers(null, pair.Value);
			//	}
			//}
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
