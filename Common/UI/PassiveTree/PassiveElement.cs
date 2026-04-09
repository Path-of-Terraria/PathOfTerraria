using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Passives;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.PassiveTree;

internal class PassiveElement : AllocatableElement
{
	public Passive Passive { get; }

	public PassiveElement(Passive passive) : base(passive)
	{
		float halfSizeX = passive.Size.X / 2;
		float halfSizeY = passive.Size.Y / 2;

		Passive = passive;
		Left.Set(passive.TreePos.X - halfSizeX, 0.5f);
		Top.Set(passive.TreePos.Y - halfSizeY, 0.5f);
		Width.Set(passive.Size.X, 0);
		Height.Set(passive.Size.Y, 0);

		if (Passive is AnchorPassive)
		{
			Passive.Level = Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>().IsAllowedAnchor(Passive) ? 1 : 0;
		}
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (Passive.CanAllocate(Main.LocalPlayer) && CheckMouseContained())
		{
			Allocate(Passive, usedCost: 1);
		}
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		if (Passive.CanDeallocate(Main.LocalPlayer) && CheckMouseContained())
		{
			Deallocate(Passive, usedCost: 1);
		}
	}

	protected override void Deallocate(Allocatable passive, int usedCost)
	{
		base.Deallocate(passive, usedCost);

		SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
	}
}
