namespace LicenseManagementAPI.Common.Helpers;

// Superseded by Infrastructure/Security/PasswordHasher.cs (implements IPasswordHasher).
// This class is kept to avoid breaking any external references but contains no logic.
[Obsolete("Use IPasswordHasher instead.")]
public static class PasswordHelper { }
