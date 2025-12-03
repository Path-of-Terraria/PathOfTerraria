using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.Synchronization;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

/// <summary>
/// Allows you to apply an "echoed" elemental damage hit on a given NPC. Use <see cref="Apply(Player, NPC, int, int, ElementType)"/>.
/// </summary>
internal class ElementalReverberationBuff : ModBuff
{
	/// <summary>
	/// Adds a stack of elemental echoing damage to an NPC. Should only be called from the client.
	/// </summary>
	internal class ElementalReverberationHandler : Handler
	{
		public static void Send(short npc, short delay, int damage, ElementType elementType)
		{
			ModPacket packet = Networking.GetPacket<ElementalReverberationHandler>(8);
			packet.Write(npc);
			packet.Write(delay);
			packet.Write(damage);
			packet.Write((byte)elementType);
			packet.Send();
		}

		internal override void ServerReceive(BinaryReader reader, byte sender)
		{
			short npc = reader.ReadInt16();
			short delay = reader.ReadInt16();
			int damage = reader.ReadInt32();
			var type = (ElementType)reader.ReadByte();

			Apply(Main.player[sender], Main.npc[npc], delay, damage, type);
		}
	}

	public static void Apply(Player player, NPC npc, int delay, int damage, ElementType type)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			ElementalReverberationHandler.Send((short)npc.whoAmI, (short)delay, damage, type);
			return;
		}

		List<ElementalReverberationStack> stacks = npc.GetGlobalNPC<ElementalReverbNPC>().Stacks;
		stacks.Add(new((byte)player.whoAmI, (short)delay, damage, type));
		npc.AddBuff(ModContent.BuffType<ElementalReverberationBuff>(), delay);
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (npc.GetGlobalNPC<ElementalReverbNPC>().Stacks.Count == 0)
		{
			npc.DelBuff(buffIndex);
			buffIndex--;
		}
	}
}

internal class ElementalReverberationStack(byte player, short delay, int damage, ElementType type)
{
	public byte Player = player;
	public short Delay = delay;
	public int Damage = damage;
	public ElementType Type = type;
}

internal class ElementalReverbNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	internal readonly List<ElementalReverberationStack> Stacks = [];

	public override bool PreAI(NPC npc)
	{
		for (int i = 0; i < Stacks.Count; ++i)
		{
			ElementalReverberationStack stack = Stacks[i];
			stack.Delay--;

			if (stack.Delay == 0)
			{
				ElementalPlayer.ApplyElementalDamage(Main.player[stack.Player], npc, stack.Damage, stack.Type);
			}
		}

		Stacks.RemoveAll(x => x.Delay <= 0);
		return true;
	}
}