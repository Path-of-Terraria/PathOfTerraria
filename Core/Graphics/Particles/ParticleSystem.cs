using System.Collections.Generic;

namespace PathOfTerraria.Core.Graphics.Particles;

[Autoload(Side = ModSide.Client)]
public sealed class ParticleSystem : ModSystem
{
    private static List<IParticle> particles = [];
    private static List<IParticle> particlesAddtive = [];

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

        particlesAddtive?.Clear();
        particlesAddtive = null;
    }

    public override void PostUpdateWorld()
    {
        base.PostUpdateWorld();
        
        for (var i = 0; i < particles.Count; i++)
        {
            particles[i].Update();
        }

        for (var i = 0; i < particlesAddtive.Count; i++)
        {
            particlesAddtive[i].Update();
        }
    }

    public static T Create<T>(T particle) where T : IParticle
    {
		if (particle.IsBlendstateAddtive())
		{
			particlesAddtive.Add(particle);
		}
		else 
		{
			particles.Add(particle);
		}

		particle.Create();

        return particle;
    }

    public static bool Destroy<T>(T particle) where T : IParticle
    {
		if (particle.IsBlendstateAddtive())
		{
			if (particlesAddtive.Remove(particle))
			{
				particle.Destroy();
			}
		}
		else
		{
			if (particles.Remove(particle))
			{
				particle.Destroy();
			}
		}

        return false;
    }

    public static IEnumerable<IParticle> Enumerate()
    {
        return particles;
    }

    public static IEnumerable<IParticle> EnumerateAddtive()
    {
        return particlesAddtive;
    }

    private static void DrawParticles()
    {
        for (var i = 0; i < particles.Count; i++)
        {
            particles[i].Draw();
        }

    }
    
	private static void DrawParticlesAddtive() 
	{
		for (var i = 0; i < particlesAddtive.Count; i++)
		{
			particlesAddtive[i].Draw();
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