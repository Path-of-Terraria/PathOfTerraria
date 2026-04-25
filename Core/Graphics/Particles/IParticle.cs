namespace PathOfTerraria.Core.Graphics.Particles;

public interface IParticle
{
    /// <summary>
    ///     Invoked every tick to update the particle.
    /// </summary>
    void Update();
    
    /// <summary>
    ///     Invoked every tick to draw the particle.
    /// </summary>
    void Draw();
    
    /// <summary>
    ///     Invoked when the particle is created.
    /// </summary>
    void Create();

    /// <summary>
    ///     Invoked when the particle is destroyed.
    /// </summary>
    void Destroy();
}