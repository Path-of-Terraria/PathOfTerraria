using ReLogic.Content;
using Terraria.GameContent;
using PathOfTerraria.Utilities.Xna;

namespace PathOfTerraria.Common.UI.PassiveTree;

internal class PassiveTreeInnerPanel : AllocatableInnerPanel
{
	private const float BackgroundOpacity = 0.78f;
	private const float StarIntensity = 0.16f;
	private const float NebulaIntensity = 0.10f;
	private static readonly Vector3 TopColor = new(0.02f, 0.03f, 0.08f);
	private static readonly Vector3 BottomColor = new(0.01f, 0.01f, 0.03f);

	private static Asset<Effect> BackgroundShader => ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/PassiveTreeBackground");

	public override string TabName => "PassiveTree";
	
	protected override bool EnableZoom => true;

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dims = GetDimensions();
		var rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

		Effect effect = BackgroundShader.Value;
		effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.05f);
		effect.Parameters["uOpacity"].SetValue(BackgroundOpacity);
		effect.Parameters["uStarIntensity"].SetValue(StarIntensity);
		effect.Parameters["uNebulaIntensity"].SetValue(NebulaIntensity);
		effect.Parameters["uBaseTopColor"].SetValue(TopColor);
		effect.Parameters["uBaseBottomColor"].SetValue(BottomColor);

		SpriteBatchArgs args = spriteBatch.GetArguments();
		using (spriteBatch.Override(args with { Effect = effect }))
		{
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, Color.White);
		}

		base.DrawSelf(spriteBatch);
	}
}
