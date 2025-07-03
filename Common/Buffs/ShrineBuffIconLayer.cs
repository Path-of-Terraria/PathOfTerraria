using PathOfTerraria.Content.Buffs.ShrineBuffs;
using PathOfTerraria.Content.Projectiles.Utility;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Buffs;

internal class ShrineBuffIconLayer : PlayerDrawLayer
{
	public override Position GetDefaultPosition()
	{
		return new BeforeParent(PlayerDrawLayers.Head);
	}

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		if (drawInfo.shadow != 0 || Main.gameMenu)
		{
			return;
		}

		Player player = drawInfo.drawPlayer;
		List<Texture2D> textures = [];

		for (int i = 0; i < player.buffType.Length; ++i)
		{
			if (ModContent.GetModBuff(player.buffType[i]) is ShrineBuff buff)
			{
				textures.Add(ShrineAoE.MapIconsByType[buff.AoEType].Value);
			}
		}

		if (textures.Count == 0)
		{
			return;
		}

		for (int i = 0; i < textures.Count; ++i)
		{
			Texture2D tex = textures[i];
			Vector2 position = drawInfo.Center - Main.screenPosition - new Vector2(0, player.height + textures.Count * 3);

			if (textures.Count > 1)
			{
				position += new Vector2(10 + 4 * textures.Count, 0).RotatedBy(i / (float)textures.Count * MathHelper.TwoPi + Main.GameUpdateCount * 0.02f);
			}
			
			drawInfo.DrawDataCache.Add(new DrawData(tex, position.Floor(), null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0));
		}
	}
}
