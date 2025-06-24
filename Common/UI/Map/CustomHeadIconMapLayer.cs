using PathOfTerraria.Common.NPCs;
using ReLogic.Content;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Map;

namespace PathOfTerraria.Common.UI.Map;

internal class CustomHeadIconMapLayer : ModMapLayer, INeedRenderTargetContent
{
	private static Asset<Texture2D> _emptyTex = null;

	private readonly PlayerHeadDrawRenderTargetContent[] _containerHeadRenderers = new PlayerHeadDrawRenderTargetContent[Main.maxNPCs];

	public bool IsReady => true;

	private readonly List<DrawData> _drawData = [];

	private bool _anyDirty = false;

	public override void Load()
	{
		for (int i = 0; i < _containerHeadRenderers.Length; i++)
		{
			_containerHeadRenderers[i] = new PlayerHeadDrawRenderTargetContent();
		}

		Main.ContentThatNeedsRenderTargets.Add(this);

		_emptyTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/EmptyHeadSize");
	}

	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.ModNPC is PlayerContainerNPC cNPC)
			{
				_drawData.Clear();

				Vector2 mapPosition = GetMapPosition(ref context);
				Vector2 position = (npc.Center.ToTileCoordinates().ToVector2() - mapPosition) * GetMapScale(ref context) + GetMapOffset(ref context);
				Rectangle? clippingRect = GetClippingRectangle(ref context);

				if (clippingRect.HasValue)
				{
					Rectangle rect = clippingRect.Value;
					rect.Inflate(-16, -16);

					if (!rect.Contains(position.ToPoint()))
					{
						continue;
					}
				}

				PlayerHeadDrawRenderTargetContent playerHeadDrawRenderTargetContent = _containerHeadRenderers[npc.whoAmI];
				playerHeadDrawRenderTargetContent.UsePlayer(cNPC.DrawDummy);
				playerHeadDrawRenderTargetContent.UseColor(Color.White);
				playerHeadDrawRenderTargetContent.Request();

				_anyDirty = true;

				if (playerHeadDrawRenderTargetContent.IsReady)
				{
					RenderTarget2D target = playerHeadDrawRenderTargetContent.GetTarget();
					_drawData.Add(new DrawData(target, position, null, Color.White, 0f, target.Size() / 2f, GetDrawScale(ref context), SpriteEffects.None));
					RenderDrawData(cNPC.DrawDummy);
				}

				if (context.Draw(_emptyTex.Value, npc.Center.ToTileCoordinates().ToVector2(), Terraria.UI.Alignment.Center).IsMouseOver)
				{
					Main.instance.MouseText(npc.FullName);
				}
			}
		}
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mapPosition")]
	static extern ref Vector2 GetMapPosition(ref MapOverlayDrawContext context);

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mapOffset")]
	static extern ref Vector2 GetMapOffset(ref MapOverlayDrawContext context);

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mapScale")]
	static extern ref float GetMapScale(ref MapOverlayDrawContext context);

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_drawScale")]
	static extern ref float GetDrawScale(ref MapOverlayDrawContext context);

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_clippingRect")]
	static extern ref Rectangle? GetClippingRectangle(ref MapOverlayDrawContext context);

	private void RenderDrawData(Player drawPlayer)
	{
		Effect pixelShader = Main.pixelShader;
		SpriteBatch spriteBatch = Main.spriteBatch;

		for (int i = 0; i < _drawData.Count; i++)
		{
			DrawData cdd = _drawData[i];

			if (!cdd.sourceRect.HasValue)
			{
				cdd.sourceRect = cdd.texture.Frame();
			}

			PlayerDrawHelper.SetShaderForData(drawPlayer, drawPlayer.cHead, ref cdd);

			if (cdd.texture != null)
			{
				cdd.Draw(spriteBatch);
			}
		}

		pixelShader.CurrentTechnique.Passes[0].Apply();
	}

	public void PrepareRenderTarget(GraphicsDevice device, SpriteBatch spriteBatch)
	{
		if (_anyDirty)
		{
			for (int i = 0; i < _containerHeadRenderers.Length; i++)
			{
				_containerHeadRenderers[i].PrepareRenderTarget(device, spriteBatch);
			}

			_anyDirty = false;
		}
	}

	public void Reset()
	{
		_anyDirty = false;
		_drawData.Clear();

		for (int i = 0; i < _containerHeadRenderers.Length; i++)
		{
			_containerHeadRenderers[i].Reset();
		}
	}
}
