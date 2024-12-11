using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Content.Projectiles.Hostile;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.HellEvent;

internal class HellEventPlayer : ModPlayer
{
	/// <summary>
	/// Strength of the event for this local player. Should only be run when <c>Player.whoAmI == Main.myPlayer</c>.
	/// </summary>
	public float LocalEventStrength { get; set; }

	private float tileFallTimer = 0;
	private float lavaEruptTimer = 0;

	public override void PreUpdate()
	{
		if (SubworldSystem.Current is not null and not WallOfFleshDomain)
		{
			LocalEventStrength = 0;
			return;
		}

		// Add screenshake for "earthquake" effect
		if (HellEventSystem.EventOccuring && Player.Center.Y / 16 > Main.maxTilesY - 250)
		{
			LocalEventStrength = 1f;

			if (Player.Center.Y / 16 < Main.maxTilesY - 200) // Create fadeout strength
			{
				int y = (int)(Player.Center.Y / 16f);
				LocalEventStrength = (Main.maxTilesY - 200f - y) / 50f;
			}

			Vector2 rotation = (Main.rand.NextFloat() * (MathHelper.Pi * 2f)).ToRotationVector2();
			float str = 10f * HellEventSystem.EventStrength * LocalEventStrength;
			PunchCameraModifier modifier = new(Player.Center, rotation, 1.5f * HellEventSystem.EventStrength, str, 2, 4000, "SkeletronRitual");
			Main.instance.CameraModifiers.Add(modifier);

			UpdateTiles();

			if (Main.myPlayer == Player.whoAmI && Quest.GetLocalPlayerInstance<WoFQuest>().CanBeStarted)
			{
				Player.GetModPlayer<QuestModPlayer>().StartQuest($"{PoTMod.ModName}/{nameof(WoFQuest)}");
			}
		}
	}

	private void UpdateTiles()
	{
		float mul = SubworldSystem.Current is WallOfFleshDomain ? 0.5f : 1f * LocalEventStrength;
		tileFallTimer += 2.5f / Main.CurrentFrameFlags.ActivePlayersCount * mul;
		lavaEruptTimer += 1.2f / Main.CurrentFrameFlags.ActivePlayersCount * mul;

		while (tileFallTimer > 2)
		{
			SpawnFallingAsh();
			tileFallTimer -= 2;
		}

		while (lavaEruptTimer > 3)
		{
			SpawnLavaPlumes();
			lavaEruptTimer -= 3;
		}
	}

	// Spawns the anticipation projectiles for the lava plumes
	private void SpawnLavaPlumes()
	{
		Point loc = Player.Center.ToTileCoordinates();
		Tile tile = GetRandomPosition(ref loc);
		int retries = 0;

		while (tile.HasTile || tile.LiquidType != LiquidID.Lava || tile.LiquidAmount < 10 || Collision.SolidCollision(loc.ToWorldCoordinates(0, -32), 16, 32) 
			|| Main.tile[loc.X, loc.Y - 1].LiquidAmount > 100)
		{
			tile = GetRandomPosition(ref loc);
			retries++;

			if (retries > 10000) // Quit if there's no valid tile nearby
			{
				return;
			}
		}

		var vel = new Vector2(0, Main.rand.NextFloat(2));
		IEntitySource source = Terraria.Entity.GetSource_NaturalSpawn();
		Projectile.NewProjectile(source, loc.ToWorldCoordinates(8, 0), Vector2.Zero, ModContent.ProjectileType<GeyserPredictionProjectile>(), 30, 0.1f, Main.myPlayer, 1);
	}

	// Spawns the ash blocks that fall from the ceiling
	private void SpawnFallingAsh()
	{
		Point loc = Player.Center.ToTileCoordinates();
		Tile tile = GetRandomPosition(ref loc);
		int retries = 0;

		while (!tile.HasTile || tile.TileType != TileID.Hellstone && tile.TileType != TileID.Ash || Collision.SolidCollision(loc.ToWorldCoordinates(0, 16), 16, 32))
		{
			tile = GetRandomPosition(ref loc);
			retries++;

			if (retries > 10000) // Quit if there's no valid tile nearby
			{
				return;
			}
		}

		var vel = new Vector2(0, Main.rand.NextFloat(2));
		IEntitySource source = Terraria.Entity.GetSource_NaturalSpawn();
		Projectile.NewProjectile(source, loc.ToWorldCoordinates(8, 24), vel, ModContent.ProjectileType<FallingAshBlock>(), 30, 0.1f, Main.myPlayer);

		for (int i = 0; i < 20; ++i)
		{
			Dust.NewDustPerfect(loc.ToWorldCoordinates(8, 16), DustID.Ash, new Vector2(0, Main.rand.NextFloat(1, 4)).RotatedByRandom(0.7f));
		}
	}

	private static Tile GetRandomPosition(ref Point loc)
	{
		loc = new Point(loc.X + Main.rand.Next(-120, 120), loc.Y + Main.rand.Next(-60, 60));
		loc.X = (int)MathHelper.Clamp(loc.X, 10, Main.maxTilesX - 10);
		loc.Y = (int)MathHelper.Clamp(loc.Y, 10, Main.maxTilesY - 10);
		Tile tile = Main.tile[loc];
		return tile;
	}
}