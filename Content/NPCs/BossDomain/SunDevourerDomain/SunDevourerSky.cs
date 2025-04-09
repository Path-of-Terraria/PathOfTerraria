using ReLogic.Content;
using Terraria.GameContent;
using Terraria.Graphics.Effects;

namespace PathOfTerraria.Content.NPCs.BossDomain.SunDevourerDomain;

public sealed class SunDevourerSky : CustomSky
{
 /// <summary>
    ///     The effect used to overlay the screen with a color.
    /// </summary>
    public static readonly Asset<Effect> OverlayShader = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/Overlay", AssetRequestMode.ImmediateLoad);

    /// <summary>
    ///     Gets or sets the intensity of the overlay effect.
    /// </summary>
    public float Intensity
    {
        get => _intensity;
        private set => _intensity = MathHelper.Clamp(value, 0f, 1f);
    }

    private float _intensity;
    
    /// <summary>
    ///         Gets or sets whether the overlay effect is active.
    /// </summary>
    public bool Active { get; private set; }
    
    public override void Activate(Vector2 position, params object[] args)
    {
        Active = true;
    }

    public override void Deactivate(params object[] args)
    {
        Active = false;
    }

    public override void Reset()
    {
        Active = false;
    }
    
    public override bool IsActive()
    {
        return Active;
    }

    public override void Update(GameTime gameTime)
    {
        Intensity = MathHelper.SmoothStep(Intensity, Active ? 1f : 0f, 0.2f);
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        if (maxDepth < 0f || minDepth > 0f)
        {
            return;
        }

        var shader = OverlayShader.Value;
        
        spriteBatch.End();
        spriteBatch.Begin
        (
            SpriteSortMode.Deferred,
            BlendState.NonPremultiplied,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            shader,
            Main.GameViewMatrix.TransformationMatrix
        );
        
        shader.Parameters["uIntensity"].SetValue(1f);
        shader.Parameters["uOpacity"].SetValue(0.75f * Intensity);
        
        shader.Parameters["uTopColor"].SetValue(SunDevourerParticle.Red.ToVector3());
        shader.Parameters["uBottomColor"].SetValue(SunDevourerParticle.Orange.ToVector3());
        
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * 0.1f);
        
        spriteBatch.End();
        spriteBatch.Begin
        (
            SpriteSortMode.Deferred,
            BlendState.NonPremultiplied,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            default,
            Main.GameViewMatrix.TransformationMatrix
        );
    }
}