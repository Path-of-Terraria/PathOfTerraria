using System.Collections.Generic;

namespace PathOfTerraria.Core.Graphics.Particles;

[Autoload(Side = ModSide.Client)]
public sealed class ParticleSystem : ModSystem
{
    private static List<IParticle> particlesAddtiveCache = [];
    private static List<IParticle> particlesAlphaBlendCache = [];

    public override void Load()
    {
        base.Load();
        
        On_Main.DrawDust += Main_DrawDust_DrawParticles;
    }

    public override void Unload()
    {
        base.Unload();
        
        particlesAlphaBlendCache?.Clear();
        particlesAlphaBlendCache = null;

        particlesAddtiveCache?.Clear();
		particlesAddtiveCache = null;
    }

    public override void PostUpdateWorld()
    {
        base.PostUpdateWorld();

		for (var i = 0; i < particlesAlphaBlendCache.Count; i++)
		{
			particlesAlphaBlendCache[i].Update();
		}

		for (var i = 0; i < particlesAddtiveCache.Count; i++)
		{
			particlesAddtiveCache[i].Update();
		}
	}

    public static T Create<T>(T particle) where T : IParticle
    {
		if (particle.IsBlendStateAddtive)
		{
			particlesAddtiveCache.Add(particle);
		}
		else 
		{
			particlesAlphaBlendCache.Add(particle);
		}

		particle.Create();

        return particle;
    }

    public static bool Destroy<T>(T particle) where T : IParticle
    {
		if (particle.IsBlendStateAddtive)
		{
			if(particlesAddtiveCache.Remove(particle)) 
			{
				particle.Destroy();
			}
		}
		else
		{
			if (particlesAlphaBlendCache.Remove(particle))
			{
				particle.Destroy();
			}
		}
		return false;
    }

    public static IEnumerable<IParticle> Enumerate(bool addtiveBlendStateParticles = false)
    {
        return addtiveBlendStateParticles ? particlesAddtiveCache : particlesAlphaBlendCache;
    }
    private static void DrawParticlesAlphaBlend()
    {
        for (var i = 0; i < particlesAlphaBlendCache.Count; i++)
        {
			particlesAlphaBlendCache[i].Draw();
        }
    }
    
	private static void DrawParticlesAddtive() 
	{
		for (var i = 0; i < particlesAddtiveCache.Count; i++)
		{
			particlesAddtiveCache[i].Draw();
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
        
        DrawParticlesAlphaBlend();
        
        Main.spriteBatch.End();
        
        Main.spriteBatch.Begin
        (
            SpriteSortMode.Texture,
            BlendState.Additive,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            default,
            Main.GameViewMatrix.TransformationMatrix
        );
        
        DrawParticlesAddtive();
        
        Main.spriteBatch.End();
        
        orig(self);
    }
}