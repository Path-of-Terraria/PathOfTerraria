namespace PathOfTerraria.Core.Graphics.Particles;

public abstract class ForegroundParticle : IParticle
{
	/// <summary>
	/// The path used to retrieve the particle's texture. This, by default, replaces "Content" with "Assets" - make sure to do the same when overriding.
	/// </summary>
    public virtual string Texture => GetType().FullName.Replace(".", "/").Replace("Content", "Assets");

    /// <summary>
    ///     Gets or sets the rotation of the particle, in radians.
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    ///     Gets or sets the scale of the particle, from 0 to 1.
    /// </summary>
    public float Scale
    {
        get => _scale;
        set => _scale = MathHelper.Clamp(value, 0f, 1f);
    }

    private float _scale = 1f;

    /// <summary>
    ///     Gets or sets the opacity of the particle, from 0 to 1.
    /// </summary>
    public float Opacity
    {
        get => _opacity;
        set => _opacity = MathHelper.Clamp(value, 0f, 1f);
    }

    private float _opacity = 1f;

    /// <summary>
    ///     The position of the screen from when the particle was created.
    /// </summary>
    public Vector2 Parallax;

    /// <summary>
    ///     The position of the particle, in world coordinates.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    ///     The velocity of the particle, in pixels per tick.
    /// </summary>
    public Vector2 Velocity;

    /// <summary>
    ///     The color of the particle.
    /// </summary>
    public Color Color = Color.White;

    public virtual void Update()
    {
        Position += Velocity;
    }

    public virtual void Draw()
    {
        var texture = ModContent.Request<Texture2D>(Texture).Value;

        var position = Position - Vector2.Lerp(Main.screenPosition, Main.screenPosition - 2f * (Parallax - Main.screenPosition), Scale);

        var origin = texture.Size() / 2f;

        var color = Color * Opacity;

        var scale = Scale * Main.GameViewMatrix.Zoom;

        Main.EntitySpriteDraw(texture, position, null, color, Rotation, origin, scale, SpriteEffects.None);
    }

    public virtual void Create()
    {
        Parallax = Main.screenPosition;
    }

    public virtual void Destroy()
    {
        ParticleSystem.Destroy(this);
    }
}