using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Content.Projectiles.Hostile;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.HellEvent;

internal class HellEventPlayer : ModPlayer
{
	private float tileFallTimer = 0;
	private float lavaEruptTimer = 0;

	public override void PreUpdate()
	{
		// Add screenshake for "earthquake" effect

		if (HellEventSystem.EventOccuring && Player.ZoneUnderworldHeight)
		{
			Vector2 rotation = (Main.rand.NextFloat() * (MathHelper.Pi * 2f)).ToRotationVector2();
			PunchCameraModifier modifier = new(Player.Center, rotation, 1.5f * HellEventSystem.EventStrength, 10f * HellEventSystem.EventStrength, 2, 4000, "SkeletronRitual");
			Main.instance.CameraModifiers.Add(modifier);

			UpdateTiles();

			if (ModContent.GetInstance<WoFQuest>().CanBeStarted)
			{
				Player.GetModPlayer<QuestModPlayer>().StartQuest($"{PoTMod.ModName}/{nameof(WoFQuest)}");
			}
		}
	}

	private void UpdateTiles()
	{
		tileFallTimer += 2.5f / Main.CurrentFrameFlags.ActivePlayersCount;
		lavaEruptTimer += 1.2f / Main.CurrentFrameFlags.ActivePlayersCount;

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

	private void SpawnLavaPlumes()
	{
		Point loc = Player.Center.ToTileCoordinates();
		Tile tile = GetRandomPosition(ref loc);

		while (tile.HasTile || tile.LiquidType != LiquidID.Lava || tile.LiquidAmount < 10 || Collision.SolidCollision(loc.ToWorldCoordinates(0, -32), 16, 32) 
			|| Main.tile[loc.X, loc.Y - 1].LiquidAmount > 0)
		{
			tile = GetRandomPosition(ref loc);
		}

		var vel = new Vector2(0, Main.rand.NextFloat(2));
		IEntitySource source = Terraria.Entity.GetSource_NaturalSpawn();
		Projectile.NewProjectile(source, loc.ToWorldCoordinates(8, 0), Vector2.Zero, ModContent.ProjectileType<GeyserPredictionProjectile>(), 30, 0.1f, Main.myPlayer, 1);
	}

	private void SpawnFallingAsh()
	{
		Point loc = Player.Center.ToTileCoordinates();
		Tile tile = GetRandomPosition(ref loc);

		while (!tile.HasTile || tile.TileType != TileID.Hellstone && tile.TileType != TileID.Ash || Collision.SolidCollision(loc.ToWorldCoordinates(0, 16), 16, 32))
		{
			tile = GetRandomPosition(ref loc);
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