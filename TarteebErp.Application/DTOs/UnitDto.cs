namespace TarteebErp.Application.DTOs;

public class UnitDto
{
    public int Id { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
}

public class CreateUnitDto
{
    public string UnitName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
}

public class UpdateUnitDto : CreateUnitDto
{
    public int Id { get; set; }
}
