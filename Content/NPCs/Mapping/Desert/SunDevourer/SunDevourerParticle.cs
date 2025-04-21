using PathOfTerraria.Core.Graphics.Particles;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

public sealed class SunDevourerParticle : ForegroundParticle
{
	/// <summary>
	///     The red color used for the particle.
	/// </summary>
	public static readonly Color Red = new(245, 78, 66, 0);

	/// <summary>
	///     The orange color used for the particle.
	/// </summary>
	public static readonly Color Orange = new(212, 153, 53, 0);

	public SunDevourerParticle(Vector2 position)
	{
		Position = position;
	}

	public override void Create()
	{
		base.Create();

		Scale = Main.rand.NextFloat(0.8f, 1.2f);
		Opacity = 0f;
	}

	public override void Update()
	{
		base.Update();

		Color = Color.Lerp(Orange, Red, Opacity);

		Velocity.Y -= 0.05f;
		Velocity.X += Main.windSpeedCurrent * 0.1f;

		Opacity += 0.01f;

		Rotation += Velocity.X * 0.1f;

		Scale -= 0.001f;

		if (Scale > 0f)
		{
			return;
		}

		Destroy();
	}
}