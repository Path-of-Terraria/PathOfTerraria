using Terraria.DataStructures;

namespace PathOfTerraria.Content.Gores;

internal abstract class BloodSplat : ModGore
{
	protected abstract int MaxTime { get; }

	public override void OnSpawn(Gore gore, IEntitySource source)
	{
		gore.sticky = false;
		gore.timeLeft = (int)(MaxTime * Main.rand.NextFloat(0.5f, 1.0f));
		gore.velocity = Vector2.Clamp(gore.velocity, Vector2.One * -1f, Vector2.One * 1f);
		gore.velocity += Main.rand.NextVector2Circular(1f, 1f);
	}

	public override bool Update(Gore gore)
	{
		gore.position += gore.velocity;
		int lastFrame = gore.Frame.RowCount - 1;
		int frameIndex = Math.Min(lastFrame - (gore.timeLeft * gore.Frame.RowCount / MaxTime), lastFrame);
		gore.Frame = gore.Frame.With(0, (byte)frameIndex);

		if (--gore.timeLeft <= 0)
		{
			gore.active = false;
		}

		return false;
	}
}

internal class BloodSplatSmall : BloodSplat
{
	protected override int MaxTime => 5 * 5;

	public override void OnSpawn(Gore gore, IEntitySource source)
	{
		base.OnSpawn(gore, source);
		gore.Frame = new SpriteFrame(1, 5) with { PaddingX = 0, PaddingY = 0 };
	}
}

internal class BloodSplatMedium : BloodSplat
{
	protected override int MaxTime => 7 * 6;

	public override void OnSpawn(Gore gore, IEntitySource source)
	{
		base.OnSpawn(gore, source);
		gore.Frame = new SpriteFrame(1, 7) with { PaddingX = 0, PaddingY = 0 };
	}
}

internal class BloodSplatLarge : BloodSplat
{
	protected override int MaxTime => 8 * 7;

	public override void OnSpawn(Gore gore, IEntitySource source)
	{
		base.OnSpawn(gore, source);
		gore.Frame = new SpriteFrame(1, 8) with { PaddingX = 0, PaddingY = 0 };
	}
}
