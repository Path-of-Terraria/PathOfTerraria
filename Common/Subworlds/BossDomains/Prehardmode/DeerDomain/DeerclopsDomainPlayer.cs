using PathOfTerraria.Common.NPCs;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.DeerDomain;

public class DeerclopsDomainPlayer : ModPlayer
{
	const int MaxTimeInSeconds = 8;

	public float Insanity => MathHelper.Clamp(LightTime / (MaxTimeInSeconds * 60f), 0, 1);

	private float LightTime = 0;
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
			if (LightTime > MaxTimeInSeconds * 60)
			{
				LightTime = MaxTimeInSeconds * 60;
			}

			LightTime -= 1.5f;
			ProjTime = 0;
		}
		else
		{
			LightTime++;

			if (LightTime > MaxTimeInSeconds * 60)
			{
				if (++ProjTime % (1f * 60) == 0)
				{
					Vector2 projPos = Player.Center + Main.rand.NextVector2CircularEdge(160, 160);
					Projectile.RandomizeInsanityShadowFor(Main.player[Player.whoAmI], true, out Vector2 spawnPosition, out Vector2 vel, out float ai, out float ai2);
					IEntitySource source = Terraria.Entity.GetSource_NaturalSpawn();
					int damage = ModeUtils.ProjectileDamage(60);
					int proj = Projectile.NewProjectile(source, spawnPosition, vel, ProjectileID.InsanityShadowHostile, damage, 6, Main.myPlayer, ai, ai2);

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