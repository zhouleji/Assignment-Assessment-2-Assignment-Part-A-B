# Recipe Management System

COMP2021 Software Development Practice – Assignment Parts A and B.

A terminal recipe manager. Recipes are loaded from `data/recipes.json` with the supplied `RecipeLoader`, and all of the collection logic is in `RecipeManagement.Core/RecipeManager.cs`.

## Projects

- `RecipeManagement.Application` – supplied console menu (`Program.cs`, `ConsoleMenu.cs`)
- `RecipeManagement.Core` – models, `RecipeLoader`, `IRecipeManager` and my `RecipeManager` implementation
- `RecipeManagement.Tests` – xUnit tests
  - `RecipeManagerTests.cs` – supplied example tests
  - `PartATests.cs` – Part A tests (catalogue, shopping list, cooking plan, removed stack, instruction queue, workflow)
  - `PartBTests.cs` – Part B tests (title/ingredient search, protein report, saved recipes, workflow)

## Collections used

| Role | Collection |
| --- | --- |
| Recipe catalogue | `Dictionary<int, Recipe>` |
| Shopping list | `List<string>` |
| Cooking plan | `LinkedList<int>` |
| Removed recipe history | `Stack<int>` |
| Cooking instructions | `Queue<string>` |
| Saved recipes (Part B) | `HashSet<int>` |

Part B searches and the protein report use LINQ. See `Design.md` for the saved-recipe collection choice and the complexity table.

## Requirements

- .NET 10 SDK

## Build, test and run

Run these from the repository root (the folder with `RecipeManagement.sln`):

```bash
dotnet build
dotnet test
dotnet run --project RecipeManagement.Application -- data/recipes.json
```

If no path is given the program looks for `data/recipes.json` itself.

## AI acknowledgement

<!-- TODO: add the AI acknowledgement statement required by the assignment specification -->
