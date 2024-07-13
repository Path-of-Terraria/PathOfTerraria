using PathOfTerraria.Content.GUI.UIItemSlots;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Armor.Elements;

public class UIArmorItemSlot : UICustomItemSlot
{
	public override Item Item
	{
		get => InventoryIndex == -1 ? item : Main.CurrentPlayer.armor[InventoryIndex];
		set
		{
			if (InventoryIndex == -1)
			{
				item = value;
			}
			else
			{
				Main.CurrentPlayer.armor[InventoryIndex] = value;
			}
		}
	}

	public int InventoryIndex = -1;
	
	public float ActiveScale { get; set; } = 1.15f;

	public float InactiveScale { get; set; } = 1f;

	public float ActiveRotation { get; set; } = MathHelper.ToRadians(2.5f);

	public float InactiveRotation { get; set; } = 0f;
	
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

		var rotation = IsMouseHovering ? ActiveRotation : InactiveRotation;

		Background.Rotation = MathHelper.SmoothStep(Background.Rotation, rotation, 0.3f);
		Icon.Rotation = MathHelper.SmoothStep(Icon.Rotation, rotation, 0.1f);
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