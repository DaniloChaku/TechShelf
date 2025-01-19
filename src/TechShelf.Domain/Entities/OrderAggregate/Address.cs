﻿namespace TechShelf.Domain.Entities.OrderAggregate;

public record Address
{
    public readonly static IReadOnlyList<string> AllowedCountries = ["US"];

    public string Country { get; private set; }
    public string AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; }
    public string Region { get; private set; }
    public string PostalCode { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Address() { } // For EF Core
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public Address(string country, string addressLine1, string? addressLine2,
        string city, string region, string postalCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(country);
        ArgumentException.ThrowIfNullOrWhiteSpace(addressLine1);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(region);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);

        if (!AllowedCountries.Contains(country))
        {
            throw new ArgumentException("Country not supported");
        }

        Country = country;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        Region = region;
        PostalCode = postalCode;
    }
}