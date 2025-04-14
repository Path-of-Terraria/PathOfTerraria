using PathOfTerraria.Common.NPCs.Overhealth;
using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ShrineBuffs;

internal class UnstoppableBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		// This allows for otherwise buff immune NPCs to have this effect
		BuffID.Sets.IsATagBuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (npc.buffTime[buffIndex] >= 1)
		{
			npc.buffImmune[BuffID.Chilled] = true;
		}
		else
		{
			npc.buffImmune[BuffID.Chilled] = ContentSamples.NpcsByNetId[npc.type].buffImmune[BuffID.Chilled];
		}
	}

	public class UnstoppableNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private bool _lastHadUnstoppable = false;

		public override void ResetEffects(NPC npc)
		{
			if (!_lastHadUnstoppable && npc.HasBuff<UnstoppableBuff>())
			{
				npc.GetGlobalNPC<OverhealthNPC>().SetOverhealth(npc.life / 2);
			}

			_lastHadUnstoppable = npc.HasBuff<UnstoppableBuff>();
		}
	}

	public class UnstoppablePlayer : ModPlayer
	{
		private bool _lastHadUnstoppable = false;

		public override void ResetEffects()
		{
			if (!_lastHadUnstoppable && Player.HasBuff<UnstoppableBuff>())
			{
				Player.GetModPlayer<OverhealthPlayer>().SetOverhealth(Player.statLifeMax2 / 2);
			}

			_lastHadUnstoppable = Player.HasBuff<UnstoppableBuff>();
		}

		public override void PostUpdateMiscEffects()
		{
			if (_lastHadUnstoppable)
			{
				Player.buffImmune[BuffID.Chilled] = true;
			}
		}
	}
}
