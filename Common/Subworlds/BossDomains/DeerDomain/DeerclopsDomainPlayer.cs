using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.DeerDomain;

public class DeerclopsDomainPlayer : ModPlayer
{
	public float Insanity => MathHelper.Clamp(LightTime / (5 * 60f), 0, 1);
	
	private int LightTime = 0;
	private int ProjTime = 0;

	public override void UpdateEquips()
	{
		if (Main.dedServ)
		{
			return;
		}

		Point16 center = Player.Center.ToTileCoordinates16();
		float bright = Lighting.Brightness(center.X, center.Y);

		if (SubworldSystem.Current is not DeerclopsDomain || bright > 0.25f)
		{
			if (LightTime > 5 * 60)
			{
				LightTime = 5 * 60;
			}

			LightTime--;
			ProjTime = 0;
		}
		else
		{
			LightTime++;

			if (LightTime > 5 * 60)
			{
				if (++ProjTime % (1f * 60) == 0)
				{
					Vector2 projPos = Player.Center + Main.rand.NextVector2CircularEdge(160, 160);
					Projectile.RandomizeInsanityShadowFor(Main.player[Player.whoAmI], true, out Vector2 spawnPosition, out Vector2 vel, out float ai, out float ai2);
					IEntitySource source = Terraria.Entity.GetSource_NaturalSpawn();
					int proj = Projectile.NewProjectile(source, spawnPosition, vel, ProjectileID.InsanityShadowHostile, 60, 6, Main.myPlayer, ai, ai2);

					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
					}
				}
			}
		}

		if (LightTime <= 0)
		{
			LightTime = 0;
		}
	}
}