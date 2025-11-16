namespace VetCareAPI.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string ClinicStaff = "ClinicStaff";
    public const string User = "User";

    public static readonly string[] All = [Admin, ClinicStaff, User];

    public static bool IsValid(string role) => All.Contains(role);
}