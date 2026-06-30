namespace TarteebErp.Application.DTOs;

public class BrandDto
{
    public int Id { get; set; }
    public string BrandName { get; set; } = string.Empty;
}

public class CreateBrandDto
{
    public string BrandName { get; set; } = string.Empty;
}

public class UpdateBrandDto : CreateBrandDto
{
    public int Id { get; set; }
}
