using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointBrowser : UIState
{
	/// <summary>
	///     The unique identifier of this state.
	/// </summary>
	public const string Identifier = $"{PoTMod.ModName}:{nameof(UIWaypointBrowser)}";

	/// <summary>
	///     The height of this element in pixels.
	/// </summary>
	public const float FullHeight = 400f;
	
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

		var root = new UIElement
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			Width = { Pixels = UIWaypointList.FullWidth + 32f },
			Height = { Pixels = FullHeight }
		};
		
		Append(root);

		var list = new UIWaypointList
		{
			VAlign = 0.5f
		};
		
		root.Append(list);
		
		var indicator = new UIHoverImage(
			ModContent.Request<Texture2D>(
				$"{PoTMod.ModName}/Assets/Waypoints/Indicator",
				AssetRequestMode.ImmediateLoad
			)
		)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			Color = Color.White * 0.8f,
			ActiveScale = 1.25f,
			Left = { Pixels = UIWaypointList.FullWidth + 4f }
		};

		indicator.OnMouseOver += (_, _) =>
		{
			SoundEngine.PlaySound(
				SoundID.MenuTick with
				{
					Pitch = 0.15f,
					MaxInstances = 1
				}
			);
		};

		indicator.OnMouseOut += (_, _) =>
		{
			SoundEngine.PlaySound(
				SoundID.MenuTick with
				{
					Pitch = -0.25f,
					MaxInstances = 1
				}
			);
		};

		indicator.OnLeftClick += (_, _) =>
		{
			root.Append(new UIWaypointPreview(TextureAssets.Item[ItemID.HermesBoots]));
		};
		
		indicator.OnUpdate += (_) =>
		{
			indicator.Top.Pixels = MathHelper.SmoothStep(
				indicator.Top.Pixels,
				50f + list.SelectedTab.Top.Pixels + 24f - 10f,
				0.3f
			);
		};
		
		root.Append(indicator);
	}

	public override void OnActivate()
	{
		base.OnActivate();
		
		// Temporary until animations are implemented.
		OnInitialize();

		TargetProgress = 1f;
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		
		// Temporary until animations are implemented.
		RemoveAllChildren();

		TargetProgress = 0f;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		Progress = MathHelper.SmoothStep(Progress, TargetProgress, 0.3f);
	}
}