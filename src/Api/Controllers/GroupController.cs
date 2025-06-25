using System.Text.Json;
using Api.Models;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/groups")]
[ApiController]
public class GroupController(ILogger<GroupController> logger) : ControllerBase
{
    [HttpPost()]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddGroupAsync([FromBody] Group group, [FromServices] IGroupRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request de adição de grupo {group}", group);
        var entity = (Core.Entities.Group)group;
        await repository.AddAsync(entity, cancellationToken);
        logger.LogInformation("Grupo adicionado com sucesso");

        return Ok((Group)entity);
    }

    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Group>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllGroupsAsync([FromServices] IGroupRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request de busca de todos os grupos");
        var groups = await repository.GetAllAsync(cancellationToken);
        if (groups.Any())
            return Ok(groups.Select(g => (Group)g));

        return NoContent();
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePatchGroupAsync(int id, [FromBody] JsonPatchDocument<Group> group, [FromServices] IGroupRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request de patch do grupo {id}", id);
        if (group == null)
        {
            logger.LogWarning("JsonPatchDocument inválido fornecido para o grupo {id}", id);
            return BadRequest("JsonPatchDocument inválido.");
        }

        var existingGroup = await repository.GetByIdAsync(id, cancellationToken);
        if (existingGroup == null)
        {
            logger.LogWarning("Grupo com ID {id} não encontrado", id);
            return NotFound($"Grupo com ID {id} não encontrado.");
        }

        var model = (Group)existingGroup;
        group.ApplyTo(model);
        existingGroup = (Core.Entities.Group)model;
        existingGroup.Id = id;

        logger.LogInformation(JsonSerializer.Serialize(existingGroup, new JsonSerializerOptions { WriteIndented = true }));

        await repository.UpdateOnlyGroupAsync(existingGroup, cancellationToken);
        logger.LogInformation("Grupo {id} atualizado com sucesso", id);
        return NoContent();
    }



    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteGroupAsync(int id, [FromServices] IGroupRepository repository, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(id, cancellationToken);
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
}