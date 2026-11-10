namespace Admission.Api.Domain;

// Names are upper-case on purpose: they are stored in the DB and returned by the API as-is.
public enum HoldStatus
{
    HELD,
    CONFIRMED,
    EXPIRED,
    CANCELLED
}
