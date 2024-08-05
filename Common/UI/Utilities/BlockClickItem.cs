using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameInput;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Utilities;

public class BlockClickItem : ModSystem
{
	// TODO: This is not blocking anything on the right. | or the open achievements button.

	public static bool Block;

	public override void Load()
	{
		IL_ItemSlot.OverrideLeftClick += IL_BlockIfBlockingRetTrue;
		IL_ItemSlot.MouseHover_ItemArray_int_int += IL_BlockIfBlocking;
		IL_UIElement.MouseOver += IL_BlockIfBlocking;

		PropertyInfo propertyInfo = typeof(PlayerInput).GetProperty("IgnoreMouseInterface",
			BindingFlags.Public |
			BindingFlags.Static);
		MethodInfo methodInfo = propertyInfo.GetGetMethod();

		MonoModHooks.Add(methodInfo, ForceIgnoreMouseInterface);
	}

	public override void UpdateUI(GameTime gameTime)
	{
		Block = false;
	}

	private void IL_BlockIfBlocking(ILContext il)
	{
		var c = new ILCursor(il);
		c.Emit(OpCodes.Ldsfld, typeof(BlockClickItem).GetField("Block"));

		ILLabel trueLabel = il.DefineLabel();
		c.Emit(OpCodes.Brfalse, trueLabel);

		c.Emit(OpCodes.Ret);
		c.MarkLabel(trueLabel);
	}

	private void IL_BlockIfBlockingRetTrue(ILContext il)
	{
		var c = new ILCursor(il);
		c.Emit(OpCodes.Ldsfld, typeof(BlockClickItem).GetField("Block"));

		ILLabel trueLabel = c.DefineLabel();
		c.Emit(OpCodes.Brfalse, trueLabel);

		c.Emit(OpCodes.Ldc_I4_1);
		c.Emit(OpCodes.Ret);

		c.MarkLabel(trueLabel);
	}

	private bool ForceIgnoreMouseInterface(Func<bool> action)
	{
		if (!Block)
		{
			return action();
		}

		return true;
	}
}