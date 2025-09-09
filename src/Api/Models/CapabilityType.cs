using System.Linq;
using System.Collections.Generic;

namespace Api.Models;

public record class CapabilityType(
    string name,
    string actuator_mode,
    string data_type,
    bool computed_value,
    string? value_symbol,
    string? active_value = null,
    List<CapabilityIcon>? icons = null)
{
    public static implicit operator CapabilityType(Core.Entities.CapabilityType capabilityType)
        => new(
            capabilityType.Name,
            capabilityType.ActuatorMode,
            capabilityType.DataType,
            capabilityType.ComputedValue,
            capabilityType.ValueSymbol,
            capabilityType.ActiveValue,
            capabilityType.Icons?.Select(i => new CapabilityIcon(i.Name, i.PrimaryColor, i.SecondaryColor)).ToList()
        );

    public static implicit operator Core.Entities.CapabilityType(CapabilityType capabilityType)
    {
        var entity = new Core.Entities.CapabilityType(
            capabilityType.name,
            capabilityType.actuator_mode,
            capabilityType.data_type,
            capabilityType.computed_value
        )
        {
            ActiveValue = capabilityType.active_value,
            ValueSymbol = capabilityType.value_symbol
        };

        if (capabilityType.icons is not null)
        {
            entity.Icons = capabilityType.icons
                .Select(i => new Core.Entities.CapabilityIcon(i.name, i.primary_color, i.secondary_color))
                .ToList();
        }

        return entity;
    }
}