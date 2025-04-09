using System.Collections.Generic;

namespace PathOfTerraria.Core.Physics.Verlet;

public sealed class VerletChain(float friction = 0.99f, float gravity = 0.33f, int capacity = 0)
{
	/// <summary>
	///		Gets the points of the chain.
	/// </summary>
	public List<VerletPoint> Points { get; } = new(capacity);
	
	/// <summary>
	///		Gets the sticks of the chain.
	/// </summary>
	public List<VerletStick> Sticks { get; } = new(capacity);

    /// <summary>
    ///		Gets or sets the friction of the chain. 
    /// </summary>
    public float Friction { get; set; } = friction;

    /// <summary>
    ///		Gets or sets the gravity of the chain.
    /// </summary>
    public float Gravity { get; set; } = gravity;

    /// <summary>
    ///		Adds a point to the chain.
    /// </summary>
    /// <param name="point">The instance of the <see cref="VerletPoint"/> to add.</param>
	public void AddPoint(in VerletPoint point)
	{
		Points.Add(point);
	}

	/// <summary>
	///		Adds a stick to the chain.
	/// </summary>
	/// <param name="stick">The instance of the <see cref="VerletStick"/> to add.</param>
	public void AddStick(in VerletStick stick)
	{
		Sticks.Add(stick);
	}
	
	/// <summary>
	///		Updates the points and sticks of the chain.
	/// </summary>
    public void Update()
    {
	    UpdatePoints();
	    UpdateSticks();
    }

	/// <summary>
	///		Renders the chain.
	/// </summary>
	/// <param name="renderer">The instance of the renderer to use.</param>
	/// <typeparam name="T">The type of the renderer to use.</typeparam>
	public void Render<T>(T renderer) where T : IVerletRenderer
	{
		renderer.Render(this);
	}

    private void UpdatePoints()
    {
        for (int i = 0; i < Points.Count; i++)
        {
            var point = Points[i];
            
            var distance = point.Position - point.OldPosition;
            var velocity = distance * Friction;
            
            point.OldPosition = point.Position;
            
            if (point.Pinned)
            {
	            continue;
            }
            
            point.Velocity = velocity;
	            
            point.Position += point.Velocity;
            point.Position.Y += Gravity;
        }
    }

    private void UpdateSticks()
    {
        for (int i = 0; i < Sticks.Count; i++)
        {
            var stick = Sticks[i];

            var distance = stick.End.Position - stick.Start.Position;
            var length = MathF.Sqrt(distance.X * distance.X + distance.Y * distance.Y);

            var difference = stick.Length - length;
            var multiplier = difference / length / 2f;

            var offset = distance * multiplier;

            if (!stick.Start.Pinned)
            {
	            stick.Start.Position -= offset;
            }

            if (!stick.End.Pinned)
            {
	            stick.End.Position += offset;
            }
        }
    }
}