using System.Text.RegularExpressions;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

public sealed record PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        Value = Normalize(value);
        Validate(Value);
    }

    public static bool TryCreate(string? value, out PhoneNumber? phoneNumber)
    {
        try
        {
            phoneNumber = new PhoneNumber(value ?? string.Empty);
            return true;
        }
        catch
        {
            phoneNumber = null;
            return false;
        }
    }

    public override string ToString() => Value;

    private static string Normalize(string value) => value.Trim();

    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The phone cannot be empty.", nameof(value));
        }

        if (!Regex.IsMatch(value, @"^\+?[1-9]\d{1,14}$"))
        {
            throw new ArgumentException("The phone format is not valid.", nameof(value));
        }
    }
}
