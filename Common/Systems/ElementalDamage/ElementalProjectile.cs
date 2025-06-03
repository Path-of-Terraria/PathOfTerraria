using PathOfTerraria.Common.Systems.MobSystem;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ElementalDamage;
internal class ElementalProjectile : GlobalProjectile
{
	public override bool InstancePerEntity => true;

	public ElementalDamage FireDamage;
	public ElementalDamage ColdDamage;
	public ElementalDamage LightningDamage;

	public override void OnSpawn(Projectile projectile, IEntitySource source)
	{
		if (source is EntitySource_Parent parent && parent.Entity is NPC npc && npc.TryGetGlobalNPC(out ArpgNPC arpgNPC) && Main.netMode != NetmodeID.MultiplayerClient)
		{
			FireDamage = arpgNPC.FireDamage;
			ColdDamage = arpgNPC.ColdDamage;
			LightningDamage = arpgNPC.LightningDamage;
			projectile.netUpdate = true;
		}
	}

	public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
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

	public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
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
