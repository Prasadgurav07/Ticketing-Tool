namespace Ticketing_Tool.Constants;

public static class RoleNames
{
    public const string Employee = "Employee";
    public const string SupportAgent = "Support Agent";
    public const string TeamLead = "Team Lead";
    public const string Admin = "Admin";
    public const string SupportOrAdmin = SupportAgent + "," + TeamLead + "," + Admin;
}
