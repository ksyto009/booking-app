namespace CourtBooking.Domain.Catalog;

using CourtBooking.Domain.Common;

public sealed record OpeningHours
{
    public TimeOnly Open { get; init; }
    public TimeOnly Close { get; init; }

    private OpeningHours(TimeOnly open, TimeOnly close)
    {
        Open = open;
        Close = close;
    }

    public static OpeningHours Create(TimeOnly open, TimeOnly close)
    {
        if (close <= open)
            throw new DomainException("Giờ đóng cửa phải sau giờ mở cửa");

        return new OpeningHours(open, close);
    }

    public static OpeningHours Default => Create(new TimeOnly(5, 0), new TimeOnly(23, 0));
}