# Path of Terraria

### Getting Started
1) Terraria is not included as a nuget package so we need to use the included tModLoader.targets file for pulling in the correct dependencies
    2) In the PathOfTerraria.csproj we see this line: `<Import Project="..\tModLoader.targets" />` - I have this setup so you can use this project outisde of the steam tModLoader folder. So the easiest way to get this to the place it's expecting (1 level up) simply copy the `tmodloader.targets` file from your steam tModLoader folder to the folder right above the root of this project.
3) We use the [Subworld Library Community Fork](https://github.com/Path-of-Terraria/SubworldLibraryCommunityFork) for subworlds.
   - Its assembly, tModLoader dependency ID, and public C# namespace are `SubworldLibraryCommunityFork`.
   - The checked-in `lib/SubworldLibraryCommunityFork.dll` and `lib/SubworldLibraryCommunityFork.xml` are built from the community fork.
   - Until the community Workshop item is published, build or install the community fork locally before launching Path of Terraria.
   - Do not use the original Workshop dependency when testing fork-specific fixes; it does not contain them.
   4) **In Visual Studio** right click on "Dependencies" (NOT the same folder as where the .dll is, it's a non-existent folder that appears in the solution explorer) and click "Add Project Reference" and select the .dll file
   5) **In Rider** right click on "Dependencies"  (NOT the same folder as where the .dll is, it's a non-existent folder that appears in the solution explorer) and click "Reference..." and select the .dll file
4) You should now be ready to launch the project. Simply hit play in your IDE and it should launch Terraria with the mod loaded.


### Additional Tooling
There are several mods that can help us do local development. I recommend the following:
1) Dragonlens - https://steamcommunity.com/sharedfiles/filedetails/?id=2939737748
2) Structurehelper - https://steamcommunity.com/sharedfiles/filedetails/?id=2790924965


### Project Notes
* All images are stored in the /Assets folder for consolidation. They are not in the same folder as the .cs files as tmodloader example mod has.