using AuthService.Application.DTOs.Request;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly IAuthService _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _authService = new AuthServiceImpl(_userRepoMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnSuccess_WhenEmailIsNew()
        {
            // Arrange
            var request = new RegisterRequest("James", "Idakwoji", "james@test.com", "Password123!");
            _userRepoMock.Setup(r => r.ExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(new User());
            _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access_token");
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh_token");

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Registration successful.", result.Message);
        }

        [Fact]
        public async Task Register_ShouldReturnFailure_WhenEmailAlreadyExists()
        {
            // Arrange
            var request = new RegisterRequest("James", "Idakwoji", "james@test.com", "Password123!");
            _userRepoMock.Setup(r => r.ExistsAsync(request.Email)).ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email already exists.", result.Message);
        }

        [Fact]
        public async Task Login_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var request = new LoginRequest("notfound@test.com", "Password123!");
            _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid email or password.", result.Message);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, FirstName = "James", LastName = "Idakwoji", Email = "james@test.com" };
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _authService.GetUserByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(userId, result.Data!.Id);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _authService.GetUserByIdAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
        }
    }
}
