using System.Text.Json.Serialization;

namespace Crucible.Api.Client;

public sealed record CreateQACRentalCarBookingRequest
{
    public required string CarId { get; init; }
    public required string StartDate { get; init; }
    public required string EndDate { get; init; }
    public string? InsuranceType { get; init; }
    public List<string>? AddOnIds { get; init; }
    public string? PaymentMethodId { get; init; }
    public string? Notes { get; init; }
}