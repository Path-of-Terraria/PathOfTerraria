using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class IgnitedDebuff : ModBuff
{
	public static void ApplyTo(NPC npc, int hitDamage, int time = 4 * 60, bool fromNet = false)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient && !fromNet)
		{
			AddIgnitedStackHandler.Send(npc, hitDamage, time);
		}

		// TODO: Add time duration modifier(s)
		IgnitedNPC ignited = npc.GetGlobalNPC<IgnitedNPC>();
		ignited.Stacks.Add(new IgnitedNPC.IgnitedStack(time + 1, hitDamage));
		ignited.Stacks = [.. ignited.Stacks.OrderByDescending(x => x.BaseDamage)];

		if (ignited.Stacks[0].BaseDamage == hitDamage)
		{
			npc.AddBuff(ModContent.BuffType<IgnitedDebuff>(), time);
		}
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (!npc.TryGetGlobalNPC(out IgnitedNPC ignited))
		{
			return;
		}

		if (ignited.Stacks.Count == 0)
		{
			npc.DelBuff(buffIndex);
			buffIndex--;
		}
		else
		{
			npc.buffTime[buffIndex] = Math.Max(npc.buffTime[buffIndex], 2);

			if (Main.rand.NextBool(Math.Max(15 - (int)MathF.Sqrt(ignited.Stacks[0].BaseDamage / 30) + 1, 1)))
			{
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, Scale: Main.rand.NextFloat(1.5f, 2.5f));
			}
		}
	}
}

internal class IgnitedNPC : GlobalNPC
{
	public class IgnitedStack(int time, int baseDamage)
	{
		public int Time = time;
		public int BaseDamage = baseDamage;
	}

	public override bool InstancePerEntity => true;

	public List<IgnitedStack> Stacks = [];
	public float ElapsedDoT = 0;

	public override bool PreAI(NPC npc)
	{
		if (npc.HasBuff<IgnitedDebuff>() && Stacks.Count > 0)
		{
			int baseDamage = Stacks[0].BaseDamage;
			int halfDamage = baseDamage / 2;
			ElapsedDoT += baseDamage / 60f;

			if (ElapsedDoT > halfDamage)
			{
				DoTFunctionality.ApplyDoT(npc, halfDamage, ref ElapsedDoT);
			}

			Stacks[0].Time--;

			if (Stacks[0].Time <= 0)
			{
				npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<IgnitedDebuff>()));
				Stacks.Clear();
			}
		}

		return true;
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		binaryWriter.Write((short)Stacks.Count);

		if (Stacks.Count > 0)
		{
			foreach (IgnitedStack stack in Stacks)
			{
				binaryWriter.Write((short)stack.Time);
				binaryWriter.Write(stack.BaseDamage);
			}
		}
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		if (binaryReader.ReadInt16() is not 0 and { } count)
		{
			for (int i = 0; i < count; ++i)
			{
				IgnitedStack stack = new(binaryReader.ReadInt16(), binaryReader.ReadInt32());
			}
		
			Stacks = [.. Stacks.OrderByDescending(x => x.BaseDamage)];
		}
	}
}