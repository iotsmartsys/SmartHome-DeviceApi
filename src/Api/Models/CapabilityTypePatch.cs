using System.Collections.Generic;

namespace Api.Models;

public record class CapabilityTypePatch(
    string? name,
    string? actuator_mode,
    string? data_type,
    bool? computed_value,
    IEnumerable<CapabilityIcon>? icons
);

