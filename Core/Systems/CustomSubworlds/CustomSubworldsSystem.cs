using SubworldLibrary;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.WorldBuilding;
using Terraria.GameContent.Generation;

namespace PathOfTerraria.Core.Systems.CustomSubworlds;
public class CustomSubworldsSystem : ModSystem
{
	private static FieldInfo _subworldsField = typeof(SubworldSystem).GetField("subworlds", BindingFlags.NonPublic | BindingFlags.Static);
	public static void EnterCustomSubworld(string name, int initialWidth = 2000, int initialHeight = 1000)
	{
		List<Subworld> subworlds = (List<Subworld>)_subworldsField.GetValue(null);
		Subworld world = subworlds.Find(s => s.Name == name);

		if (world is null)
		{
			subworlds.Add(new CustomSubworld(name, initialWidth, initialHeight, [ new Subworlds.Passes.FlatWorldPass() ]));
			world = subworlds.Find(s => s.Name == name);

			typeof(ModType).GetProperty("Mod").SetMethod.Invoke(world, new object[] { PathOfTerraria.Instance });
		}

		Console.WriteLine(world.Name + " | " + world.FullName);
		SubworldSystem.Enter(world.FullName);
	}
}
