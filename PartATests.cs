using System;
using System.Collections.Generic;
using RecipeManagement.Core;

namespace RecipeManagement.Tests;

// Part A tests - dictionary, list, linked list, stack, queue
public sealed class PartATests
{
    // small test data, doesn't rely on the big json file
    private static List<Recipe> TestRecipes()
    {
        return new List<Recipe>
        {
            new Recipe
            {
                Id = 1,
                Title = "Pancakes",
                Ingredients = new() { "2 cups flour", "2 eggs", "1 cup milk" },
                Instructions = new() { "Mix everything", "Rest batter", "Cook in pan" }
            },
            new Recipe
            {
                Id = 2,
                Title = "Toast",
                Ingredients = new() { "2 slices bread", "butter" },
                Instructions = new() { "Toast bread", "Spread butter" }
            },
            new Recipe
            {
                Id = 3,
                Title = "Glass of water",
                Ingredients = new() { "water" }
                // no instructions on purpose
            }
        };
    }

    private static IRecipeManager NewManager() => new RecipeManager(TestRecipes());

    // ===== Constructor / Dictionary =====

    [Fact]
    public void Constructor_LoadsAllRecipes()
    {
        IRecipeManager manager = NewManager();

        Assert.Equal(3, manager.RecipeCount);
        Assert.Equal("Toast", manager.FindRecipe(2)?.Title);
    }

    [Fact]
    public void Constructor_NullCollection_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RecipeManager(null!));
    }

    [Fact]
    public void Constructor_DuplicateId_Throws()
    {
        var recipes = TestRecipes();
        recipes.Add(new Recipe { Id = 1, Title = "Another pancake" });

        Assert.Throws<ArgumentException>(() => new RecipeManager(recipes));
    }

    [Theory]
    [InlineData(0, "Zero id")]
    [InlineData(-5, "Negative id")]
    [InlineData(7, "")]
    [InlineData(8, "   ")]
    public void Constructor_InvalidRecipe_Throws(int id, string title)
    {
        var recipes = new List<Recipe> { new Recipe { Id = id, Title = title } };

        Assert.Throws<ArgumentException>(() => new RecipeManager(recipes));
    }

    [Fact]
    public void Constructor_EmptyCollection_IsFine()
    {
        var manager = new RecipeManager(Array.Empty<Recipe>());

        Assert.Equal(0, manager.RecipeCount);
    }

    [Fact]
    public void AddRecipe_NewRecipe_ReturnsTrueAndCanBeFound()
    {
        IRecipeManager manager = NewManager();

        bool added = manager.AddRecipe(new Recipe { Id = 50, Title = "Soup" });

        Assert.True(added);
        Assert.Equal(4, manager.RecipeCount);
        Assert.Equal("Soup", manager.FindRecipe(50)?.Title);
    }

    [Fact]
    public void AddRecipe_DuplicateId_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();

        bool added = manager.AddRecipe(new Recipe { Id = 1, Title = "Fake pancakes" });

        Assert.False(added);
        Assert.Equal(3, manager.RecipeCount);
        Assert.Equal("Pancakes", manager.FindRecipe(1)?.Title); // original not replaced
    }

    [Theory]
    [InlineData(0, "Soup")]
    [InlineData(-1, "Soup")]
    [InlineData(10, "")]
    [InlineData(11, "  ")]
    public void AddRecipe_InvalidRecipe_ReturnsFalse(int id, string title)
    {
        IRecipeManager manager = NewManager();

        Assert.False(manager.AddRecipe(new Recipe { Id = id, Title = title }));
        Assert.Equal(3, manager.RecipeCount);
    }

    [Fact]
    public void AddRecipe_Null_Throws()
    {
        IRecipeManager manager = NewManager();

        Assert.Throws<ArgumentNullException>(() => manager.AddRecipe(null!));
    }

    [Fact]
    public void FindRecipe_MissingId_ReturnsNull()
    {
        IRecipeManager manager = NewManager();

        Assert.Null(manager.FindRecipe(999));
    }

    [Fact]
    public void RemoveRecipe_Existing_ReturnsTrue()
    {
        IRecipeManager manager = NewManager();

        Assert.True(manager.RemoveRecipe(2));
        Assert.Null(manager.FindRecipe(2));
        Assert.Equal(2, manager.RecipeCount);
    }

    [Fact]
    public void RemoveRecipe_MissingId_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();

        Assert.False(manager.RemoveRecipe(999));
        Assert.Equal(3, manager.RecipeCount);
    }

    [Fact]
    public void RemoveRecipe_InCookingPlan_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();
        manager.AddRecipeToCookingPlan(1);

        Assert.False(manager.RemoveRecipe(1));
        Assert.NotNull(manager.FindRecipe(1));
    }

    // ===== Shopping list =====

    [Fact]
    public void AddIngredients_CopiesIngredientsInOrder()
    {
        IRecipeManager manager = NewManager();

        int added = manager.AddIngredientsToShoppingList(1);

        Assert.Equal(3, added);
        Assert.Equal(new[] { "2 cups flour", "2 eggs", "1 cup milk" }, manager.GetShoppingList());
        Assert.Equal(3, manager.ShoppingItemCount);
    }

    [Fact]
    public void AddIngredients_TwoRecipes_AppendsSecondAfterFirst()
    {
        IRecipeManager manager = NewManager();

        manager.AddIngredientsToShoppingList(2);
        manager.AddIngredientsToShoppingList(3);

        Assert.Equal(new[] { "2 slices bread", "butter", "water" }, manager.GetShoppingList());
    }

    [Fact]
    public void AddIngredients_SameRecipeTwice_KeepsDuplicates()
    {
        // spec says no merging, so the lines just get added again
        IRecipeManager manager = NewManager();

        manager.AddIngredientsToShoppingList(3);
        manager.AddIngredientsToShoppingList(3);

        Assert.Equal(new[] { "water", "water" }, manager.GetShoppingList());
    }

    [Fact]
    public void AddIngredients_MissingRecipe_ReturnsZero()
    {
        IRecipeManager manager = NewManager();

        Assert.Equal(0, manager.AddIngredientsToShoppingList(999));
        Assert.Empty(manager.GetShoppingList());
    }

    [Fact]
    public void ClearShoppingList_RemovesEverything()
    {
        IRecipeManager manager = NewManager();
        manager.AddIngredientsToShoppingList(1);

        manager.ClearShoppingList();

        Assert.Equal(0, manager.ShoppingItemCount);
        Assert.Empty(manager.GetShoppingList());
    }

    [Fact]
    public void GetShoppingList_ReturnsCopyNotInternalList()
    {
        IRecipeManager manager = NewManager();
        manager.AddIngredientsToShoppingList(2);

        var list = manager.GetShoppingList();
        if (list is List<string> asList)
        {
            asList.Add("sneaky extra item");
        }

        Assert.Equal(2, manager.ShoppingItemCount);
    }

    // ===== Cooking plan (LinkedList) =====

    [Fact]
    public void CookingPlan_AddsToEndInOrder()
    {
        IRecipeManager manager = NewManager();

        Assert.True(manager.AddRecipeToCookingPlan(2));
        Assert.True(manager.AddRecipeToCookingPlan(1));
        Assert.True(manager.AddRecipeToCookingPlan(3));

        Assert.Equal(new[] { 2, 1, 3 }, manager.GetCookingPlan());
        Assert.Equal(3, manager.CookingPlanCount);
    }

    [Fact]
    public void CookingPlan_DuplicateRecipe_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();
        manager.AddRecipeToCookingPlan(1);

        Assert.False(manager.AddRecipeToCookingPlan(1));
        Assert.Equal(new[] { 1 }, manager.GetCookingPlan());
    }

    [Fact]
    public void CookingPlan_MissingRecipe_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();

        Assert.False(manager.AddRecipeToCookingPlan(999));
        Assert.Empty(manager.GetCookingPlan());
    }

    [Fact]
    public void CookingPlan_RemoveFromMiddle_KeepsOrder()
    {
        IRecipeManager manager = NewManager();
        manager.AddRecipeToCookingPlan(1);
        manager.AddRecipeToCookingPlan(2);
        manager.AddRecipeToCookingPlan(3);

        Assert.True(manager.RemoveRecipeFromCookingPlan(2));

        Assert.Equal(new[] { 1, 3 }, manager.GetCookingPlan());
    }

    // ===== Removed history (Stack) =====

    [Fact]
    public void RemoveFromPlan_PushesIdOntoStack()
    {
        IRecipeManager manager = NewManager();
        manager.AddRecipeToCookingPlan(1);

        manager.RemoveRecipeFromCookingPlan(1);

        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.Equal(1, manager.PeekLastRemovedRecipe());
    }

    [Fact]
    public void RemoveFromPlan_NotPlanned_ReturnsFalseAndStackUnchanged()
    {
        IRecipeManager manager = NewManager();
        manager.AddRecipeToCookingPlan(1);
        manager.RemoveRecipeFromCookingPlan(1);

        Assert.False(manager.RemoveRecipeFromCookingPlan(2));

        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.Equal(1, manager.PeekLastRemovedRecipe());
    }

    [Fact]
    public void PeekLastRemoved_EmptyStack_ReturnsNull()
    {
        IRecipeManager manager = NewManager();

        Assert.Null(manager.PeekLastRemovedRecipe());
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void PeekLastRemoved_DoesNotPop()
    {
        IRecipeManager manager = NewManager();
        manager.AddRecipeToCookingPlan(1);
        manager.RemoveRecipeFromCookingPlan(1);

        manager.PeekLastRemovedRecipe();
        manager.PeekLastRemovedRecipe();

        Assert.Equal(1, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RestoreLastRemoved_EmptyStack_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();

        Assert.False(manager.RestoreLastRemovedRecipe());
    }

    [Fact]
    public void RestoreLastRemoved_RestoresInLifoOrderToEndOfPlan()
    {
        IRecipeManager manager = NewManager();
        manager.AddRecipeToCookingPlan(1);
        manager.AddRecipeToCookingPlan(2);
        manager.AddRecipeToCookingPlan(3);
        manager.RemoveRecipeFromCookingPlan(1);
        manager.RemoveRecipeFromCookingPlan(2);   // plan is now [3], stack top is 2

        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 3, 2 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.PeekLastRemovedRecipe());

        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 3, 2, 1 }, manager.GetCookingPlan());
        Assert.Equal(0, manager.RemovedRecipeCount);
        Assert.False(manager.RestoreLastRemovedRecipe());
    }

    [Fact]
    public void RestoreLastRemoved_AlreadyPlannedAgain_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();
        manager.AddRecipeToCookingPlan(1);
        manager.RemoveRecipeFromCookingPlan(1);
        manager.AddRecipeToCookingPlan(1); // added back by hand

        Assert.False(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 1 }, manager.GetCookingPlan()); // still only once
    }

    [Fact]
    public void RestoreLastRemoved_RecipeDeletedFromCatalogue_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();
        manager.AddRecipeToCookingPlan(2);
        manager.RemoveRecipeFromCookingPlan(2);
        manager.RemoveRecipe(2);

        Assert.False(manager.RestoreLastRemovedRecipe());
        Assert.Empty(manager.GetCookingPlan());
    }

    // ===== Instructions (Queue) =====

    [Fact]
    public void StartCooking_LoadsInstructionsInOrder()
    {
        IRecipeManager manager = NewManager();

        Assert.True(manager.StartCooking(1));
        Assert.Equal(3, manager.PendingInstructionCount);
        Assert.Equal("Mix everything", manager.CompleteNextInstruction());
        Assert.Equal("Rest batter", manager.CompleteNextInstruction());
        Assert.Equal("Cook in pan", manager.CompleteNextInstruction());
    }

    [Fact]
    public void StartCooking_MissingRecipe_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();

        Assert.False(manager.StartCooking(999));
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    [Fact]
    public void StartCooking_NoInstructions_ReturnsFalse()
    {
        IRecipeManager manager = NewManager();

        Assert.False(manager.StartCooking(3));
    }

    [Fact]
    public void StartCooking_InvalidRecipe_KeepsCurrentQueue()
    {
        IRecipeManager manager = NewManager();
        manager.StartCooking(2);

        manager.StartCooking(3); // fails, has no instructions

        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("Toast bread", manager.PeekNextInstruction());
    }

    [Fact]
    public void StartCooking_AgainClearsOldQueue()
    {
        IRecipeManager manager = NewManager();
        manager.StartCooking(1);
        manager.CompleteNextInstruction();

        manager.StartCooking(2);

        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("Toast bread", manager.PeekNextInstruction());
    }

    [Fact]
    public void PeekNextInstruction_DoesNotRemove()
    {
        IRecipeManager manager = NewManager();
        manager.StartCooking(2);

        Assert.Equal("Toast bread", manager.PeekNextInstruction());
        Assert.Equal("Toast bread", manager.PeekNextInstruction());
        Assert.Equal(2, manager.PendingInstructionCount);
    }

    [Fact]
    public void CompleteNextInstruction_RemovesExactlyOne()
    {
        IRecipeManager manager = NewManager();
        manager.StartCooking(2);

        manager.CompleteNextInstruction();

        Assert.Equal(1, manager.PendingInstructionCount);
        Assert.Equal("Spread butter", manager.PeekNextInstruction());
    }

    [Fact]
    public void EmptyQueue_PeekAndComplete_ReturnNull()
    {
        IRecipeManager manager = NewManager();

        Assert.Null(manager.PeekNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
    }

    [Fact]
    public void Queue_AfterLastStep_ReturnsNull()
    {
        IRecipeManager manager = NewManager();
        manager.StartCooking(2);
        manager.CompleteNextInstruction();
        manager.CompleteNextInstruction();

        Assert.Null(manager.PeekNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    // ===== Interaction between components =====

    [Fact]
    public void Workflow_RecipeGoesThroughCatalogueListPlanStackAndQueue()
    {
        IRecipeManager manager = NewManager();

        // find + shopping list
        Recipe? pancakes = manager.FindRecipe(1);
        Assert.NotNull(pancakes);
        Assert.Equal(3, manager.AddIngredientsToShoppingList(pancakes!.Id));

        // plan, remove, restore
        manager.AddRecipeToCookingPlan(1);
        manager.AddRecipeToCookingPlan(2);
        manager.RemoveRecipeFromCookingPlan(1);
        Assert.Equal(new[] { 2 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.PeekLastRemovedRecipe());
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 2, 1 }, manager.GetCookingPlan());

        // cook the first planned recipe until the queue is empty
        int first = manager.GetCookingPlan()[0];
        Assert.True(manager.StartCooking(first));

        var done = new List<string>();
        while (manager.PeekNextInstruction() != null)
        {
            done.Add(manager.CompleteNextInstruction()!);
        }

        Assert.Equal(new[] { "Toast bread", "Spread butter" }, done);
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    [Fact]
    public void Workflow_CanRemoveRecipeOnlyAfterTakingItOutOfPlan()
    {
        IRecipeManager manager = NewManager();
        manager.AddRecipeToCookingPlan(2);

        Assert.False(manager.RemoveRecipe(2));

        manager.RemoveRecipeFromCookingPlan(2);
        Assert.True(manager.RemoveRecipe(2));
        Assert.False(manager.AddRecipeToCookingPlan(2)); // gone from catalogue now
    }
}
