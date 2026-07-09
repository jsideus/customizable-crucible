using System.Text.Json.Serialization;

namespace Crucible.Api.Client;

public sealed record QACRentalCarsResponse
{
    public required string Id { get; init; }
    public required string OwnerId { get; init; }
    public required string Make { get; init; }
    public required string Model { get; init; }
    public required int Year { get; init; }
    public required string Color { get; init; }
    public required string LicensePlate { get; init; }
    public required int DailyRateCents { get; init; }
    public required decimal TankCapacityGallons { get; init; }
    public required int MileageKm { get; init; }
    public required string Status { get; init; }
    public required string Description { get; init; }
    public required string ImageUrl { get; init; }
    public string? DeletedAt { get; init; }
    public required string CreatedAt { get; init; }
    public required string UpdatedAt { get; init; }
}