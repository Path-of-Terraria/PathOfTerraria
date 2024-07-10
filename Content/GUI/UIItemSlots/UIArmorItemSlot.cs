using ReLogic.Content;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.UIItemSlots;

public class UIArmorItemSlot : UICustomItemSlot
{
	public const float ActiveScale = 1.2f;

	public const float InactiveScale = 1f;

	public const float ActiveRotation = MathHelper.Pi / 8f;

	public const float InactiveRotation = 1f;
	
	public UIArmorItemSlot(
		Asset<Texture2D> background,
		Asset<Texture2D> icon,
		Color? backgroundColor = null,
		int context = ItemSlot.Context.InventoryItem,
		float scale = 1f
	) : base(background, icon, backgroundColor, context, scale) { }

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		var scale = IsMouseHovering ? ActiveScale : InactiveScale;

		Background.ImageScale = MathHelper.SmoothStep(Background.ImageScale, scale, 0.3f);
		Icon.ImageScale = MathHelper.SmoothStep(Background.ImageScale, scale, 0.1f);

		var rotation = IsMouseHovering ? MathHelper.ToRadians(5f) : 0f;

		Background.Rotation = MathHelper.SmoothStep(Background.Rotation, rotation, 0.3f);
		Icon.Rotation = MathHelper.SmoothStep(Icon.Rotation, rotation, 0.1f);
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);

		SoundEngine.PlaySound(SoundID.MenuTick with
		{
			Pitch = 0.1f,
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