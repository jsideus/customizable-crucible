using System.Text.Json.Serialization;

namespace Crucible.Api.Client;

public sealed record QACRentalCarBookingResponse
{
    public required string Id { get; init; }
    public required string CarId { get; init; }
    public required string OwnerId { get; init; }
    public required string StartDate { get; init; }
    public required string EndDate { get; init; }
    public required string InsuranceType { get; init; }
    public required string Status { get; init; }
    public required int FuelPickupPercent { get; init; }
    public required string Notes { get; init; }
    public required string CreatedAt { get; init; }
}