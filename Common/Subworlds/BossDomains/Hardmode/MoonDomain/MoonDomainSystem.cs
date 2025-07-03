using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

internal class MoonDomainSystem : ModSystem
{
	public static float EffectStrength = 1f;
	public static bool BossSpawned = false;

	private static bool ExitSpawned = false;

	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
	{
		if (SubworldSystem.Current is not MoonLordDomain)
		{
			return;
		}

		int y = (int)(Main.LocalPlayer.Center.Y / 16f);
		int top = MoonLordDomain.CloudTop + 30;

		if (y < top)
		{
			tileColor = Color.Black;
			backgroundColor = Color.Black;
		}
		else if (y < MoonLordDomain.CloudBottom)
		{
			tileColor = Color.Lerp(Color.Black, tileColor, (y - top) / (float)(MoonLordDomain.CloudBottom - top));
			backgroundColor = tileColor;
		}
	}

	public override void PreUpdateDusts()
	{
		if (SubworldSystem.Current is not MoonLordDomain)
		{
			return;
		}

		if (Main.spawnTileY > Main.maxTilesY - 250)
		{
			EffectStrength = 1f;
			return;
		}

		if (ExitSpawned)
		{
			EffectStrength = MathHelper.Lerp(EffectStrength, 0, 0.05f);
		}

		bool allPlayersAtop = true;

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.Center.Y / 16 > MoonLordDomain.TopOfTheWorld)
			{
				allPlayersAtop = false;
				break;
			}
		}

		if (!BossSpawned && allPlayersAtop && Main.CurrentFrameFlags.ActivePlayersCount > 0)
		{
			BossSpawned = true;

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int npc = NPC.NewNPC(new EntitySource_SpawnNPC(), Main.maxTilesX * 8, MoonLordDomain.TopOfTheWorld * 16 - 300, NPCID.MoonLordCore);
			}
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.MoonLordCore) && !ExitSpawned)
		{
			ExitSpawned = true;

			HashSet<Player> players = [];

			foreach (Player plr in Main.ActivePlayers)
			{
				if (!plr.dead)
				{
					players.Add(plr);
				}
			}

			IEntitySource src = Entity.GetSource_NaturalSpawn();
			Vector2 position = Main.rand.Next([.. players]).Center - new Vector2(0, 60);
			Projectile.NewProjectile(src, position, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
		}
	}
}
