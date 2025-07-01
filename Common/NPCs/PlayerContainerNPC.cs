using Terraria.ID;

namespace PathOfTerraria.Common.NPCs;

internal abstract class PlayerContainerNPC : ModNPC
{
	internal readonly struct PlayerColors
	{
		public Color Skin { get; init; }
		public Color Eyes { get; init; }
		public Color Shoes { get; init; }
		public Color Shirt { get; init; }
		public Color Undershirt { get; init; }
		public Color Pants { get; init; }
		public Color Hair { get; init; }

		public readonly void SetColors(Player player)
		{
			player.skinColor = Skin;
			player.eyeColor = Eyes;
			player.hairColor = Hair;
			player.pantsColor = Pants;
			player.shirtColor = Shirt;
			player.underShirtColor = Undershirt;
			player.shoeColor = Shoes;
		}
	}
	
	// This has to be set to something near the size of the NPC because otherwise it'll give you a tiny clickbox. Who knows why!
	public override string Texture => "Terraria/Images/NPC_" + NPCID.Guide;

	internal Player DrawDummy = null;

	public override void SetDefaults()
	{
		Defaults();

		DrawDummy = new Player();
		InitializePlayer();
	}

	protected virtual void InitializePlayer() { }
	protected virtual void PreDrawPlayer() { }

	public virtual void Defaults() { }

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		DrawDummy.position = NPC.position;
		DrawDummy.velocity = NPC.velocity;
		DrawDummy.PlayerFrame();
		DrawDummy.direction = NPC.direction;
		DrawDummy.gfxOffY = NPC.gfxOffY;
		PreDrawPlayer();

		Main.spriteBatch.End();
		Main.PlayerRenderer.DrawPlayers(Main.Camera, [DrawDummy]);
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
			DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

		return false;
	}
}
