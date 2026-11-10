using ChargeTags.Api.Controllers;
using ChargeTags.Api.Data;
using ChargeTags.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChargeTags.Api.Tests;

public class ChargeTagsControllerTests
{
    private const string TagId = "dk-0123456789-clever";

    [Test]
    public async Task Create_ReturnsConflict_WhenTagAlreadyExists()
    {
        var controller = CreateController();
        var request = new CreateChargeTagRequest { TagId = TagId };
        await controller.Create(request, CancellationToken.None);

        var result = await controller.Create(request, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ConflictObjectResult>());
    }

    [Test]
    public async Task Verify_ReturnsCannotCharge_AfterTagIsBlocked()
    {
        var controller = CreateController();
        await controller.Create(new CreateChargeTagRequest { TagId = TagId }, CancellationToken.None);
        await controller.Block(TagId, CancellationToken.None);

        var result = await controller.Verify(TagId, CancellationToken.None);

        var response = GetOkValue(result);
        Assert.That(response.Exists, Is.True);
        Assert.That(response.IsBlocked, Is.True);
        Assert.That(response.CanCharge, Is.False);
    }

    [Test]
    public async Task Verify_ReturnsNotExisting_ForUnknownTag()
    {
        var controller = CreateController();

        var result = await controller.Verify(TagId, CancellationToken.None);

        var response = GetOkValue(result);
        Assert.That(response.Exists, Is.False);
        Assert.That(response.CanCharge, Is.False);
    }

    // Each test gets its own empty in-memory database, so the tests do not affect each other
    private static ChargeTagsController CreateController()
    {
        var options = new DbContextOptionsBuilder<ChargeTagDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ChargeTagDbContext(options);
        return new ChargeTagsController(dbContext, NullLogger<ChargeTagsController>.Instance);
    }

    private static VerifyChargeTagResponse GetOkValue(ActionResult<VerifyChargeTagResponse> result)
    {
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        return (VerifyChargeTagResponse)okResult.Value!;
    }
}
