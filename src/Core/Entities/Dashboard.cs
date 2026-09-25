using Core.Contracts.Services;

namespace Core.Entities;

public class Dashboard
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string LayoutType { get; set; } = "grid";
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<DashboardWidget> Widgets { get; set; } = [];

    public bool Update(DashboardRequest request, bool create = false)
    {
        var old = (Name, Description, LayoutType, IsDefault, DisplayOrder);
        if (create || request.NameSpecified) Name = DashboardValidation.Text(request.Name, "name", "INVALID_DASHBOARD_NAME", 120, true)!;
        if (request.DescriptionSpecified) Description = DashboardValidation.Text(request.Description, "description", "INVALID_DASHBOARD_DESCRIPTION", 255, trim: false);
        if (request.LayoutTypeSpecified) LayoutType = request.LayoutType ?? "grid";
        if (LayoutType != "grid") throw DashboardValidation.Invalid("INVALID_LAYOUT_TYPE", "layoutType");
        if (request.IsDefaultSpecified) IsDefault = request.IsDefault ?? false;
        if (request.DisplayOrderSpecified) DisplayOrder = request.DisplayOrder ?? 0;
        DashboardValidation.Number(DisplayOrder, "displayOrder", "INVALID_DISPLAY_ORDER", 0, int.MaxValue);
        var changed = old != (Name, Description, LayoutType, IsDefault, DisplayOrder);
        if (create) CreatedAt = DashboardValidation.UtcNow();
        else if (changed) UpdatedAt = DashboardValidation.UtcNow();
        return create || changed;
    }
}
