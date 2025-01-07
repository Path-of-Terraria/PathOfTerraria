using PathOfTerraria.Common.Systems.SurfaceBGModifications;

namespace PathOfTerraria.Content.Scenes;

internal class RavencrestSurfaceBackground : ModSurfaceBackgroundStyle, IBackgroundModifier
{
	public override int ChooseFarTexture()
	{
		return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/RavencrestFar");
	}

	public override int ChooseMiddleTexture()
	{
		return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/RavencrestMiddle");
	}

	public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
	{
		b -= 1550;
		return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/RavencrestFront");
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
		y -= isFar ? 1100 : 1150;
	}
}
