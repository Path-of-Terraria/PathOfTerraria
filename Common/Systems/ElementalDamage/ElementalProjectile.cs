using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ElementalDamage;
internal class ElementalProjectile : GlobalProjectile
{
	public override bool InstancePerEntity => true;

	public ElementalContainer Container = new();

	public override void OnSpawn(Projectile projectile, IEntitySource source)
	{
		if (source is EntitySource_Parent parent && parent.Entity is NPC npc && npc.TryGetGlobalNPC(out ElementalNPC elemNPC) && Main.netMode != NetmodeID.MultiplayerClient)
		{
			Container = elemNPC.Container.Clone();
			projectile.netUpdate = true;
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
