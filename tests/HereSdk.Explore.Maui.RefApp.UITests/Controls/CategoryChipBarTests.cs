using Xunit;
using Here.Explore.Maui.RefApp.Controls;

namespace Here.Explore.Maui.RefApp.UITests.Controls;

public class CategoryChipBarTests
{
    [Fact]
    public void DefaultCategories_HasAllEight()
    {
        Assert.Equal(8, CategoryChipBar.DefaultCategories.Length);
    }

    [Fact]
    public void DefaultCategories_HaveCorrectIds()
    {
        var ids = CategoryChipBar.DefaultCategories.Select(c => c.CategoryId).ToList();
        Assert.Contains("restaurant", ids);
        Assert.Contains("hotel", ids);
        Assert.Contains("fuel-station", ids);
        Assert.Contains("parking", ids);
        Assert.Contains("atm", ids);
        Assert.Contains("hospital", ids);
        Assert.Contains("shopping", ids);
        Assert.Contains("attraction", ids);
    }

    [Fact]
    public void CategoryChip_Restaurant_HasCorrectProperties()
    {
        var chip = CategoryChipBar.DefaultCategories[0];
        Assert.Equal("restaurant", chip.CategoryId);
        Assert.Equal("Restaurants", chip.Label);
        Assert.NotNull(chip.Icon);
        Assert.False(string.IsNullOrEmpty(chip.Icon));
    }

    [Fact]
    public void CategoryChip_Hotel_HasCorrectProperties()
    {
        var chip = CategoryChipBar.DefaultCategories[1];
        Assert.Equal("hotel", chip.CategoryId);
        Assert.Equal("Hotels", chip.Label);
    }

    [Fact]
    public void CategoryChip_FuelStation_HasCorrectProperties()
    {
        var chip = CategoryChipBar.DefaultCategories[2];
        Assert.Equal("fuel-station", chip.CategoryId);
        Assert.Equal("Gas Stations", chip.Label);
    }

    [Fact]
    public void CategoryChip_Parking_HasCorrectProperties()
    {
        var chip = CategoryChipBar.DefaultCategories[3];
        Assert.Equal("parking", chip.CategoryId);
        Assert.Equal("Parking", chip.Label);
    }

    [Fact]
    public void CategoryChip_ATM_HasCorrectProperties()
    {
        var chip = CategoryChipBar.DefaultCategories[4];
        Assert.Equal("atm", chip.CategoryId);
        Assert.Equal("ATMs", chip.Label);
    }

    [Fact]
    public void CategoryChip_Hospital_HasCorrectProperties()
    {
        var chip = CategoryChipBar.DefaultCategories[5];
        Assert.Equal("hospital", chip.CategoryId);
        Assert.Equal("Hospitals", chip.Label);
    }

    [Fact]
    public void AllChips_HaveNonEmptyIds()
    {
        foreach (var chip in CategoryChipBar.DefaultCategories)
        {
            Assert.False(string.IsNullOrEmpty(chip.CategoryId), $"Chip {chip.Label} missing id");
        }
    }

    [Fact]
    public void AllChips_HaveIcons()
    {
        foreach (var chip in CategoryChipBar.DefaultCategories)
        {
            Assert.False(string.IsNullOrEmpty(chip.Icon), $"Chip {chip.Label} missing icon");
        }
    }

    [Fact]
    public void ChipsContainer_IsConstructedWithChildren()
    {
        // Instantiating CategoryChipBar builds all chip borders in ChipsContainer
        var bar = new CategoryChipBar();
        Assert.Equal(8, bar.ChipsContainer.Children.Count);
    }

    [Fact]
    public void EachChip_HasTapGesture()
    {
        var bar = new CategoryChipBar();
        foreach (var child in bar.ChipsContainer.Children)
        {
            var border = (Border)child;
            Assert.Contains(border.GestureRecognizers, g => g is TapGestureRecognizer);
        }
    }
}
