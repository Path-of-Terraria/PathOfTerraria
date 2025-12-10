namespace PathOfTerraria.Content.Dusts;

internal class SiphonDust : ModDust
{
	internal record struct SiphonData(byte Who, short Counter)
	{
		public readonly byte Who = Who;

		public short Counter = Counter;
	}

	internal static ref SiphonData PlayerIndex(Dust dust)
	{
		return ref System.Runtime.CompilerServices.Unsafe.Unbox<SiphonData>(dust.customData);
	}

	public static void Spawn(Vector2 position, int who, Vector2? velocity = null)
	{
		velocity ??= new Vector2(Main.rand.NextFloat(4, 8), 0).RotatedByRandom(MathHelper.TwoPi);
		Dust dust = Dust.NewDustPerfect(position, ModContent.DustType<SiphonDust>(), velocity.Value, Scale: Main.rand.NextFloat(1, 2));
		dust.customData = new SiphonData((byte)who, 0);
	}

	public override bool Update(Dust dust)
	{
		dust.position += dust.velocity;
		ref SiphonData data = ref PlayerIndex(dust);
		data.Counter++;

		Player player = Main.player[data.Who];

		if (player.dead || player.Hitbox.Contains(dust.position.ToPoint()))
		{
			dust.active = false;
			return false;
		}

		dust.velocity = Vector2.Lerp(dust.velocity, dust.position.DirectionTo(player.Center) * 16, MathF.Min(MathF.Pow(data.Counter * 0.006f, 2), 1));
		dust.alpha = (byte)MathHelper.Lerp(0, 255, MathHelper.Clamp(dust.position.Distance(player.Center) / 600f, 0, 1));

		if (dust.velocity.LengthSquared() > 16 * 16)
		{
			dust.velocity = Vector2.Normalize(dust.velocity) * 16;
		}

		Tile tile = Main.tile[dust.position.ToTileCoordinates()];

		if (tile.HasTile && Main.tileSolid[tile.TileType])
		{
			dust.active = false;
		}

		return false;
	}
}
