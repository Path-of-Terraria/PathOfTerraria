using ReLogic.Content;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor.Elements;

public abstract class UIArmorPage : UIElement
{
	public static readonly Asset<Texture2D> HelmetIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Helmet", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> ChestIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Chest", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> LegsIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Legs", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> WeaponIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Weapon", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> OffhandIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Offhand", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> RingIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Ring", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> NecklaceIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Necklace", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> WingsIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Wings", AssetRequestMode.ImmediateLoad);

	protected static Player Player => Main.LocalPlayer;

	protected virtual void UpdateMouseOver(UIMouseEvent @event, UIElement element)
	{
		SoundEngine.PlaySound
		(
			SoundID.MenuTick with
			{
				Pitch = 0.15f,
				MaxInstances = 1
			}
		);
	}

	protected virtual void UpdateMouseOut(UIMouseEvent @event, UIElement element)
	{
		SoundEngine.PlaySound
		(
			SoundID.MenuTick with
			{
				Pitch = -0.25f,
				MaxInstances = 1
			}
		);
	}
}