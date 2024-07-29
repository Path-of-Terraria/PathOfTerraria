using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Buffs;

public class BloodclotDebuff() : SmartBuff(true)
{
	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.position -= npc.velocity * 0.05f;

		if (Main.rand.NextBool(8))
		{
			Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood);
		}

		if (Main.time % 60 == 0)
		{
			npc.SimpleStrikeNPC(3, 0, false, 0, null, false, 0, true);
		}
	}

	public override void Update(Player player, ref int buffIndex)
	{
		player.position -= player.velocity * 0.05f;

		if (Main.rand.NextBool(8))
		{
			Dust.NewDust(player.position, player.width, player.height, DustID.Blood);
		}

		if (Main.time % 60 == 0)
		{
			string deathText = Language.GetText("Mods.PathOfTerraria.Buffs.BloodclotDebuff.DeathText").WithFormatArgs(player.name).Value;
			player.Hurt(PlayerDeathReason.ByCustomReason(deathText), 3, 0, false, false, -1, false, 0, 1, 0);
		}
	}
}
