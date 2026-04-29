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

	/// <summary>
	/// Shorthand for adding to <see cref="ElementalContainer"/>'s <see cref="ElementalDamage"/> values.
	/// </summary>
	public void AddElementalValues(params (ElementType type, int add, float conv)[] values)
	{
		Container.AddElementalValues(values);
	}

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

		if (source is EntitySource_ItemUse_WithAmmo itemUseSource)
		{
			SourceItem = itemUseSource.Item.type;

			if (Main.myPlayer == projectile.owner && itemUseSource.Player.TryGetModPlayer(out ElementalPlayer elemItemPlayer))
			{
				Container = elemItemPlayer.Container.Clone();
				projectile.netUpdate = true;
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
