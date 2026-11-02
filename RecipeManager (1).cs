using System;
using System.Collections.Generic;
using System.Linq;

namespace RecipeManagement.Core;

/// <summary>
/// Main recipe manager. Holds the recipe catalogue, shopping list, cooking plan,
/// removed-recipe history, instruction queue and the saved recipes.
/// </summary>
public sealed class RecipeManager : IRecipeManager
{
    // Part A collections
    private readonly Dictionary<int, Recipe> _recipes = new();
    private readonly List<string> _shoppingList = new();
    private readonly LinkedList<int> _cookingPlan = new();
    private readonly Stack<int> _removedRecipes = new();
    private readonly Queue<string> _instructions = new();

    // Part B - saved recipes. HashSet because we only need unique ids + fast Contains
    private readonly HashSet<int> _savedRecipes = new();

    public RecipeManager(IEnumerable<Recipe> recipes)
    {
        if (recipes == null)
        {
            throw new ArgumentNullException(nameof(recipes));
        }

        foreach (Recipe recipe in recipes)
        {
            if (recipe == null)
            {
                throw new ArgumentException("Recipe list contains a null recipe.", nameof(recipes));
            }

            if (!IsValidRecipe(recipe))
            {
                throw new ArgumentException(
                    $"Recipe {recipe.Id} needs a positive id and a title.", nameof(recipes));
            }

            if (_recipes.ContainsKey(recipe.Id))
            {
                throw new ArgumentException($"Duplicate recipe id {recipe.Id}.", nameof(recipes));
            }

            _recipes.Add(recipe.Id, recipe);
        }
    }

    public int RecipeCount => _recipes.Count;
    public int ShoppingItemCount => _shoppingList.Count;
    public int CookingPlanCount => _cookingPlan.Count;
    public int PendingInstructionCount => _instructions.Count;
    public int RemovedRecipeCount => _removedRecipes.Count;

    // ---------- Recipe catalogue (Dictionary) ----------

    public bool AddRecipe(Recipe recipe)
    {
        if (recipe == null)
        {
            throw new ArgumentNullException(nameof(recipe));
        }

        if (!IsValidRecipe(recipe) || _recipes.ContainsKey(recipe.Id))
        {
            return false;
        }

        _recipes.Add(recipe.Id, recipe);
        return true;
    }

    public Recipe? FindRecipe(int recipeId)
    {
        _recipes.TryGetValue(recipeId, out Recipe? recipe);
        return recipe;
    }

    public bool RemoveRecipe(int recipeId)
    {
        // can't remove something that is still planned to be cooked
        if (!_recipes.ContainsKey(recipeId) || _cookingPlan.Contains(recipeId))
        {
            return false;
        }

        return _recipes.Remove(recipeId);
    }

    // ---------- Shopping list (List) ----------

    public int AddIngredientsToShoppingList(int recipeId)
    {
        Recipe? recipe = FindRecipe(recipeId);
        if (recipe == null)
        {
            return 0;
        }

        // just copy the original strings in order, no merging
        _shoppingList.AddRange(recipe.Ingredients);
        return recipe.Ingredients.Count;
    }

    public IReadOnlyList<string> GetShoppingList()
    {
        // return a copy so the caller can't change the real list
        return _shoppingList.ToList();
    }

    public void ClearShoppingList()
    {
        _shoppingList.Clear();
    }

    // ---------- Cooking plan (LinkedList) + removed history (Stack) ----------

    public bool AddRecipeToCookingPlan(int recipeId)
    {
        if (!_recipes.ContainsKey(recipeId) || _cookingPlan.Contains(recipeId))
        {
            return false;
        }

        _cookingPlan.AddLast(recipeId);
        return true;
    }

    public bool RemoveRecipeFromCookingPlan(int recipeId)
    {
        // LinkedList.Remove returns false if the id isn't there, so the stack is left alone
        if (!_cookingPlan.Remove(recipeId))
        {
            return false;
        }

        _removedRecipes.Push(recipeId);
        return true;
    }

    public bool RestoreLastRemovedRecipe()
    {
        if (_removedRecipes.Count == 0)
        {
            return false;
        }

        int recipeId = _removedRecipes.Pop();

        // recipe might have been deleted from the catalogue or re-added to the plan since
        if (!_recipes.ContainsKey(recipeId) || _cookingPlan.Contains(recipeId))
        {
            return false;
        }

        _cookingPlan.AddLast(recipeId);
        return true;
    }

    public int? PeekLastRemovedRecipe()
    {
        if (_removedRecipes.Count == 0)
        {
            return null;
        }

        return _removedRecipes.Peek();
    }

    public IReadOnlyList<int> GetCookingPlan()
    {
        // walks the linked list from first to last
        return _cookingPlan.ToList();
    }

    // ---------- Cooking instructions (Queue) ----------

    public bool StartCooking(int recipeId)
    {
        Recipe? recipe = FindRecipe(recipeId);
        if (recipe == null || recipe.Instructions.Count == 0)
        {
            return false;
        }

        _instructions.Clear();
        foreach (string step in recipe.Instructions)
        {
            _instructions.Enqueue(step);
        }

        return true;
    }

    public string? PeekNextInstruction()
    {
        if (_instructions.Count == 0)
        {
            return null;
        }

        return _instructions.Peek();
    }

    public string? CompleteNextInstruction()
    {
        if (_instructions.Count == 0)
        {
            return null;
        }

        return _instructions.Dequeue();
    }

    // ---------- Part B: LINQ searches and report ----------

    public IReadOnlyList<Recipe> SearchByTitle(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return GetAllRecipesById();
        }

        string text = searchText.Trim();

        return _recipes.Values
            .Where(r => r.Title.Contains(text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Id)
            .ToList();
    }

    public IReadOnlyList<Recipe> SearchByIngredient(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return GetAllRecipesById();
        }

        string text = searchText.Trim();

        // Any() stops at the first matching ingredient, so a recipe only comes back once
        return _recipes.Values
            .Where(r => r.Ingredients.Any(ing =>
                ing != null && ing.Contains(text, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(r => r.Id)
            .ToList();
    }

    public IReadOnlyList<Recipe> GetHighestProteinRecipes(int count)
    {
        if (count <= 0)
        {
            return new List<Recipe>();
        }

        return _recipes.Values
            .Where(r => r.Nutrition?.ProteinG != null)   // skip missing protein data
            .OrderByDescending(r => r.Nutrition!.ProteinG!.Value)
            .ThenBy(r => r.Id)
            .Take(count)
            .ToList();
    }

    // ---------- Part B: saved recipes (HashSet) ----------

    public bool AddSavedRecipe(int recipeId)
    {
        if (!_recipes.ContainsKey(recipeId))
        {
            return false;
        }

        // HashSet.Add gives false if it's already saved
        return _savedRecipes.Add(recipeId);
    }

    public bool RemoveSavedRecipe(int recipeId)
    {
        return _savedRecipes.Remove(recipeId);
    }

    public bool IsRecipeSaved(int recipeId)
    {
        return _savedRecipes.Contains(recipeId);
    }

    public IReadOnlyList<int> GetSavedRecipes()
    {
        // order isn't assessed but sorting makes the menu output easier to read
        return _savedRecipes.OrderBy(id => id).ToList();
    }

    // ---------- helpers ----------

    private static bool IsValidRecipe(Recipe recipe)
    {
        return recipe.Id > 0 && !string.IsNullOrWhiteSpace(recipe.Title);
    }

    private List<Recipe> GetAllRecipesById()
    {
        return _recipes.Values.OrderBy(r => r.Id).ToList();
    }
}
