using Terraria.GameContent;

namespace PathOfTerraria.Core.Physics.Verlet;

public readonly struct SunDevourerVerletRenderer : IVerletRenderer
{
	void IVerletRenderer.Render(VerletChain chain)
	{
		for (var i = 0; i < chain.Sticks.Count; i++)
		{
			var stick = chain.Sticks[i];
			
			var start = stick.Start.Position - Main.screenPosition;
			var end = stick.End.Position - Main.screenPosition;

			var difference = end - start;
			var rotation = difference.ToRotation();

			var texture = TextureAssets.MagicPixel.Value;

			// TODO: Better asset handling.
			switch (i)
			{
				case 0:
					texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/NPCs/Mapping/Desert/SunDevourerContent/SunDevourerNPC_Tail_4").Value;
					break;
				case >= 1 and < 4:
					texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/NPCs/Mapping/Desert/SunDevourerContent/SunDevourerNPC_Tail_3").Value;
					break;
				case >= 4 and < 6:
					texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/NPCs/Mapping/Desert/SunDevourerContent/SunDevourerNPC_Tail_2").Value;
					break;
				case >= 6 and < 8:
					texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/NPCs/Mapping/Desert/SunDevourerContent/SunDevourerNPC_Tail_1").Value;
					break;
				case 8:
					texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/NPCs/Mapping/Desert/SunDevourerContent/SunDevourerNPC_Tail_0").Value;
					break;
			}
			
			Main.spriteBatch.Draw
			(
				texture,
				new Rectangle((int)start.X, (int)start.Y, (int)difference.Length(), (int)texture.Height),
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