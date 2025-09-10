using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer.Projectiles;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria.GameContent;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

/// <summary>
/// Handles batching <see cref="SunspotAura"/> draw calls to save a bit of performance.
/// </summary>
internal class FrozenNPCBatching : GlobalNPC
{
	public static bool Drawing { get; private set; }

	public static Queue<int> CachedNPCs = [];
	public static HashSet<int> CachedGore = [];

	private static Asset<Effect> FrozenEffect;
	private static RenderTarget2D FrozenTarget;

	public override void Load()
	{
		On_Main.DoDraw_WallsTilesNPCs += DrawFrozenNPCs;
		IL_Main.DrawGore += AddGoreDrawHook;

		FrozenEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/FrozenEffect");

		Main.RunOnMainThread(() =>
		{
			const RenderTargetUsage UsageType = RenderTargetUsage.PreserveContents;
			GraphicsDevice device = Main.instance.GraphicsDevice;
			FrozenTarget = new RenderTarget2D(device, Main.displayWidth.Max(), Main.displayHeight.Max(), false, SurfaceFormat.Color, DepthFormat.None, 1, UsageType);

			Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
			Main.graphics.ApplyChanges();
		});
	}

	private void AddGoreDrawHook(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(MoveType.After, x => x.MatchCall<Main>(nameof(Main.LoadGore))))
		{
			return;
		}

		ILLabel label = null;
		
		if (!c.TryGotoPrev(x => x.MatchBr(out label)))
		{
			return;
		}

		if (!c.TryGotoNext(MoveType.After, x => x.MatchCall<Main>(nameof(Main.LoadGore))))
		{
			return;
		}

		c.Emit(OpCodes.Ldloc_0);
		c.EmitDelegate(PreDrawGore);
		c.Emit(OpCodes.Brfalse, label);
	}

	public static bool PreDrawGore(int who)
	{
		return CachedGore.Contains(who) == Drawing;
	}

	private void DrawFrozenNPCs(On_Main.orig_DoDraw_WallsTilesNPCs orig, Main self)
	{
		DrawNPCs(true);
		orig(self);
	}

	private static void DrawNPCs(bool behindTiles)
	{
		if (CachedNPCs.Count <= 0 && CachedGore.Count <= 0)
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

		DrawFrozenGore();

		Main.spriteBatch.End();
		Drawing = false;
	}

	private static void DrawFrozenGore()
	{
		HashSet<int> removals = [];

		foreach (int i in CachedGore)
		{
			Gore gore = Main.gore[i];

			if (!gore.active || gore.type <= 0)
			{
				removals.Add(i);
				continue;
			}

			/*
			// I don't know what this code is for - this was copied from vanilla. I just commented it out because idk - Gabe
			if (((gore[i].type >= 706 && gore[i].type <= 717) || gore[i].type == 943 || gore[i].type == 1147 || (gore[i].type >= 1160 && gore[i].type <= 1162)) 
				&& (gore[i].frame < 7 || gore[i].frame > 9)) {
			*/
			//if (GoreID.Sets.DrawBehind[gore[i].type] || (GoreID.Sets.LiquidDroplet[gore[i].type] && gore[i].frame is < 7 or > 9))
			//{
			//	drawBackGore = true;
			//	continue;
			//}

			if (gore.Frame.ColumnCount > 1 || gore.Frame.RowCount > 1)
			{
				Asset<Texture2D> tex = TextureAssets.Gore[gore.type];
				Rectangle src = gore.Frame.GetSourceRectangle(tex.Value);
				Vector2 vector = new(0f, 0f);

				if (gore.type == 1217)
				{
					vector.Y += 4f;
				}

				vector += gore.drawOffset;
				Color alpha = gore.GetAlpha(Lighting.GetColor((int)(gore.position.X + src.Width * 0.5) / 16, (int)((gore.position.Y + src.Height * 0.5) / 16.0)));
				Vector2 pos = new Vector2(gore.position.X + (src.Width / 2), gore.position.Y + (src.Height / 2) - 2f) + vector - Main.screenPosition;
				Main.spriteBatch.Draw(tex.Value, pos, src, alpha, gore.rotation, new Vector2(src.Width / 2, src.Height / 2), gore.scale, SpriteEffects.None, 0f);
			}
			else
			{
				Asset<Texture2D> tex = TextureAssets.Gore[gore.type];
				Color alpha2 = gore.GetAlpha(Lighting.GetColor((int)(gore.position.X + tex.Width() * 0.5) / 16, (int)((gore.position.Y + tex.Height() * 0.5) / 16.0)));
				Vector2 pos = new Vector2(gore.position.X + (tex.Width() / 2), gore.position.Y + tex.Height() / 2) + gore.drawOffset - Main.screenPosition;
				Main.spriteBatch.Draw(tex.Value, pos, new Rectangle(0, 0, tex.Width(), tex.Height()), alpha2, gore.rotation, tex.Size() / 2f, gore.scale, SpriteEffects.None, 0f);
			}
		}

		foreach (int value in removals)
		{
			CachedGore.Remove(value);
		}
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
