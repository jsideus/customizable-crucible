using Refit;

namespace Crucible.Api.Client;

public interface IQACRentalApi
{
    [Post("/api/rental/cars")]
    Task<QACRentalCarsResponse> CreateQACRentalCarAsync([Body] CreateQACRentalCarRequest request);

    [Get("/api/rental/cars/{id}")]
    Task<QACRentalCarsResponse> GetQACRentalCarAsync(string id);

    [Post("/api/rental/bookings")]
    Task<QACRentalCarBookingResponse> CreateQACRentalCarBookingAsync([Body] CreateQACRentalCarBookingRequest request);

    [Get("/api/rental/bookings")]
    Task<IReadOnlyList<QACRentalCarBookingResponse>> GetMyQACRentalCarBookingsAsync();
}