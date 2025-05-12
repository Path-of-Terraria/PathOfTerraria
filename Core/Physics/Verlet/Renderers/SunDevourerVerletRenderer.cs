using Terraria.GameContent;

namespace PathOfTerraria.Core.Physics.Verlet;

/// <summary>
/// Will go unused, but kept for reference in case we use this system later.
/// </summary>
public readonly struct SunDevourerVerletRenderer : IVerletRenderer
{
	void IVerletRenderer.Render(VerletChain chain)
	{
		for (int i = 0; i < chain.Sticks.Count; i++)
		{
			VerletStick stick = chain.Sticks[i];
			Texture2D texture = TextureAssets.MagicPixel.Value;

			Vector2 start = stick.Start.Position - Main.screenPosition;
			Vector2 end = stick.End.Position - Main.screenPosition;
			Vector2 difference = end - start;
			float rotation = difference.ToRotation();

			// TODO: Better asset handling.
			switch (i)
			{
				case 0:
					texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/NPCs/Mapping/Desert/SunDevourer/SunDevourerNPC_Tail_4").Value;
					break;
				case >= 1 and < 4:
					texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/NPCs/Mapping/Desert/SunDevourer/SunDevourerNPC_Tail_3").Value;
					break;
				case >= 4 and < 6:
					texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/NPCs/Mapping/Desert/SunDevourer/SunDevourerNPC_Tail_2").Value;
					break;
				case >= 6 and < 8:
					texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/NPCs/Mapping/Desert/SunDevourer/SunDevourerNPC_Tail_1").Value;
					break;
				case 8:
					texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/NPCs/Mapping/Desert/SunDevourer/SunDevourerNPC_Tail_0").Value;
					break;
			}
			
			Main.spriteBatch.Draw
			(
				texture,
				new Rectangle((int)start.X, (int)start.Y, (int)difference.Length(), texture.Height),
				null, 
				Color.White, 
				rotation,
				texture.Size() / 2f,
				SpriteEffects.None,
				0f
			);
		}
	}
}