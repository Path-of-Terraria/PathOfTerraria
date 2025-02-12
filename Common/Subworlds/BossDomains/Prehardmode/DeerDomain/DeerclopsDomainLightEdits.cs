using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.DeerDomain;

internal class DeerclopsDomainLightEdits : ModSystem
{
	internal static float LightMultiplier = 0;

	public override void Load()
	{
		On_Lighting.AddLight_int_int_float_float_float += HijackAddLight;
		On_Lighting.AddLight_int_int_int_float += HideTorchLight; ;
		On_Player.ItemCheck += SoftenPlayerLight;
		On_Main.Update += ResetLightMul;
		On_Projectile.ProjLight += HideProjLight;
		On_Dust.UpdateDust += HideDustLight;
	}

	private void HideTorchLight(On_Lighting.orig_AddLight_int_int_int_float orig, int i, int j, int torchID, float lightAmount)
	{
		if (SubworldSystem.Current is DeerclopsDomain && j > DeerclopsDomain.Surface + 10)
		{
			lightAmount *= LightMultiplier;
		}

		orig(i, j, torchID, lightAmount);
	}

	private void HideDustLight(On_Dust.orig_UpdateDust orig)
	{
		LightMultiplier = 0f;
		orig();
		LightMultiplier = 0f;
	}

	private void HideProjLight(On_Projectile.orig_ProjLight orig, Projectile self)
	{
		LightMultiplier = 0f;
		orig(self);
		LightMultiplier = 0f;
	}

	private void ResetLightMul(On_Main.orig_Update orig, Main self, GameTime gameTime)
	{
		LightMultiplier = 0;
		orig(self, gameTime);
	}

	private void SoftenPlayerLight(On_Player.orig_ItemCheck orig, Player self)
	{
		LightMultiplier = 0.15f;
		orig(self);
		LightMultiplier = 0;
	}

	private void HijackAddLight(On_Lighting.orig_AddLight_int_int_float_float_float orig, int i, int j, float r, float g, float b)
	{
		if (SubworldSystem.Current is DeerclopsDomain && j > DeerclopsDomain.Surface + 10)
		{
			(r, g, b) = (r * LightMultiplier, g * LightMultiplier, b * LightMultiplier);
		}

		orig(i, j, r, g, b);
	}
}
