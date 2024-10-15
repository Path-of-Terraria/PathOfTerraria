using SubworldLibrary;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Subworlds.BossDomains.DeerDomain;

internal class PlayerInsanityLayer : PlayerDrawLayer
{
	public override Position GetDefaultPosition()
	{
		return new BeforeParent(PlayerDrawLayers.Head);
	}

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		if (SubworldSystem.Current is not DeerclopsDomain || drawInfo.headOnlyRender || drawInfo.shadow > 0)
		{
			return;
		}

		Texture2D tex = Mod.Assets.Request<Texture2D>("Assets/Misc/VFX/InsanityMarker").Value;
		float insanity = drawInfo.drawPlayer.GetModPlayer<DeerclopsDomainPlayer>().Insanity;
		Color color = Color.White * insanity;
		float scale = 1f;
		
		if (insanity > 0.9f)
		{
			float remainder = MathF.Pow((insanity - 0.9f) * 100, 2);
			scale *= 1 + remainder / 200;
		}

		Vector2 position = drawInfo.Center - Main.screenPosition - new Vector2(0, 20 * (2 - scale));

		for (int i = 0; i < 3; ++i)
		{
			Vector2 off = Main.rand.NextVector2Circular(3 * insanity, 3 * insanity);
			drawInfo.DrawDataCache.Add(new DrawData(tex, position + off, new Rectangle(0, 20 * i, 36, 18), color, 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0));
		}
	}
}
