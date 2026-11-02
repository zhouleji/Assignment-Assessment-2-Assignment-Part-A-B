# Design

## 1. Saved recipe collection

I used a **`HashSet<int>`** (`_savedRecipes` in `RecipeManager`) to store the saved recipe IDs.

The saved feature only needs to hold a group of unique IDs and answer "is this one saved?", and a HashSet fits that well:

- **Add** – `HashSet.Add(id)` returns `false` if the ID is already in the set, so duplicate prevention comes for free. Before adding I check the main recipe dictionary (`_recipes.ContainsKey`) so only recipes that actually exist can be saved.
- **Remove** – `HashSet.Remove(id)` returns `false` when the ID isn't saved, which is exactly what `RemoveSavedRecipe` needs to return.
- **Membership check** – `HashSet.Contains(id)` is a hash lookup, so `IsRecipeSaved` doesn't have to loop through every saved ID.
- **Order** – a HashSet doesn't keep insertion order, but the spec says the order of saved IDs isn't assessed, so this isn't a problem. `GetSavedRecipes` sorts the IDs before returning them, just so the console output looks tidy.

Other options I considered:

- `List<int>` would work, but `Contains` and `Remove` would be O(n) and I would have to write the duplicate check myself.
- `Dictionary<int, Recipe>` would also be O(1), but I don't need a value for each key because the recipe objects are already in the main catalogue, so it would just be duplicating data.
- `SortedSet<int>` keeps the IDs sorted but its operations are O(log n), and sorting isn't needed.

## 2. Integration

All Part B features were added to the existing `RecipeManager` class instead of making a new class, so they work on the same `Dictionary<int, Recipe>` catalogue that Part A builds from the `RecipeLoader` data. The title search, ingredient search and protein report are LINQ queries over `_recipes.Values` (`Where` to filter, `OrderBy`/`OrderByDescending` to sort, `Take` to limit, `ToList` to return a copy), so any recipe added or removed with the Part A `AddRecipe`/`RemoveRecipe` methods shows up (or disappears) in the results straight away. The saved recipe feature uses the dictionary's `ContainsKey` to make sure an ID exists before it is saved. The results are returned as `IReadOnlyList` through the same `IRecipeManager` interface, so the supplied console menu (options 15–21) calls them the same way as the Part A options, and the IDs from a search can be passed straight into Part A methods such as `AddRecipeToCookingPlan` or `StartCooking` (this is tested in `PartBTests.Workflow_PartBWorksWithPartACatalogueAndPlan`).

## 3. Basic complexity

n = number of items in the structure being used (recipes in the catalogue, or IDs in the cooking plan), s = number of saved recipes.

| Operation | Structure | Expected complexity | Reason |
| --- | --- | --- | --- |
| Lookup recipe by ID | Dictionary | Average O(1) | Hash-based key lookup. |
| Traverse cooking plan | LinkedList | O(n) | Each planned recipe may need to be visited. |
| Complete next instruction | Queue | O(1) | The item at the front is removed. |
| LINQ title/ingredient search | Recipe collection | O(n) | Each recipe may need to be inspected. |
| Check whether a recipe is saved | HashSet\<int\> | Average O(1) | `Contains` hashes the ID and goes straight to its bucket, so it doesn't have to compare against every saved ID. Worst case is O(s) if lots of IDs collide, but that's unlikely with int keys. |

Some extra notes on my implementation:

- The ingredient search also has to look at each ingredient line of each recipe, so strictly it's O(total number of ingredient lines), but this still grows linearly with the number of recipes. I sort the matches by ID, which adds O(m log m) for the m matching recipes.
- `GetHighestProteinRecipes` filters in O(n) and then sorts the recipes with protein data, so it is O(n log n) overall.
- `AddSavedRecipe` and `RemoveSavedRecipe` are both average O(1) (one dictionary lookup + one HashSet operation).
- `GetSavedRecipes` is O(s log s) because I sort the IDs before returning them.
