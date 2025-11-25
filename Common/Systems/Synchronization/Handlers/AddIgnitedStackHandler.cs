using PathOfTerraria.Content.Buffs.ElementalBuffs;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class AddIgnitedStackHandler : Handler
{
	public static void Send(NPC npc, int hitDamage, int time = 4 * 60, int toClient = -1, int ignoreClient = -1)
	{
		ModPacket packet = Networking.GetPacket<AddIgnitedStackHandler>(9);
		packet.Write((short)npc.whoAmI);
		packet.Write(hitDamage);
		packet.Write((short)time);
		packet.Send(toClient, ignoreClient);
	}

	internal override void Receive(BinaryReader reader, byte sender)
	{
		short who = reader.ReadInt16();
		int hitDamage = reader.ReadInt32();
		int time = reader.ReadInt16();

		IgnitedDebuff.ApplyTo(Main.npc[who], hitDamage, time, true);

		if (Main.netMode == NetmodeID.Server)
		{
			Send(Main.npc[who], hitDamage, time, -1, sender);
		}
	}
}