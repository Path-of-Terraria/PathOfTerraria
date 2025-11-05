using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using ReLogic.Content;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class StarcallerMastery : Passive
{
	internal class StarcallerHeadLayer : PlayerDrawLayer
	{
		private static Asset<Texture2D> StarcallerHead = null;

		public override void Load()
		{
			StarcallerHead = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/Starcaller_Head");
		}

		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.FinchNest);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player player = drawInfo.drawPlayer;

			if (drawInfo.headOnlyRender || player.dead || Main.gameMenu)
			{
				return;
			}

			Vector2 bob = Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];
			Vector2 position = drawInfo.Center - Main.screenPosition - new Vector2(0, player.height / 2f - bob.Y);
			SpriteEffects effect = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			if (effect == SpriteEffects.FlipHorizontally)
			{
				position.X -= 12;
			}

			drawInfo.DrawDataCache.Add(new DrawData(StarcallerHead.Value, position.Floor(), null, Color.White, 0f, Vector2.Zero, 1f, effect, 0));
		}
	}
}
