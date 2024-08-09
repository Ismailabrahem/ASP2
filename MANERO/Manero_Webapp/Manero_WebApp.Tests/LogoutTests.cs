using Xunit;
using System.Threading.Tasks;
using Moq;
using System.Reflection;
using Manero_WebApp.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

public class LogoutTests
{
    [Fact]
    public async Task Logout_ShouldRedirectToProductHome()
    {
        // Arrange
        var mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            Mock.Of<UserManager<ApplicationUser>>(),
            null, null, null, null, null, null);
        mockSignInManager.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

        var navigationManager = new Mock<NavigationManager>();
        navigationManager.Setup(n => n.NavigateTo("/producthome", false));

        var programType = typeof(Program);
        var programInstance = Activator.CreateInstance(programType);

        var logoutMethod = programType.GetMethod("Logout", BindingFlags.Static | BindingFlags.NonPublic);

        // Act
        await (Task)logoutMethod.Invoke(programInstance, new object[] { mockSignInManager.Object, navigationManager.Object });

        // Assert
        navigationManager.Verify(n => n.NavigateTo("/producthome", false), Times.Once);
    }
}