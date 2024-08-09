using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using Manero_WebApp.Services;
using Manero_WebApp.Data;
using Microsoft.AspNetCore.Http;

namespace YourProject.Tests
{
    public class LoginServiceTests
    {
        [Fact]
        public async Task LoginUser_SuccessfulLogin_SignInSuccessful()
        {
            // Arrange
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);

            // Mock the PasswordSignInAsync to return Success
            mockSignInManager
                .Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);

            var loginService = new LoginService(mockSignInManager.Object); // pass null for RedirectManager

            var loginInput = new LoginService.InputModel
            {
                Email = "test@example.com",
                Password = "password",
                RememberMe = true
            };
            loginService.Input = loginInput;

            // Act
            await loginService.LoginUser();

            // Assert
            mockSignInManager.Verify(m => m.PasswordSignInAsync(
                It.Is<string>(email => email == loginInput.Email),
                It.Is<string>(password => password == loginInput.Password),
                It.Is<bool>(rememberMe => rememberMe == loginInput.RememberMe),
                It.IsAny<bool>()),
                Times.Once);

            // Verify the result (though this might be done inside LoginService)
            // Here, we're just verifying that PasswordSignInAsync was called with expected parameters.
        }
    }
}
