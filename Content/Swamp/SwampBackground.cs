using PathOfTerraria.Common.Systems.SurfaceBGModifications;

namespace PathOfTerraria.Content.Swamp;

internal class SwampBackground : ModSurfaceBackgroundStyle, IBackgroundModifier
{
	public override int ChooseFarTexture()
	{
		return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/SwampFar");
	}

	public override int ChooseMiddleTexture()
	{
		return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/SwampMiddle");
	}

	public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
	{
		b -= 1250;

		return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/SwampFront");
	}

	public override void ModifyFarFades(float[] fades, float transitionSpeed)
	{
		for (int i = 0; i < fades.Length; i++)
		{
			if (i == Slot)
			{
				fades[i] += transitionSpeed;

				if (fades[i] > 1f)
				{
					fades[i] = 1f;
				}
			}
			else
			{
				fades[i] -= transitionSpeed;

				if (fades[i] < 0f)
				{
					fades[i] = 0f;
				}
			}
		}
	}

	public void ModifyPosition(bool isFar, ref int y)
	{
		y -= isFar ? 200 : 1100;
	}
}