using ReLogic.Content;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Elements;

/// <summary>
///		Provides a <see cref="UIItemSlot"/> with a localized hover tooltip and hover transform animations.
/// </summary>
public class UIDynamicItemSlot : UITooltipItemSlot
{
	private float smoothness = 0.3f;

	/// <summary>
	///		The smoothness used during interpolation.
	/// </summary>
	/// <remarks>
	///		Defaults to <c>0.3f</c>. Ranges from <c>0f</c> - <c>1f</c>.
	/// </remarks>
	public float Smoothness
	{
		get => smoothness;
		set => smoothness = MathHelper.Clamp(value, 0f, 1f);
	}

	/// <summary>
	///		The active scale of the element.
	/// </summary>
	/// <remarks>
	///		This will be used as the target scale when the mouse is hovering over the element.
	/// </remarks>
	public float ActiveScale = 1.15f;

	/// <summary>
	///		The inactive scale of the element.
	/// </summary>
	/// <remarks>
	///		This will be used as the target scale when the mouse is not hovering over the element.
	/// </remarks>
	public float InactiveScale = 1f;
	
	/// <summary>
	///		The current rotation of the element.
	/// </summary>
	public float Rotation;

	/// <summary>
	///		The active rotation of the element.
	/// </summary>
	/// <remarks>
	///		This will be used as the target rotation when the mouse is not hovering over the element.
	/// </remarks>
	public float ActiveRotation = MathHelper.ToRadians(2.5f);

	/// <summary>
	///		The inactive rotation of the element.
	/// </summary>
	/// <remarks>
	///		This will be used as the target rotation when the mouse is not hovering over the element.
	/// </remarks>
	public float InactiveRotation = 0f;
	
	public UIDynamicItemSlot(
		Asset<Texture2D> backgroundTexture,
		Asset<Texture2D> iconTexture,
		string key,
		int context = ItemSlot.Context.InventoryItem
	) : base(backgroundTexture, iconTexture, key, context) { }

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		Scale = MathHelper.SmoothStep(Scale, IsMouseHovering ? ActiveScale : InactiveScale, Smoothness);
		Rotation = MathHelper.SmoothStep(Rotation, IsMouseHovering ? ActiveRotation : InactiveRotation, Smoothness);

		Background.Rotation = Rotation;
		Icon.Rotation = Rotation;
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);

		SoundEngine.PlaySound(SoundID.MenuTick with
		{
			Pitch = 0.15f,
			MaxInstances = 1
		});
	}

	public override void MouseOut(UIMouseEvent evt)
	{
		base.MouseOut(evt);
		
		SoundEngine.PlaySound(SoundID.MenuTick with
		{
			Pitch = -0.25f,
			MaxInstances = 1
		});
	}
}