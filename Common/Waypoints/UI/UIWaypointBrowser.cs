using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointBrowser : UIState
{
	/// <summary>
	///     The unique identifier of this state.
	/// </summary>
	public const string Identifier = $"{PoTMod.ModName}:{nameof(UIWaypointBrowser)}";

	/// <summary>
	///     The animation progress of this state.
	/// </summary>
	/// <remarks>
	///     This ranges from <c>0f</c> (Inactive) - <c>1f</c> (Active).
	/// </remarks>
	public float Progress
	{
		get => _progress;
		set => _progress = MathHelper.Clamp(value, 0f, 1f);
	}
	
	private float _progress;

	/// <summary>
	///     The target value for the animation progress of this state.
	/// </summary>
	/// <remarks>
	///     This ranges from <c>0f</c> (Inactive) - <c>1f</c> (Active).
	/// </remarks>
	public float TargetProgress
	{
		get => _targetProgress;
		set => _targetProgress = MathHelper.Clamp(value, 0f, 1f);
	}

	private float _targetProgress;

	public override void OnInitialize()
	{
		base.OnInitialize();

		var list = new UIWaypointList
		{
			HAlign = 0.5f,
			VAlign = 0.5f
		};
		
		Append(list);
	}

	public override void OnActivate()
	{
		base.OnActivate();

		TargetProgress = 1f;
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();

		TargetProgress = 0f;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		Progress = MathHelper.SmoothStep(Progress, TargetProgress, 0.3f);
	}
}