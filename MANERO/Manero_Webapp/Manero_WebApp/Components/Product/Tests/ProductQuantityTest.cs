using Bunit;
using Manero_WebApp.Components.Product.SubComponents;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Manero_WebApp.Components.Product.Tests;

public class ProductQuantityTests : TestContext
{
    [Fact]
    public void IncreaseQuantity_Updates_Quantity()
    {
        // Arrange
        var initialQuantity = 1;
        var component = RenderComponent<ProductQuantity>(parameters => parameters
            .Add(p => p.Quantity, initialQuantity)
            .Add(p => p.QuantityChanged, EventCallback.Factory.Create<int>(this, q => initialQuantity = q)));

        // Act
        var increaseButton = component.Find("button:last-of-type");
        increaseButton.Click();

        // Assert
        Assert.Equal(2, initialQuantity);
    }

    [Fact]
    public void DecreaseQuantity_DoesNotDecreaseBelowOne()
    {
        // Arrange
        var initialQuantity = 1;
        var component = RenderComponent<ProductQuantity>(parameters => parameters
            .Add(p => p.Quantity, initialQuantity)
            .Add(p => p.QuantityChanged, EventCallback.Factory.Create<int>(this, q => initialQuantity = q)));

        // Act
        var decreaseButton = component.Find("button:first-of-type");
        decreaseButton.Click();

        // Assert
        Assert.Equal(1, initialQuantity);
    }

    [Fact]
    public void DecreaseQuantity_Updates_Quantity()
    {
        // Arrange
        var initialQuantity = 2;
        var component = RenderComponent<ProductQuantity>(parameters => parameters
            .Add(p => p.Quantity, initialQuantity)
            .Add(p => p.QuantityChanged, EventCallback.Factory.Create<int>(this, q => initialQuantity = q)));

        // Act
        var decreaseButton = component.Find("button:first-of-type");
        decreaseButton.Click();

        // Assert
        Assert.Equal(1, initialQuantity);
    }
}
