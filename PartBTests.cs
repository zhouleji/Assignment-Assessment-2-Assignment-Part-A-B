using System.Collections.Generic;
using System.Linq;
using RecipeManagement.Core;

namespace RecipeManagement.Tests;

// Part B tests - LINQ searches, protein report and saved recipes
public sealed class PartBTests
{
    private static List<Recipe> TestRecipes()
    {
        return new List<Recipe>
        {
            new Recipe
            {
                Id = 1,
                Title = "Chicken Curry",
                Ingredients = new() { "500g chicken thigh", "1 onion", "Curry paste" },
                Instructions = new() { "Fry onion", "Add chicken", "Simmer" },
                Nutrition = new NutritionInfo { ProteinG = 35.5 }
            },
            new Recipe
            {
                Id = 2,
                Title = "Garlic Bread",
                Ingredients = new() { "1 baguette", "3 garlic cloves", "Butter" },
                Instructions = new() { "Mix butter and garlic", "Bake" },
                Nutrition = new NutritionInfo { ProteinG = 8 }
            },
            new Recipe
            {
                Id = 3,
                Title = "Roast CHICKEN with garlic",
                Ingredients = new() { "1 whole chicken", "garlic", "GARLIC salt" },
                Instructions = new() { "Roast" },
                Nutrition = new NutritionInfo { ProteinG = 52.1 }
            },
            new Recipe
            {
                // nutrition object there but protein missing
                Id = 4,
                Title = "Fruit Salad",
                Ingredients = new() { "1 apple", "1 banana" },
                Instructions = new() { "Chop fruit" },
                Nutrition = new NutritionInfo { ProteinG = null, CaloriesKcal = 120 }
            },
            new Recipe
            {
                // no nutrition at all
                Id = 5,
                Title = "Plain Rice",
                Ingredients = new() { "1 cup rice", "2 cups water" },
                Instructions = new() { "Boil" },
                Nutrition = null
            },
            new Recipe
            {
                Id = 6,
                Title = "Tofu Stir Fry",
                Ingredients = new() { "300g tofu", "soy sauce" },
                Instructions = new() { "Stir fry" },
                Nutrition = new NutritionInfo { ProteinG = 0 }
            }
        };
    }

    private static IRecipeManager NewManager() => new RecipeManager(TestRecipes());

    private static int[] Ids(IEnumerable<Recipe> recipes) => recipes.Select(r => r.Id).ToArray();

    // ===== Title search =====

    [Fact]
    public void SearchByTitle_PartialMatch()
    {
        IRecipeManager manager = NewManager();

        var result = manager.SearchByTitle("bread");

        Assert.Equal(new[] { 2 }, Ids(result));
    }

    [Theory]
    [InlineData("chicken")]
    [InlineData("CHICKEN")]
    [InlineData("ChIcKeN")]
    public void SearchByTitle_IsCaseInsensitive(string text)
    {
        IRecipeManager manager = NewManager();

        var result = manager.SearchByTitle(text);

        Assert.Equal(new[] { 1, 3 }, Ids(result));
    }

    [Fact]
    public void SearchByTitle_NoMatch_ReturnsEmpty()
    {
        IRecipeManager manager = NewManager();

        Assert.Empty(manager.SearchByTitle("pizza"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SearchByTitle_Blank_ReturnsAllRecipes(string text)
    {
        IRecipeManager manager = NewManager();

        var result = manager.SearchByTitle(text);

        Assert.Equal(6, result.Count);
    }

    [Fact]
    public void SearchByTitle_Null_DoesNotCrash()
    {
        IRecipeManager manager = NewManager();

        var result = manager.SearchByTitle(null!);

        Assert.Equal(6, result.Count);
    }

    [Fact]
    public void SearchByTitle_EachRecipeOnlyOnce()
    {
        IRecipeManager manager = NewManager();

        // "garlic" is in the title of 2 and 3 - check no doubles
        var ids = Ids(manager.SearchByTitle("garlic"));

        Assert.Equal(ids.Distinct().Count(), ids.Length);
        Assert.Equal(new[] { 2, 3 }, ids);
    }

    // ===== Ingredient search =====

    [Fact]
    public void SearchByIngredient_PartialMatch()
    {
        IRecipeManager manager = NewManager();

        var result = manager.SearchByIngredient("tofu");

        Assert.Equal(new[] { 6 }, Ids(result));
    }

    [Fact]
    public void SearchByIngredient_IsCaseInsensitive()
    {
        IRecipeManager manager = NewManager();

        var result = manager.SearchByIngredient("BUTTER");

        Assert.Equal(new[] { 2 }, Ids(result));
    }

    [Fact]
    public void SearchByIngredient_RecipeWithManyMatchingLines_ReturnedOnce()
    {
        // recipe 3 has "garlic" AND "GARLIC salt", recipe 2 has "3 garlic cloves"
        IRecipeManager manager = NewManager();

        var result = manager.SearchByIngredient("garlic");

        Assert.Equal(new[] { 2, 3 }, Ids(result));
    }

    [Fact]
    public void SearchByIngredient_MatchesInsideIngredientLine()
    {
        IRecipeManager manager = NewManager();

        // "chicken" is inside "500g chicken thigh" and "1 whole chicken"
        var result = manager.SearchByIngredient("chicken");

        Assert.Equal(new[] { 1, 3 }, Ids(result));
    }

    [Fact]
    public void SearchByIngredient_NoMatch_ReturnsEmpty()
    {
        IRecipeManager manager = NewManager();

        Assert.Empty(manager.SearchByIngredient("chocolate"));
    }

    [Fact]
    public void SearchByIngredient_DoesNotSearchTitles()
    {
        IRecipeManager manager = NewManager();

        // "Salad" is only in a title, not in any ingredient
        Assert.Empty(manager.SearchByIngredient("salad"));
    }

    // ===== Protein report =====

    [Fact]
    public void HighestProtein_ReturnsDescendingOrder()
    {
        IRecipeManager manager = NewManager();

        var result = manager.GetHighestProteinRecipes(3);

        Assert.Equal(new[] { 3, 1, 2 }, Ids(result));
    }

    [Fact]
    public void HighestProtein_ExcludesMissingProteinData()
    {
        IRecipeManager manager = NewManager();

        // ask for more than there are
        var result = manager.GetHighestProteinRecipes(100);

        Assert.Equal(new[] { 3, 1, 2, 6 }, Ids(result));
        Assert.DoesNotContain(result, r => r.Id == 4); // ProteinG null
        Assert.DoesNotContain(result, r => r.Id == 5); // Nutrition null
    }

    [Fact]
    public void HighestProtein_ZeroProteinIsStillIncluded()
    {
        // 0 is a real value, not missing data
        IRecipeManager manager = NewManager();

        var result = manager.GetHighestProteinRecipes(10);

        Assert.Contains(result, r => r.Id == 6);
    }

    [Fact]
    public void HighestProtein_CountOne_ReturnsTopRecipe()
    {
        IRecipeManager manager = NewManager();

        var result = manager.GetHighestProteinRecipes(1);

        Assert.Single(result);
        Assert.Equal("Roast CHICKEN with garlic", result[0].Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void HighestProtein_ZeroOrNegativeCount_ReturnsEmpty(int count)
    {
        IRecipeManager manager = NewManager();

        Assert.Empty(manager.GetHighestProteinRecipes(count));
    }

    [Fact]
    public void HighestProtein_AllMissingNutrition_DoesNotCrash()
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe { Id = 1, Title = "A" },
            new Recipe { Id = 2, Title = "B", Nutrition = new NutritionInfo() }
        });

        Assert.Empty(manager.GetHighestProteinRecipes(5));
    }

    // ===== Saved recipes (HashSet) =====

    [Fact]
    public void AddSavedRecipe_ExistingRecipe_ReturnsTrue()
    {
        IRecipeManager manager = NewManager();

        Assert.True(manager.AddSavedRecipe(2));
        Assert.True(manager.IsRecipeSaved(2));
    }

    [Fact]
    public void AddSavedRecipe_MissingRecipe_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();

        Assert.False(manager.AddSavedRecipe(999));
        Assert.False(manager.IsRecipeSaved(999));
        Assert.Empty(manager.GetSavedRecipes());
    }

    [Fact]
    public void AddSavedRecipe_Duplicate_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();
        manager.AddSavedRecipe(1);

        Assert.False(manager.AddSavedRecipe(1));
        Assert.Single(manager.GetSavedRecipes());
    }

    [Fact]
    public void RemoveSavedRecipe_Saved_ReturnsTrue()
    {
        IRecipeManager manager = NewManager();
        manager.AddSavedRecipe(1);

        Assert.True(manager.RemoveSavedRecipe(1));
        Assert.False(manager.IsRecipeSaved(1));
    }

    [Fact]
    public void RemoveSavedRecipe_NotSaved_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();

        Assert.False(manager.RemoveSavedRecipe(1));    // exists but not saved
        Assert.False(manager.RemoveSavedRecipe(999));  // doesn't exist at all
    }

    [Fact]
    public void IsRecipeSaved_NothingSaved_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();

        Assert.False(manager.IsRecipeSaved(1));
    }

    [Fact]
    public void GetSavedRecipes_ReturnsAllSavedIds()
    {
        IRecipeManager manager = NewManager();
        manager.AddSavedRecipe(5);
        manager.AddSavedRecipe(1);
        manager.AddSavedRecipe(3);

        var saved = manager.GetSavedRecipes();

        // order isn't assessed so compare as sets
        Assert.Equal(3, saved.Count);
        Assert.True(new HashSet<int> { 1, 3, 5 }.SetEquals(saved));
    }

    [Fact]
    public void GetSavedRecipes_NoneSaved_ReturnsEmptyList()
    {
        IRecipeManager manager = NewManager();

        var saved = manager.GetSavedRecipes();

        Assert.NotNull(saved);
        Assert.Empty(saved);
    }

    [Fact]
    public void SavedRecipe_CanBeSavedAgainAfterRemove()
    {
        IRecipeManager manager = NewManager();
        manager.AddSavedRecipe(4);
        manager.RemoveSavedRecipe(4);

        Assert.True(manager.AddSavedRecipe(4));
        Assert.True(manager.IsRecipeSaved(4));
    }

    // ===== Integrated Part B workflow =====

    [Fact]
    public void Workflow_SearchThenSaveThenReport()
    {
        IRecipeManager manager = NewManager();

        // search and save every chicken recipe
        foreach (Recipe r in manager.SearchByTitle("chicken"))
        {
            Assert.True(manager.AddSavedRecipe(r.Id));
        }

        Assert.True(new HashSet<int> { 1, 3 }.SetEquals(manager.GetSavedRecipes()));

        // the top protein recipe should be one of the saved ones
        Recipe top = manager.GetHighestProteinRecipes(1).Single();
        Assert.True(manager.IsRecipeSaved(top.Id));

        // unsave it and check the rest still there
        Assert.True(manager.RemoveSavedRecipe(top.Id));
        Assert.Equal(new[] { 1 }, manager.GetSavedRecipes());
    }

    [Fact]
    public void Workflow_PartBWorksWithPartACatalogueAndPlan()
    {
        IRecipeManager manager = NewManager();

        // new recipe added through Part A shows up in Part B searches/report
        manager.AddRecipe(new Recipe
        {
            Id = 20,
            Title = "Steak and Eggs",
            Ingredients = new() { "1 steak", "2 eggs" },
            Instructions = new() { "Grill steak", "Fry eggs" },
            Nutrition = new NutritionInfo { ProteinG = 70 }
        });

        Assert.Equal(new[] { 20 }, Ids(manager.SearchByIngredient("steak")));
        Assert.Equal(20, manager.GetHighestProteinRecipes(1)[0].Id);
        Assert.True(manager.AddSavedRecipe(20));

        // use a search result with the Part A plan + queue
        int found = manager.SearchByTitle("steak")[0].Id;
        Assert.True(manager.AddRecipeToCookingPlan(found));
        Assert.True(manager.StartCooking(found));
        Assert.Equal("Grill steak", manager.PeekNextInstruction());

        // removed recipe no longer shows up in searches
        manager.RemoveRecipeFromCookingPlan(found);
        Assert.True(manager.RemoveRecipe(found));
        Assert.Empty(manager.SearchByTitle("steak"));
        Assert.Equal(3, manager.GetHighestProteinRecipes(1)[0].Id);
    }
}
