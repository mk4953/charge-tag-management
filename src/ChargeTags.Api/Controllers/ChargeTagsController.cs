using System.ComponentModel.DataAnnotations;
using ChargeTags.Api.Data;
using ChargeTags.Api.Models;
using ChargeTags.Api.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargeTags.Api.Controllers;

[ApiController]
[Route("api/chargetags")]
public class ChargeTagsController : ControllerBase
{
    private const string InvalidFormatMessage = "The tag id must have the format dk-{10 digits}-clever.";

    private readonly ChargeTagDbContext _dbContext;
    private readonly ILogger<ChargeTagsController> _logger;

    public ChargeTagsController(ChargeTagDbContext dbContext, ILogger<ChargeTagsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new charge tag.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ChargeTagResponse>> Create(CreateChargeTagRequest request, CancellationToken cancellationToken)
    {
        if (!ChargeTagIdValidator.IsValid(request.TagId))
        {
            return BadRequest(InvalidFormatMessage);
        }

        var alreadyExists = await _dbContext.ChargeTags.AnyAsync(tag => tag.Id == request.TagId, cancellationToken);
        if (alreadyExists)
        {
            return Conflict($"Charge tag {request.TagId} already exists.");
        }

        var chargeTag = new ChargeTag
        {
            Id = request.TagId,
            IsBlocked = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ChargeTags.Add(chargeTag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created charge tag {TagId}", chargeTag.Id);

        return CreatedAtAction(nameof(Verify), new { tagId = chargeTag.Id }, ToResponse(chargeTag));
    }

    /// <summary>
    /// Checks whether a charge tag exists and can be used for charging.
    /// </summary>
    /// <param name="tagId" example="dk-0123456789-clever">Format: dk-{10 digits}-clever</param>
    [HttpGet("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerifyChargeTagResponse>> Verify([Required] string tagId, CancellationToken cancellationToken)
    {
        if (!ChargeTagIdValidator.IsValid(tagId))
        {
            return BadRequest(InvalidFormatMessage);
        }

        var chargeTag = await _dbContext.ChargeTags.FindAsync(new object[] { tagId }, cancellationToken);

        var exists = chargeTag != null;
        var isBlocked = chargeTag != null && chargeTag.IsBlocked;

        var response = new VerifyChargeTagResponse
        {
            TagId = tagId,
            Exists = exists,
            IsBlocked = isBlocked,
            CanCharge = exists && !isBlocked
        };

        if (!response.CanCharge)
        {
            _logger.LogInformation("Charge tag {TagId} cannot charge (exists: {Exists}, blocked: {IsBlocked})", tagId, exists, isBlocked);
        }

        return Ok(response);
    }

    /// <summary>
    /// Blocks a charge tag, so it can no longer be used for charging.
    /// </summary>
    /// <param name="tagId" example="dk-0123456789-clever">Format: dk-{10 digits}-clever</param>
    [HttpPost("{tagId}/block")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChargeTagResponse>> Block(string tagId, CancellationToken cancellationToken)
    {
        if (!ChargeTagIdValidator.IsValid(tagId))
        {
            return BadRequest(InvalidFormatMessage);
        }

        var chargeTag = await _dbContext.ChargeTags.FindAsync(new object[] { tagId }, cancellationToken);
        if (chargeTag == null)
        {
            return NotFound($"Charge tag {tagId} does not exist.");
        }

        // Blocking an already blocked tag just returns it, so the call is safe to repeat
        if (!chargeTag.IsBlocked)
        {
            chargeTag.IsBlocked = true;
            chargeTag.BlockedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Blocked charge tag {TagId}", tagId);
        }

        return Ok(ToResponse(chargeTag));
    }

    private static ChargeTagResponse ToResponse(ChargeTag chargeTag)
    {
        return new ChargeTagResponse
        {
            TagId = chargeTag.Id,
            IsBlocked = chargeTag.IsBlocked,
            CreatedAt = chargeTag.CreatedAt,
            BlockedAt = chargeTag.BlockedAt
        };
    }
}
