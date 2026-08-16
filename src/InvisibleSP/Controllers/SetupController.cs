namespace InvisibleSP.Controllers;

[ApiController]
[Route("api/setup")]
public sealed class SetupController(IMediator mediator) : ControllerBase
{
    [HttpGet("status")]
    [AllowAnonymous]
    public Task<SetupStatusResponse> GetStatus(CancellationToken cancellationToken) =>
        mediator.RequestAsync<GetSetupStatusQuery, SetupStatusResponse>(new GetSetupStatusQuery());

    [HttpPost("initialize")]
    [AllowAnonymous]
    public async Task<IActionResult> Initialize(InitializeSetupRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<InitializeSetupCommand, IdentityResultResponse>(
            new InitializeSetupCommand(request.Email, request.Password));

        return result.Succeeded ? Ok(result) : Conflict(result);
    }
}

public sealed record InitializeSetupRequest(string Email, string Password);
