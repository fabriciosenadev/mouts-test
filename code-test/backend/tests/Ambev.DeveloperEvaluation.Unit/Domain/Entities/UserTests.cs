using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class UserTests
{
    [Fact(DisplayName = "User status should change to Active when activated")]
    public void Given_SuspendedUser_When_Activated_Then_StatusShouldBeActive()
    {
        var user = UserTestData.GenerateValidUser(UserStatus.Suspended);

        user.Activate();

        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact(DisplayName = "User status should change to Suspended when suspended")]
    public void Given_ActiveUser_When_Suspended_Then_StatusShouldBeSuspended()
    {
        var user = UserTestData.GenerateValidUser(UserStatus.Active);

        user.Suspend();

        user.Status.Should().Be(UserStatus.Suspended);
    }

    [Fact(DisplayName = "Validation should pass for valid user data")]
    public void Given_ValidUserData_When_Validated_Then_ShouldReturnValid()
    {
        var user = UserTestData.GenerateValidUser();

        var result = user.Validate();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Validation should fail for default user data")]
    public void Given_DefaultUser_When_Validated_Then_ShouldReturnInvalid()
    {
        var user = new Ambev.DeveloperEvaluation.Domain.Entities.User();

        var result = user.Validate();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "UpdateProfile should normalize value objects and refresh update timestamp")]
    public void Given_ValidProfileData_When_Updating_Then_ShouldNormalizeAndStamp()
    {
        var user = UserTestData.GenerateValidUser();
        var previousUpdatedAt = user.UpdatedAt;

        user.UpdateProfile(
            "updated.user",
            new EmailAddress(" Updated.User@Example.com "),
            new PhoneNumber(" +5511999999999 "),
            UserRole.Admin,
            UserStatus.Active);

        user.Username.Should().Be("updated.user");
        user.Email.Should().Be("updated.user@example.com");
        user.Phone.Should().Be("+5511999999999");
        user.Role.Should().Be(UserRole.Admin);
        user.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }
}
