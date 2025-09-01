using Terraria.Graphics.CameraModifiers;

namespace PathOfTerraria.Core.Graphics.Camera.Modifiers;

public sealed class FocusCameraModifier : ICameraModifier
{
	public string UniqueIdentity { get; } 

	/// <summary>
	///		Gets the position of the modifier, in world coordinates.
	/// </summary>
	public Func<Vector2> Position { get; }
	
	/// <summary>
	///		Gets the duration of the modifier, in ticks.
	/// </summary>
	public int Duration { get; }
	
	/// <summary>
	///		Gets or sets the timer of the modifier, in ticks.
	/// </summary>
	public int Timer { get; private set; }
	
	public bool Finished { get; private set; }
	
	public FocusCameraModifier(string identifier, int duration, Func<Vector2> position)
	{
		ArgumentNullException.ThrowIfNullOrEmpty(identifier, nameof(identifier));
		
		UniqueIdentity = identifier;
		
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(duration, nameof(duration));
		
		Duration = duration;
		Position = position;
	}

	public void Update(ref CameraInfo cameraPosition)
	{
		if (Timer >= Duration * 2)
		{
			Finished = true;
		}
		else
		{
			Timer++;

			var multiplier = Timer / (float)(Duration * 2f);
			var fade = MathF.Sin(multiplier * MathF.PI); 
			
			var center = Position.Invoke() - new Vector2(Main.screenWidth, Main.screenHeight) / 2f;

			cameraPosition.CameraPosition = Vector2.SmoothStep(cameraPosition.OriginalCameraPosition, center, fade);
		}
	}
}