using ReLogic.Content;
using Terraria.GameContent;
using PathOfTerraria.Utilities.Xna;

namespace PathOfTerraria.Common.UI.PassiveTree;

internal class PassiveTreeInnerPanel : AllocatableInnerPanel
{
	private static readonly Asset<Effect> _backgroundShader = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/PassiveTreeBackground", AssetRequestMode.ImmediateLoad);

	public override string TabName => "PassiveTree";
	
	protected override bool EnableZoom => true;

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dims = GetDimensions();
		var rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

		Effect effect = _backgroundShader.Value;
		effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.05f);
		effect.Parameters["uOpacity"].SetValue(0.78f);
		effect.Parameters["uStarIntensity"].SetValue(0.16f);
		effect.Parameters["uNebulaIntensity"].SetValue(0.10f);
		effect.Parameters["uBaseTopColor"].SetValue(new Vector3(0.02f, 0.03f, 0.08f));
		effect.Parameters["uBaseBottomColor"].SetValue(new Vector3(0.01f, 0.01f, 0.03f));

		SpriteBatchArgs args = spriteBatch.GetArguments();
		var shaderArgs = args with { Effect = effect };

		spriteBatch.End();
		spriteBatch.Begin(shaderArgs);
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, Color.White);
		spriteBatch.End();
		spriteBatch.Begin(args);

		base.DrawSelf(spriteBatch);
	}
}
