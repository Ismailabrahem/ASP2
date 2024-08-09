using Bunit;
using Xunit;
using Manero_WebApp.Components.Product.SubComponents;

public class ProductSizeTests : TestContext
{
    [Fact]
    public void ProductSize_ChangesSelectedSize_WhenOptionSelected()
    {
        // Arrange
        var sizes = new List<string> { "S", "M", "L", "XL" };
        var allSizes = new List<string> { "XS", "S", "M", "L", "XL", "XXL" };
        var component = RenderComponent<ProductSize>(parameters => parameters
            .Add(p => p.Sizes, sizes)
            .Add(p => p.AllSizes, allSizes)
            .Add(p => p.SelectedSize, "M")
        );

        // Act
        var sizeLink = component.FindAll("a").FirstOrDefault(a => a.TextContent == "L");
        sizeLink!.Click();

        // Assert
        var updatedSizeLink = component.Find("a.selected");
        Assert.Equal("L", updatedSizeLink.TextContent);
    }
}
