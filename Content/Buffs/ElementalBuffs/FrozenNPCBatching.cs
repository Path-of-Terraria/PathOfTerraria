using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer.Projectiles;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
		On_Main.DoDraw_WallsTilesNPCs += DrawFrozenNPCs;

		FrozenEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/FrozenEffect");

		Main.QueueMainThreadAction(() =>
		{
			const RenderTargetUsage UsageType = RenderTargetUsage.PreserveContents;
			GraphicsDevice device = Main.instance.GraphicsDevice;
			FrozenTarget = new RenderTarget2D(device, Main.displayWidth.Max(), Main.displayHeight.Max(), false, SurfaceFormat.Color, DepthFormat.None, 1, UsageType);
		});

		Main.RunOnMainThread(() =>
			{
				Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
				Main.graphics.ApplyChanges();
			}
		);
	}

	private void DrawFrozenNPCs(On_Main.orig_DoDraw_WallsTilesNPCs orig, Main self)
	{
		DrawNPCs(true);
		orig(self);
	}

	private static void DrawNPCs(bool behindTiles)
	{
		if (CachedNPCs.Count <= 0)
		{
			return;
		}

		Drawing = true;

		RenderToTarget(behindTiles, out RenderTargetBinding[] targets, out Matrix trans, out Effect effect);

		Main.instance.GraphicsDevice.SetRenderTargets(targets);
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, trans);

		Main.spriteBatch.Draw(FrozenTarget, Vector2.Zero, Color.White);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, trans);
	}

	private static void RenderToTarget(bool behindTiles, out RenderTargetBinding[] targets, out Matrix trans, out Effect effect)
	{
		Main.spriteBatch.End();

		targets = Main.instance.GraphicsDevice.GetRenderTargets();
		ApplyToBindings(targets);

		trans = Main.GameViewMatrix.TransformationMatrix;
		effect = FrozenEffect.Value;
		effect.Parameters["scroller"].SetValue(Main.GameUpdateCount * 0.004f);

		Main.instance.GraphicsDevice.SetRenderTarget(FrozenTarget);
		Main.instance.GraphicsDevice.Clear(Color.Transparent);

		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, trans);

		while (CachedNPCs.Count > 0)
		{
			NPC npc = Main.npc[CachedNPCs.Dequeue()];
			Vector2 pos = npc.position;

			// The NPC "shakes" half a second before being unfrozen
			if (npc.FindBuffIndex(ModContent.BuffType<FreezeDebuff>()) is int index and not -1 && npc.buffTime[index] < 30)
			{
				npc.position += Main.rand.NextVector2CircularEdge(2, 2);
			}

			Main.instance.DrawNPCDirect(Main.spriteBatch, npc, behindTiles, Main.screenPosition);

			npc.position = pos;
		}

		Main.spriteBatch.End();
		Drawing = false;
	}

	/// <summary>
	/// Forces all targets to use the PreserveContents <see cref="RenderTargetUsage"/> so they're not cleared.
	/// </summary>
	public static void ApplyToBindings(RenderTargetBinding[] bindings)
	{
		foreach (RenderTargetBinding binding in bindings)
		{
			if (binding.RenderTarget is not RenderTarget2D rt)
			{
				continue;
			}

			SetRenderTargetUsage(rt, RenderTargetUsage.PreserveContents);
		}

		[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_RenderTargetUsage")]
		static extern void SetRenderTargetUsage(RenderTarget2D rt, RenderTargetUsage render);
	}
}
