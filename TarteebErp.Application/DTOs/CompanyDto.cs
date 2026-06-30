namespace TarteebErp.Application.DTOs;

public class CompanyDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Logo { get; set; }
    public string? TaxNumber { get; set; }
}

public class CreateCompanyDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Logo { get; set; }
    public string? TaxNumber { get; set; }
}

public class UpdateCompanyDto : CreateCompanyDto
{
    public int Id { get; set; }
}