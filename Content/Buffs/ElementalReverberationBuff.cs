using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Buffs;

internal class ElementalReverberationBuff : ModBuff
{
	public static void Apply(Player player, NPC npc, int delay, int damage, ElementType type)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			BleedStackHandler.Send((short)npc.whoAmI, (ushort)delay, (ushort)damage);
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

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		binaryWriter.Write((short)Stacks.Count);

		foreach (ElementalReverberationStack stack in Stacks)
		{
			binaryWriter.Write(stack.Player);
			binaryWriter.Write(stack.Delay);
			binaryWriter.Write(stack.Damage);
			binaryWriter.Write((byte)stack.Type);
		}
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
	{
		Stacks.Clear();
		short count = reader.ReadInt16();

		for (int i = 0; i < count; ++i)
		{
			Stacks.Add(new ElementalReverberationStack(reader.ReadByte(), reader.ReadInt16(), reader.ReadInt32(), (ElementType)reader.ReadByte()));
		}
	}
}