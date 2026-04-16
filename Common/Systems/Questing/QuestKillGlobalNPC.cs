namespace PathOfTerraria.Common.Systems.Questing;

internal sealed class QuestKillGlobalNPC : GlobalNPC
{
	public override void OnKill(NPC npc)
	{
		NPC.HitInfo hit = default;

		for (int i = 0; i < npc.playerInteraction.Length; i++)
		{
			if (!npc.playerInteraction[i])
			{
				continue;
			}

			Player player = Main.player[i];

			if (!player.active)
			{
				continue;
			}

			player.GetModPlayer<QuestModPlayer>().OnKillNPC(npc, hit, 0);
		}
	}
}
