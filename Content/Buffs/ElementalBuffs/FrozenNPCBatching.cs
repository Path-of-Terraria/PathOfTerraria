using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer.Projectiles;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

/// <summary>
/// Handles batching <see cref="SunspotAura"/> draw calls to save a bit of performance.
/// </summary>
internal class FrozenNPCBatching : GlobalNPC
{
	public static bool Drawing { get; private set; }

	public static Queue<int> CachedNPCs = [];

	private static Asset<Effect> FrozenEffect;
	private static RenderTarget2D FrozenTarget;

	public override void Load()
	{
		//On_Main.DrawNPCs += DrawFrozenNPCs;
		On_Main.CheckMonoliths += DrawCachedNPCs;
		On_Main.DoDraw_WallsTilesNPCs += eg;

		FrozenEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/FrozenEffect");

		Main.QueueMainThreadAction(() =>
		{
			const RenderTargetUsage UsageType = RenderTargetUsage.PreserveContents;
			GraphicsDevice device = Main.instance.GraphicsDevice;
			FrozenTarget = new RenderTarget2D(device, Main.displayWidth.Max(), Main.displayHeight.Max(), false, SurfaceFormat.Color, DepthFormat.None, 1, UsageType);
		});
	}

	private void eg(On_Main.orig_DoDraw_WallsTilesNPCs orig, Main self)
	{
		orig(self);

		DrawNPCs(true);
	}

	private void DrawCachedNPCs(On_Main.orig_CheckMonoliths orig)
	{
		//DrawNPCs(false);
		orig();

	}

	private static void DrawNPCs(bool behindTiles)
	{
		if (CachedNPCs.Count <= 0)
		{
			return;
		}
		Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

		Drawing = true;
		Main.spriteBatch.End();

		RenderTargetBinding[] targets = Main.instance.GraphicsDevice.GetRenderTargets();

		Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

		Matrix trans = Main.GameViewMatrix.TransformationMatrix;
		Effect effect = FrozenEffect.Value;
		effect.Parameters["scroller"].SetValue(Main.GameUpdateCount * 0.01f);

		Main.instance.GraphicsDevice.SetRenderTarget(FrozenTarget);
		Main.instance.GraphicsDevice.Clear(Color.Transparent);

		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, trans);

		while (CachedNPCs.Count > 0)
		{
			Main.instance.DrawNPCDirect(Main.spriteBatch, Main.npc[CachedNPCs.Dequeue()], behindTiles, Main.screenPosition);
		}

		Main.spriteBatch.End();
		Drawing = false;

		Main.instance.GraphicsDevice.SetRenderTargets(targets);
		Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.DiscardContents;

		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, trans);

		Main.spriteBatch.Draw(FrozenTarget, Vector2.Zero, Color.White);

		//Main.spriteBatch.End();
	}
}
