using PathOfTerraria.Content.Projectiles.Utility;
using SubworldLibrary;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.LunaticDomain;

internal class LunaticStartEventNPC : GlobalNPC
{
	public override void Load()
	{
		On_WorldGen.StartImpendingDoom += NoSubworldDoom;
	}

	private void NoSubworldDoom(On_WorldGen.orig_StartImpendingDoom orig, int countdownTime)
	{
		if (SubworldSystem.Current is not CultistDomain)
		{
			orig(countdownTime);
		}
		else if (Main.netMode != NetmodeID.MultiplayerClient)
		{ 
			int type = ModContent.ProjectileType<MLPortal>();
			Projectile.NewProjectile(new EntitySource_SpawnNPC(), new Vector2(Main.spawnTileX, Main.spawnTileY) * 16, Vector2.Zero, type, 0, 0, Main.myPlayer);

			WorldGen.BroadcastText(NetworkText.FromKey("Mods.PathOfTerraria.Misc.ImpendingDoom"), 50, 255, 130);

			NPC.MoonLordCountdown = 3000000; // This stops "Moon Lord has awoken!" from being printed to chat
			NPC.MaxMoonLordCountdown = NPC.MoonLordCountdown;
			NPC.LunarApocalypseIsUp = false;
		}
	}

	public override void OnKill(NPC npc)
	{
		if (npc.type == NPCID.CultistBoss && SubworldSystem.Current is CultistDomain)
		{
			PriorityQueue<Vector2, float> positions = new();
			positions.Enqueue(new Vector2((Main.maxTilesX / 2 - CultistDomain.PedestalDistance) * 16, 5200), Main.rand.NextFloat());
			positions.Enqueue(new Vector2((Main.maxTilesX / 2 + CultistDomain.EdgeDistance) * 16, 4800), Main.rand.NextFloat());
			positions.Enqueue(new Vector2((Main.maxTilesX / 2 - CultistDomain.EdgeDistance) * 16, 4800), Main.rand.NextFloat());
			positions.Enqueue(new Vector2((Main.maxTilesX / 2 + CultistDomain.PedestalDistance) * 16, 5200), Main.rand.NextFloat());

			PriorityQueue<int, float> types = new();
			types.Enqueue(NPCID.LunarTowerNebula, Main.rand.NextFloat());
			types.Enqueue(NPCID.LunarTowerSolar, Main.rand.NextFloat());
			types.Enqueue(NPCID.LunarTowerStardust, Main.rand.NextFloat());
			types.Enqueue(NPCID.LunarTowerVortex, Main.rand.NextFloat());

			Debug.Assert(positions.Count == types.Count);

			for (int i = 0; i < 4; ++i)
			{
				int type = types.Dequeue();
				Vector2 pos = positions.Dequeue() + Main.rand.NextVector2Circular(300, 300);

				NPC.NewNPC(new EntitySource_SpawnNPC(), (int)pos.X, (int)pos.Y, type, 0);
			}

			NPC.ShieldStrengthTowerNebula = NPC.ShieldStrengthTowerVortex = NPC.ShieldStrengthTowerStardust = NPC.ShieldStrengthTowerSolar = NPC.ShieldStrengthTowerMax;
			NPC.TowerActiveVortex = NPC.TowerActiveNebula = NPC.TowerActiveSolar = NPC.TowerActiveStardust = true;
			NPC.LunarApocalypseIsUp = true;
			NPC.ResetBadgerHatTime();
			NetMessage.SendData(MessageID.UpdateTowerShieldStrengths);
		}
	}
}
