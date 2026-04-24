using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class IgnitedDebuff : ModBuff
{
	public const int DefaultTickRate = 60;

	/// <summary>
	/// Applies this buff to a given entity (Player or NPC). If victim is an <see cref="NPC"/>, attacker is a <see cref="Player"/>. NPCs cannot apply this buff to other NPCs at this time.
	/// </summary>
	public static void ApplyTo(Entity attacker, Entity victim, int hitDamage, int time = 4 * 60, bool fromNet = false)
	{
		int stackDamage = hitDamage;
		float tickRate = DefaultTickRate;

		if (attacker is Player atkPlayer)
		{
			tickRate = atkPlayer.GetModPlayer<IgnitedPlayer>().IgniteDuration.ApplyTo(DefaultTickRate);
		}
		
		if (victim is NPC npc)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient && !fromNet)
			{
				AddIgnitedStackHandler.Send(npc, hitDamage, time);
			}

			IgnitedNPC ignited = npc.GetGlobalNPC<IgnitedNPC>();
			ignited.Stacks.Add(new IgnitedStack(time + 1, stackDamage));
			ignited.Stacks = [.. ignited.Stacks.OrderByDescending(x => x.BaseDamage)];
			ignited.LastTickCount = tickRate;
			
			if (ignited.Stacks[0].BaseDamage == stackDamage)
			{
				npc.AddBuff(ModContent.BuffType<IgnitedDebuff>(), time);
			}
		}
		else if (victim is Player player)
		{
			// Todo: test multiplayer
			IgnitedPlayer ignited = player.GetModPlayer<IgnitedPlayer>();
			ignited.Stacks.Add(new IgnitedStack(time + 1, stackDamage));
			ignited.Stacks = [.. ignited.Stacks.OrderByDescending(x => x.BaseDamage)];

			if (ignited.Stacks[0].BaseDamage == stackDamage)
			{
				player.AddBuff(ModContent.BuffType<IgnitedDebuff>(), time);
			}
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

	public override void Update(Player player, ref int buffIndex)
	{
		if (!player.TryGetModPlayer(out IgnitedPlayer ignited))
		{
			return;
		}

		if (ignited.Stacks.Count == 0)
		{
			player.DelBuff(buffIndex);
			buffIndex--;
		}
		else
		{
			player.buffTime[buffIndex] = Math.Max(player.buffTime[buffIndex], 2);

			if (Main.rand.NextBool(Math.Max(15 - (int)MathF.Sqrt(ignited.Stacks[0].BaseDamage / 30) + 1, 1)))
			{
				Dust.NewDust(player.position, player.width, player.height, DustID.Torch, Scale: Main.rand.NextFloat(1.5f, 2.5f));
			}
		}
	}
}

public class IgnitedStack(int time, int baseDamage)
{
	public int Time = time;
	public int BaseDamage = baseDamage;
}

internal class IgnitedNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public List<IgnitedStack> Stacks = [];
	public float ElapsedDoT = 0;
	public float LastTickCount = 60;

	private int _timer = 0;

	public override bool PreAI(NPC npc)
	{
		if (npc.HasBuff<IgnitedDebuff>() && Stacks.Count > 0)
		{
			int baseDamage = Stacks[0].BaseDamage;
			int halfDamage = baseDamage / 2;
			ElapsedDoT += baseDamage / 60f;

			if (++_timer > LastTickCount)
			{
				DoTFunctionality.ApplyDoT(npc, (int)ElapsedDoT, ref ElapsedDoT);

				_timer = 0;
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
		Stacks.Clear();
		short count = binaryReader.ReadInt16();

		if (count > 0)
		{
			for (int i = 0; i < count; ++i)
			{
				Stacks.Add(new IgnitedStack(binaryReader.ReadInt16(), binaryReader.ReadInt32()));
			}
		
			Stacks = [.. Stacks.OrderByDescending(x => x.BaseDamage)];
		}
	}
}

public class IgnitedPlayer : ModPlayer
{
	public StatModifier IgniteDuration = new();
	public StatModifier IgniteDamage = new();
	public float AddedIgniteChance = 0;

	public List<IgnitedStack> Stacks = [];

	public override void ResetEffects()
	{
		IgniteDuration = new();
		IgniteDamage = new();
		AddedIgniteChance = 0;
	}

	public override void UpdateBadLifeRegen()
	{
		if (Stacks.Count == 0) 
		{
			return;
		}

		Player.lifeRegen = Math.Min(Player.lifeRegen, 0);
		Player.lifeRegenTime = 0;
		Player.lifeRegen -= Stacks[0].BaseDamage;

		Stacks[0].Time--;

		if (Stacks[0].Time <= 0)
		{
			Player.ClearBuff(ModContent.BuffType<IgnitedDebuff>());
			Stacks.Clear();
		}
	}
}
