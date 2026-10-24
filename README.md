# Recipe Favorites — a Vintage Story mod

Bookmark your favorite handbook pages and find them again in one click through a dedicated **★ Favorites** tab.

> Client-side only. No need to install it on the server, and it works on any server you join.

## Features

- Press **Shift + F** on any handbook page (item, creature, food, block…) to add it to your favorites. Press it again to remove it.
- Favorited entries get a small **★** marker in the handbook list.
- A new **★ Favorites** tab is added to the handbook's left-hand tab list and shows only the pages you have favorited.
- Favorites are saved to disk per savegame, so they persist after closing the game.

### What about crafting grid recipes?

In Vintage Story, an item's crafting recipe is shown inside that item's handbook page. Favoriting the page therefore gives you quick access to its recipe from the Favorites tab.

## Installation

1. Download the latest `recipefavorites-x.x.x.zip` from the [Releases](../../releases) page (or from the ModDB page).
2. Put the zip file, without extracting it, into your Vintage Story `Mods` folder.
   - You can open it from the in-game Mods menu with **Open Mod Folder**.
   - It is usually located at `%appdata%\VintagestoryData\Mods` on Windows.
3. Start the game.

## Usage

| Action | How |
| --- | --- |
| Add or remove a favorite | Open a handbook page and press **Shift + F** |
| View all favorites | Click the **★ Favorites** tab in the handbook |

## Compatibility

- Vintage Story 1.22 or newer (built against .NET 10)
- Client-side only

Vintage Story updates can occasionally rename internal classes or methods. If the mod stops working after a game update, please open an issue.

## Building from source

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Create an environment variable named `VINTAGE_STORY` that points to your Vintage Story **installation** folder (the folder that contains `VintagestoryAPI.dll`).
3. In the project folder, run:
   ```
   dotnet build -c Release
   ```
4. The output is in `bin/Release/net10.0/`. The files you need are `RecipeFavorites.dll` and `modinfo.json`. Zip them together to install the mod as described above.

## Reporting problems

Please [open an issue](../../issues) and include:

- Your Vintage Story version and the mod version
- What you did and what you expected to happen
- The relevant lines from `%appdata%\VintagestoryData\Logs\client-main.txt`

## License

Released under the [MIT License](LICENSE).
