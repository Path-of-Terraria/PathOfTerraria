using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Gores;

[Autoload(false)]
internal class AdvancedGore : ModGore
{
	public override bool Update(Gore gore)
	{
		int tick = gore.timeLeft + (gore.alpha / 2) + (gore.GetHashCode() * 33);
		if (tick % 150 == 0 || tick % 340 == 0)
		{
			int type = tick % 3 == 0 ? ModContent.GoreType<BloodSplatMedium>() : ModContent.GoreType<BloodSplatSmall>();
			Vector2 halfSize = Size(gore) * 0.5f;
			Vector2 pos = gore.position + halfSize + Main.rand.NextVector2Circular(halfSize.X + 5f, halfSize.Y + 5f);
			var blood = Gore.NewGorePerfect(null, pos, gore.velocity, type);
			blood.position -= Size(blood) * 0.5f;

			//SoundEngine.PlaySound(SoundID.ShimmerWeak1 with { Volume = 0.05f, Pitch = 0.7f, PitchVariance = 0.2f, MaxInstances = 3 }, gore.position);
		}

		return true;
	}

	private static Vector2 Size(Gore gore)
	{
		return gore.Frame.GetSourceRectangle(TextureAssets.Gore[gore.type].Value).Size();
	}
}
