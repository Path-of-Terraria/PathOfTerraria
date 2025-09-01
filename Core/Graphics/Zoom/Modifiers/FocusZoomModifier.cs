using Terraria.Graphics;

namespace PathOfTerraria.Core.Graphics.Zoom.Modifiers;

public sealed class FocusZoomModifier : IZoomModifier
{
	public string Identifier { get; }

    /// <summary>
    ///     Gets the total duration of the modifier.
    /// </summary>
    public int TimeMax { get; }
    
    /// <summary>
    ///     Gets the remaining duration of the modifier.
    /// </summary>
    public int TimeLeft { get; private set; }

    public bool Finished { get; private set; }
    
    public FocusZoomModifier(string identifier, int timeLeft)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(identifier, nameof(identifier));
        
        Identifier = identifier;

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeLeft, nameof(timeLeft));
        
        TimeMax = timeLeft;
        TimeLeft = timeLeft;
    }

    public void Update(ref SpriteViewMatrix matrix)
    {
        if (TimeLeft <= 0)
        {
            Finished = true;
        }
        else
        {
	        TimeLeft--;
        
	        var progress = TimeLeft / (float)TimeMax;
        
	        var multiplier = 1f - progress; 
	        var fade = MathF.Sin(multiplier * MathF.PI); 

	        matrix.Zoom = new Vector2(1f + 1f * fade * fade * fade);
        }
    }
}