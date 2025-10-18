using System.Text.RegularExpressions;
using TravelPlanner.Application.Models;

namespace TravelPlanner.Application.Validation;

public static class CreateTripRequestValidator
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static void ValidateAndThrow(CreateTripRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.OwnerEmail) || !EmailRegex.IsMatch(request.OwnerEmail))
            AddError(errors, nameof(request.OwnerEmail), "Valid email is required.");

        if (request.StartDate == default)
            AddError(errors, nameof(request.StartDate), "StartDate is required.");

        if (request.EndDate == default)
            AddError(errors, nameof(request.EndDate), "EndDate is required.");

        if (request.EndDate <= request.StartDate)
            AddError(errors, nameof(request.EndDate), "EndDate must be after StartDate.");

        if (request.Budget < 0)
            AddError(errors, nameof(request.Budget), "Budget must be non-negative.");

        if (request.Destinations == null || request.Destinations.Count == 0)
        {
            AddError(errors, nameof(request.Destinations), "At least one destination is required.");
        }
        else
        {
            for (int i = 0; i < request.Destinations.Count; i++)
            {
                var d = request.Destinations[i];
                if (string.IsNullOrWhiteSpace(d.City))
                    AddError(errors, $"Destinations[{i}].City", "City is required.");
                if (string.IsNullOrWhiteSpace(d.Country))
                    AddError(errors, $"Destinations[{i}].Country", "Country is required.");
            }
        }

        if (errors.Count > 0)
        {
            var dict = errors.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray());
            throw new ValidationException(dict);
        }
    }

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var list))
        {
            list = new List<string>();
            errors[key] = list;
        }
        list.Add(message);
    }
}

