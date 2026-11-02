# Recipe Management System

COMP2021 Software Development Practice – Assignment Part A.

A terminal recipe manager. Recipes are loaded from `data/recipes.json` with the supplied `RecipeLoader`, and all of the collection logic is in `RecipeManagement.Core/RecipeManager.cs`.

## Projects

- `RecipeManagement.Application` – supplied console menu (`Program.cs`, `ConsoleMenu.cs`)
- `RecipeManagement.Core` – models, `RecipeLoader`, `IRecipeManager` and my `RecipeManager` implementation
- `RecipeManagement.Tests` – xUnit tests
  - `RecipeManagerTests.cs` – supplied example tests
  - `PartATests.cs` – Part A tests (catalogue, shopping list, cooking plan, removed stack, instruction queue, workflow)

## Collections used (Part A)

| Role | Collection |
| --- | --- |
| Recipe catalogue | `Dictionary<int, Recipe>` |
| Shopping list | `List<string>` |
| Cooking plan | `LinkedList<int>` |
| Removed recipe history | `Stack<int>` |
| Cooking instructions | `Queue<string>` |

Part B members (menu options 15–21) are not implemented yet and show a "Not implemented" message.

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
