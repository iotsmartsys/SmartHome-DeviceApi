using System.Text.Json;
using Api.Models;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

[Route("api/v1/groups")]
[ApiController]
public class GroupController(ILogger<GroupController> logger) : ControllerBase
{
    [HttpPost()]
    [ProducesResponseType(typeof(Group), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> AddGroupAsync(
        [FromBody] GroupCreateRequest? request,
        [FromServices] IGroupRepository repository,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            ModelState.AddModelError("request", "O payload de criação é obrigatório.");
            return ValidationProblem(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.name))
            ModelState.AddModelError(nameof(request.name), "name é obrigatório.");

        if (request.icon is not null && string.IsNullOrWhiteSpace(request.icon.name))
            ModelState.AddModelError("icon.name", "icon.name deve ser preenchido quando icon for informado.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var normalizedRequest = request with
        {
            name = request.name!.Trim(),
            icon = request.icon is null ? null : new GroupIconRequest(request.icon.name!.Trim())
        };

        logger.LogInformation("Request de adição de grupo {Group}", normalizedRequest);
        var entity = normalizedRequest.ToEntity();
        await repository.AddAsync(entity, cancellationToken);
        logger.LogInformation("Grupo adicionado com sucesso");

        return Ok((Group)entity);
    }

    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Group>))]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAllGroupsAsync([FromServices] IGroupRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request de busca de todos os grupos");
        var groups = await repository.GetAllAsync(cancellationToken);
        return Ok(groups.Select(g => (Group)g).ToArray());
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateGroupAsync(int id, [FromBody] Group group, [FromServices] IGroupRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request de atualização do grupo {id}", id);
        if (id != group.id)
        {
            logger.LogWarning("ID do grupo na URL ({id}) não corresponde ao ID do grupo no corpo da requisição ({groupId})", id, group.id);
            return NotFound();
        }
        var entity = (Core.Entities.Group)group;
        entity.Id = id;
        await repository.UpdateAsync(entity, cancellationToken);
        logger.LogInformation("Grupo {id} atualizado com sucesso", id);
        return NoContent();
    }

    [HttpPatch("{id}")]
    [Consumes("application/json-patch+json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdatePatchGroupAsync(
        int id,
        [FromBody] JsonPatchDocument<GroupPatchRequest>? patch,
        [FromServices] IGroupRepository repository,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Request de patch do grupo {id}", id);

        if (id <= 0)
            return BadRequestProblem("O id do Group deve ser positivo.");

        if (patch is null || patch.Operations.Count == 0)
        {
            logger.LogWarning("JsonPatchDocument inválido fornecido para o grupo {id}", id);
            return BadRequestProblem("O documento JSON Patch deve conter ao menos uma operação.");
        }

        foreach (var operation in patch.Operations)
        {
            if (!string.Equals(operation.op, "replace", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("patch.op", "Somente operações replace são aceitas.");

            if (!AllowedPatchPaths.Contains(operation.path))
                ModelState.AddModelError("patch.path", $"O caminho '{operation.path}' não é permitido.");
            else if (!HasExpectedValue(operation.path, operation.value))
                ModelState.AddModelError("patch.value", $"O valor informado para '{operation.path}' é inválido.");
        }

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var existingGroup = await repository.GetByIdAsync(id, cancellationToken);
        if (existingGroup == null)
        {
            logger.LogWarning("Grupo com ID {id} não encontrado", id);
            return NotFoundProblem(id);
        }

        var model = GroupPatchRequest.FromEntity(existingGroup);
        patch.ApplyTo(model, ModelState);

        var changedPaths = patch.Operations
            .Select(operation => operation.path)
            .ToHashSet(StringComparer.Ordinal);

        if (changedPaths.Contains("/name"))
        {
            if (string.IsNullOrWhiteSpace(model.name))
                ModelState.AddModelError(nameof(model.name), "name é obrigatório.");
            else
                model.name = model.name.Trim();
        }

        if (changedPaths.Contains("/icon") && model.icon is not null)
        {
            if (string.IsNullOrWhiteSpace(model.icon.name))
                ModelState.AddModelError("icon.name", "icon.name deve ser preenchido quando icon for informado.");
            else
                model.icon = new GroupIconRequest(model.icon.name.Trim());
        }

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        logger.LogInformation(JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true }));

        var updated = await repository.UpdateAdministrativeAsync(
            id,
            changedPaths.Contains("/name") ? model.name : null,
            changedPaths.Contains("/active") ? model.active : null,
            model.icon?.name,
            changedPaths.Contains("/icon"),
            cancellationToken);

        if (!updated)
            return NotFoundProblem(id);

        logger.LogInformation("Grupo {id} atualizado com sucesso", id);
        return NoContent();
    }



    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DeleteGroupAsync(int id, [FromServices] IGroupRepository repository, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequestProblem("O id do Group deve ser positivo.");

        var deleted = await repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFoundProblem(id);

        logger.LogInformation("Grupo com ID {id} excluído com sucesso", id);
        return NoContent();
    }

    [HttpPut("{groupId}/capabilities/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddCapabilityToGroupAsync(int groupId, [FromBody] CapabilityGroup capability, [FromServices] IGroupRepository repository, CancellationToken cancellationToken)
    {
        if (groupId <= 0)
        {
            logger.LogWarning("ID de grupo inválido fornecido: {groupId}", groupId);
            return BadRequest("ID de grupo inválido fornecido.");
        }
        var existingGroup = await repository.GetByIdAsync(groupId, cancellationToken);
        if (existingGroup == null)
        {
            logger.LogWarning("Grupo com ID {groupId} não encontrado para adição de capability", groupId);
            return NotFound($"Grupo com ID {groupId} não encontrado.");
        }

        logger.LogInformation("Request de adição de capability {capability} ao grupo {groupId}", capability, groupId);
        if (capability == null || string.IsNullOrWhiteSpace(capability.capability_name))
        {
            logger.LogWarning("Capability inválida ou sem nome fornecida para o grupo {groupId}", groupId);
            return BadRequest("Capability inválida ou sem nome.");
        }

        Core.Entities.CapabilityGroup entityCapability = (Core.Entities.CapabilityGroup)capability;
        await repository.AddCapabilityToGroupAsync(groupId, entityCapability, cancellationToken);
        logger.LogInformation("Capability {capability} adicionada ao grupo {groupId} com sucesso", capability, groupId);
        return NoContent();
    }

    [HttpDelete("{groupId}/capabilities/{capabilityId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCapabilityFromGroupAsync(int groupId, int capabilityId, [FromServices] IGroupRepository repository, CancellationToken cancellationToken)
    {
        if(groupId <= 0 || capabilityId <= 0)
        {
            logger.LogWarning("IDs inválidos fornecidos para exclusão de capability do grupo: groupId={groupId}, capabilityId={capabilityId}", groupId, capabilityId);
            return BadRequest("IDs inválidos fornecidos.");
        }
        var existingGroup = await repository.GetByIdAsync(groupId, cancellationToken);
        if (existingGroup == null)
        {
            logger.LogWarning("Grupo com ID {groupId} não encontrado para exclusão de capability", groupId);
            return NotFound($"Grupo com ID {groupId} não encontrado.");
        }

        logger.LogInformation("Request de exclusão da capability {capabilityId} do grupo {groupId}", capabilityId, groupId);
        await repository.DeleteCapabilityForGroupAsync(groupId, capabilityId, cancellationToken);
        logger.LogInformation("Capability {capabilityId} excluída do grupo {groupId} com sucesso", capabilityId, groupId);
        return NoContent();
    }

    private static readonly HashSet<string> AllowedPatchPaths =
    [
        "/name",
        "/active",
        "/icon"
    ];

    private static bool HasExpectedValue(string path, object? value) => path switch
    {
        "/name" => value is string || value is JValue { Type: JTokenType.String },
        "/active" => value is bool || value is JValue { Type: JTokenType.Boolean },
        "/icon" => value is null || value is JObject || value is GroupIconRequest,
        _ => false
    };

    private ObjectResult BadRequestProblem(string detail) => Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: "Requisição inválida",
        detail: detail);

    private ObjectResult NotFoundProblem(int id) => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Group não encontrado",
        detail: $"Grupo com ID {id} não encontrado.");
}
