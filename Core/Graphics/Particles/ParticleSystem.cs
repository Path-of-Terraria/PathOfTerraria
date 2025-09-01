using System.Collections.Generic;

namespace PathOfTerraria.Core.Graphics.Particles;

[Autoload(Side = ModSide.Client)]
public sealed class ParticleSystem : ModSystem
{
    private static List<IParticle> particles = [];

    public override void Load()
    {
        base.Load();
        
        On_Main.DrawDust += Main_DrawDust_DrawParticles;
    }

    public override void Unload()
    {
        base.Unload();
        
        particles?.Clear();
        particles = null;
    }

    public override void PostUpdateWorld()
    {
        base.PostUpdateWorld();
        
        for (var i = 0; i < particles.Count; i++)
        {
            particles[i].Update();
        }
    }

    public static T Create<T>(T particle) where T : IParticle
    {
        particles.Add(particle);
        
        particle.Create();

        return particle;
    }

    public static bool Destroy<T>(T particle) where T : IParticle
    {
        if (particles.Remove(particle))
        {
            particle.Destroy();
        }

        return false;
    }

    public static IEnumerable<IParticle> Enumerate()
    {
        return particles;
    }

    private static void DrawParticles()
    {
        for (var i = 0; i < particles.Count; i++)
        {
            particles[i].Draw();
        }
    }
    
    private static void Main_DrawDust_DrawParticles(On_Main.orig_DrawDust orig, Main self)
    {
        Main.spriteBatch.Begin
        (
            SpriteSortMode.Texture,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            default,
            Main.GameViewMatrix.TransformationMatrix
        );
        
        DrawParticles();
        
        Main.spriteBatch.End();
        
        orig(self);
    }
}