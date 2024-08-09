using Manero_WebApp.Models;

namespace Manero_WebApp.Services;

public class CartService
{
    private List<CartItem> CartItems { get; set; } = new();

    public virtual void AddToCart(CartItem item)
    {
        var existingItem = CartItems.FirstOrDefault(x => x.ProductId == item.ProductId && x.Size == item.Size && x.Color == item.Color);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            CartItems.Add(item);
        }
    }

    public List<CartItem> GetCartItems()
    {
        return CartItems;
    }

    public void RemoveFromCart(string productId, string size, string color)
    {
        var item = CartItems.FirstOrDefault(x => x.ProductId == productId && x.Size == size && x.Color == color);
        if (item != null)
        {
            CartItems.Remove(item);
        }
    }

    public void ClearCart()
    {
        CartItems.Clear();
    }
}
