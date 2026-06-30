using System.Text.RegularExpressions;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

public sealed record EmailAddress
{
    private static readonly Regex ValidationRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

    public string Value { get; }

    public EmailAddress(string value)
    {
        Value = Normalize(value);
        Validate(Value);
    }

    public static bool TryCreate(string? value, out EmailAddress? emailAddress)
    {
        try
        {
            emailAddress = new EmailAddress(value ?? string.Empty);
            return true;
        }
        catch
        {
            emailAddress = null;
            return false;
        }
    }

    public override string ToString() => Value;

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The email address cannot be empty.", nameof(value));
        }

        if (value.Length > 100)
        {
            throw new ArgumentException("The email address cannot be longer than 100 characters.", nameof(value));
        }

        if (!ValidationRegex.IsMatch(value))
        {
            throw new ArgumentException("The provided email address is not valid.", nameof(value));
        }
    }
}
