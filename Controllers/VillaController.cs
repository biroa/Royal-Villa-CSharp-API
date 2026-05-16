using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVillaApi.Data;
using RoyalVillaApi.Models;

namespace RoyalVillaApi.Controllers;

[ApiController]
[Route("api/villa")]
[ApiExplorerSettings(GroupName = "v1")]
public class VillaController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public VillaController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    ///     GET all villas from the database and return them as JSON.
    /// </summary>
    /// <remarks>
    ///     The return type is built from three nested concepts:
    ///     <para>
    ///         <b>Task</b> — Marks this action as asynchronous. The method can await I/O (here,
    ///         <c>ToListAsync</c>) without blocking a thread while the database responds. Callers
    ///         receive a <see cref="Task{TResult}"/> that completes when the query finishes and
    ///         the HTTP response is ready.
    ///     </para>
    ///     <para>
    ///         <b>ActionResult</b> — An ASP.NET Core wrapper for an HTTP outcome: status code,
    ///         headers, and optional body. <see cref="ControllerBase.Ok(object?)"/> produces
    ///         200 OK with a serialized payload; other helpers (e.g. NotFound, BadRequest) set
    ///         different status codes without changing the action’s signature.
    ///     </para>
    ///     <para>
    ///         <b>IEnumerable{Villa}</b> — The shape of the success payload: a sequence of
    ///         <see cref="Villa"/> records. The serializer turns this into a JSON array. EF Core
    ///         materializes the query with <c>ToListAsync</c> before returning, so the client
    ///         gets a concrete list rather than a deferred database cursor.
    ///     </para>
    ///     Read together: <c>Task&lt;ActionResult&lt;IEnumerable&lt;Villa&gt;&gt;&gt;</c> means
    ///     “eventually return an HTTP result whose 200 body is a collection of villas.”
    /// </remarks>
    /// <returns>200 OK with all villas when the query succeeds.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Villa>>> GetVillas()
    {
        return Ok(await _dbContext.Villas.ToListAsync());
    }
}
