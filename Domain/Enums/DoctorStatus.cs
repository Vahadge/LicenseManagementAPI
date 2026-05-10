namespace LicenseManagementAPI.Domain.Enums;

public static class DoctorStatus
{
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Suspended = "Suspended";

    public static readonly string[] All = [Active, Expired, Suspended];

    public static bool IsValid(string status) =>
        All.Contains(status, StringComparer.OrdinalIgnoreCase);
}
