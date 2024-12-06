using PathOfTerraria.Common.Systems.DisableBuilding;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace PathOfTerraria.Common.Subworlds.BossDomains.BoCDomain;

public class CrimsonMawPlayer : ModPlayer
{
	public int Time = 0;
	public int StopTime = 0;

	public override void Load()
	{
		Main.OnPostDraw += DrawBlack;
		On_Player.QuickMount += On_Player_QuickMount;
	}

	public override void Unload()
	{
		Main.OnPostDraw -= DrawBlack;
	}

	private void On_Player_QuickMount(On_Player.orig_QuickMount orig, Player self)
	{
		bool frozen = self.frozen;

		if (self.GetModPlayer<CrimsonMawPlayer>().Time > 0)
		{
			self.frozen = true;
		}

		orig(self);
		self.frozen = frozen;
	}

	private void DrawBlack(GameTime obj)
	{
		if (Main.gameMenu || !WorldGen.crimson)
		{
			return;
		}

		int time = Main.LocalPlayer.GetModPlayer<CrimsonMawPlayer>().Time;

		if (time <= 0)
		{
			return;
		}

		float opacity = 0;

		if (time < 120f)
		{
			opacity = 1 - time / 120f;
		}

		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
			Main.GameViewMatrix.TransformationMatrix);

		Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-20, -20, Main.ScreenSize.X + 40, Main.ScreenSize.Y + 40), Color.Lerp(Color.Red, Color.Black, opacity) * opacity);
		Main.spriteBatch.End();
	}

	public override bool CanUseItem(Item item)
	{
		return Time <= 0;
	}

	public override void ResetEffects()
	{
		Time--;
		StopTime--;

		if (Time > 0)
		{
			Player.GetModPlayer<StopBuildingPlayer>().ConstantStopBuilding = true;
		}
	}

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
	{
		if (Time > 0)
		{
			fullBright = false;
			r = g = b = a = 0;
		}
	}

	public override void PreUpdateMovement()
	{
		if (Time > 0 || StopTime > 0)
		{
			Player.velocity *= 0;
		}
	}
}