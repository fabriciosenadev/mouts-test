using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class UserTestData
{
    private static readonly Faker Faker = new();

    public static User GenerateValidUser(
        UserStatus status = UserStatus.Active,
        UserRole role = UserRole.Customer)
    {
        return User.Create(
            GenerateValidUsername(),
            GenerateValidPassword(),
            new EmailAddress(GenerateValidEmail()),
            new PhoneNumber(GenerateValidPhone()),
            role,
            status);
    }

    public static string GenerateValidEmail()
    {
        return Faker.Internet.Email();
    }

    public static string GenerateValidPassword()
    {
        return $"Test@{Faker.Random.Number(100, 999)}";
    }

    public static string GenerateValidPhone()
    {
        return $"+55{Faker.Random.Number(11, 99)}{Faker.Random.Number(100000000, 999999999)}";
    }

    public static string GenerateValidUsername()
    {
        return Faker.Internet.UserName();
    }

    public static string GenerateInvalidEmail()
    {
        return Faker.Lorem.Word();
    }

    public static string GenerateInvalidPassword()
    {
        return Faker.Lorem.Word();
    }

    public static string GenerateInvalidPhone()
    {
        return Faker.Random.AlphaNumeric(5);
    }

    public static string GenerateLongUsername()
    {
        return Faker.Random.String2(51);
    }
}
