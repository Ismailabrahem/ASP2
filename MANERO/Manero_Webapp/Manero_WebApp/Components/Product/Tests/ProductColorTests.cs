using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;
using Manero_WebApp.Components.Product;
using Manero_WebApp.Components.Product.SubComponents;

public class ProductColorTests : TestContext
{
    [Fact]
    public void SelectColor_Updates_SelectedColor_And_Invokes_OnColorChanged()
    {
        // Arrange
        var colors = new List<string> { "Red", "LightBlue", "Beige", "DarkBlue", "Black" };
        var availableColors = new List<string> { "Red", "Black" };
        var selectedColor = string.Empty;

        var component = RenderComponent<ProductColor>(parameters => parameters
            .Add(p => p.AllColors, colors)
            .Add(p => p.Colors, availableColors)
            .Add(p => p.SelectedColor, selectedColor)
            .Add(p => p.OnColorChanged, EventCallback.Factory.Create<string>(this, color => selectedColor = color)));

        // Act
        var colorElement = component.FindAll("a").First(e => e.ClassList.Contains("prod-red"));
        colorElement.Click();

        // Assert
        Assert.Equal("Red", selectedColor);
        var selectedElement = component.Find("a.selected");
        Assert.Contains("prod-red", selectedElement.ClassList);
    }

}

