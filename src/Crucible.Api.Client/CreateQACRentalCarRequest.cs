using System.Text.Json.Serialization;

namespace Crucible.Api.Client;

public sealed record CreateQACRentalCarRequest
{
    [JsonPropertyName("make")]
    public required string Make { get; init; }

    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("year")]
    public required int Year { get; init; }

    [JsonPropertyName("color")]
    public required string Color { get; init; }

    [JsonPropertyName("license_plate")]
    public required string LicensePlate { get; init; }

    [JsonPropertyName("daily_rate_cents")]
    public required int DailyRateCents { get; init; }

    [JsonPropertyName("tank_capacity_gallons")]
    public required decimal TankCapacityGallons { get; init; }

    [JsonPropertyName("mileage_km")]
    public required int MileageKm { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("image_url")]
    public required string ImageUrl { get; init; }
}