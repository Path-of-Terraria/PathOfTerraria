using PathOfTerraria.Content.Skills.Ranged;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Buffs;

public sealed class BloodclotDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.position -= npc.velocity * 0.05f;

		if (Main.rand.NextBool(8))
		{
			Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood);
		}

		if (++npc.GetGlobalNPC<SiphonNPC>().BuffDrainTime % 60 == 0)
		{
			int def = npc.defense;
			npc.defense = 0;
			npc.SimpleStrikeNPC(3, 0, false, 0, null, false, 0, true);
			npc.defense = def;
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
			var source = PlayerDeathReason.ByCustomReason(NetworkText.FromKey("Mods.PathOfTerraria.Buffs.BloodclotDebuff.DeathText", player.name));
			player.Hurt(source, 3, 0, false, false, ImmunityCooldownID.TileContactDamage, false, 0, 1, 0);
		}
	}
}
