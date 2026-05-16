using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVillaApi.Data;
using RoyalVillaApi.Models;
using RoyalVillaApi.Models.DTO;

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

    /// <summary>
    ///     GET a single villa by its numeric id from the route, or return an error status.
    /// </summary>
    /// <remarks>
    ///     <b>Route and binding</b>
    ///     <para>
    ///         <c>[HttpGet("{id:int}")]</c> — HTTP GET on <c>api/villa/{id}</c>. The
    ///         <c>{id:int}</c> segment is a <b>route constraint</b>: only integer values match;
    ///         non-numeric paths (e.g. <c>api/villa/abc</c>) do not hit this action and typically
    ///         yield 404 from the framework. The matched value is bound to the <paramref name="id"/>
    ///         parameter (similar to path variables in Express, Flask, or Spring).
    ///     </para>
    ///     <para>
    ///         <c>[ProducesResponseType(...)]</c> — Documents possible HTTP status codes for
    ///         OpenAPI/Swagger; they do not change runtime behavior. Clients and tools use them to
    ///         know which responses to expect.
    ///     </para>
    ///     <b>Return type (read inside-out)</b>
    ///     <para>
    ///         <b>Task</b> — The action is asynchronous; <c>await</c> yields the thread while
    ///         the database runs. The caller gets a <see cref="Task{TResult}"/> that completes when
    ///         the HTTP response is ready.
    ///     </para>
    ///     <para>
    ///         <b>ActionResult</b> — Wraps the HTTP outcome (status, headers, body).
    ///         <see cref="ControllerBase.Ok(object?)"/> → 200,
    ///         <see cref="ControllerBase.BadRequest(object?)"/> → 400,
    ///         <see cref="ControllerBase.NotFound(object?)"/> → 404,
    ///         <see cref="ControllerBase.StatusCode(int, object?)"/> → arbitrary code (here 500).
    ///         One method signature can return different status codes via these helpers.
    ///     </para>
    ///     <para>
    ///         <b>Villa</b> — On success, the 200 body is one <see cref="Villa"/> entity serialized
    ///         to JSON (not a list). <c>Task&lt;ActionResult&lt;Villa&gt;&gt;</c> means “eventually
    ///         return an HTTP result whose success body is a single villa.”
    ///     </para>
    ///     <b>Keywords and patterns in the method body</b>
    ///     <para>
    ///         <c>async</c> / <c>await</c> — <c>await</c> waits for a <see cref="Task"/> without
    ///         blocking; use it on I/O such as EF Core’s <c>FirstOrDefaultAsync</c> and
    ///         <c>FindAsync</c>. Do not use <c>await</c> on purely CPU-bound work.
    ///     </para>
    ///     <para>
    ///         <c>var</c> — Local type inference; the compiler picks the type (here
    ///         <see cref="Villa"/> or <c>null</c>). Equivalent to writing the type explicitly.
    ///     </para>
    ///     <para>
    ///         <c>FirstOrDefaultAsync(v =&gt; v.Id == id)</c> — EF Core translates the lambda into
    ///         SQL <c>WHERE Id = @id</c> and returns the first row or <c>null</c> if none (unlike
    ///         <c>FirstAsync</c>, which throws when empty). <c>v =&gt; ...</c> is a lambda
    ///         (anonymous function), like <c>x =&gt; x.Id == id</c> in JavaScript or a Python
    ///         <c>lambda</c>.
    ///     </para>
    ///     <para>
    ///         <c>FindAsync(id)</c> — Looks up by primary key; may return a tracked entity from
    ///         memory if it was already loaded in this request. This action calls both
    ///         <c>FirstOrDefaultAsync</c> (existence check) and <c>FindAsync</c> (response body);
    ///         after a successful <c>FirstOrDefaultAsync</c>, <c>FindAsync</c> often resolves from
    ///         the change tracker without a second database round-trip.
    ///     </para>
    ///     <para>
    ///         <c>if (villa == null)</c> — Reference types (classes) use <c>null</c> for “missing”;
    ///         value types like <c>int</c> cannot be null unless declared as <c>int?</c>.
    ///     </para>
    ///     <para>
    ///         <c>$"..."</c> — String interpolation; expressions in <c>{ }</c> are formatted into
    ///         the string (like f-strings in Python or template literals in JavaScript).
    ///     </para>
    ///     <para>
    ///         <c>try</c> / <c>catch (Exception ex)</c> — Catches any exception from the try block;
    ///         <c>ex.Message</c> is the error text. Unhandled exceptions would otherwise become a
    ///         generic 500; here failures are returned as 500 with a message body.
    ///     </para>
    ///     <b>Typical questions from other languages</b>
    ///     <para>
    ///         <b>Why validate <c>id &lt;= 0</c> if the route is <c>{id:int}</c>?</b> — The route
    ///         only guarantees an integer, not a positive one. <c>api/villa/0</c> or
    ///         <c>api/villa/-1</c> still reach this action; the check returns 400 for invalid
    ///         business rules.
    ///     </para>
    ///     <para>
    ///         <b>Why return <c>ActionResult&lt;Villa&gt;</c> instead of <c>Villa</c>?</b> — So the
    ///         same method can return 200 with a body or 400/404/500 without exceptions as control
    ///         flow. Returning only <c>Villa</c> would always imply 200 unless you throw.
    ///     </para>
    ///     <para>
    ///         <b>Is <c>return Ok(...)</c> the HTTP response?</b> — Yes. The framework converts it
    ///         to a response with status 200 and JSON body; you do not write headers manually.
    ///     </para>
    /// </remarks>
    /// <param name="id">Primary key of the villa; must be greater than zero.</param>
    /// <returns>200 OK with the villa, 400 if id is invalid, 404 if not found, 500 on failure.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Villa>> GetVillaById(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("Id is required");
            }
            
            var villa = await _dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);           
            if (villa == null)
            {
                return NotFound($"Villa with id {id} not found");
            }
            return Ok(await _dbContext.Villas.FindAsync(id));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
            $"An error occurred while fetching villa with id {id}: {ex.Message}"
            );
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Villa>> CreateVilla(VillaCreateDTO villaCreateDTO)
    {
        try
        {
            if (villaCreateDTO == null)
            {
                return BadRequest("Villa is required");
            }

            var villa = new Villa
            {
                Name = villaCreateDTO.Name,
                Details = villaCreateDTO.Details,
                Rate = villaCreateDTO.Rate,
                Sqft = villaCreateDTO.Sqft,
                Occupancy = villaCreateDTO.Occupancy,
                ImageUrl = villaCreateDTO.ImageUrl,
                CreatedDate = DateTime.Now.ToUniversalTime()
            };
            
            await _dbContext.Villas.AddAsync(villa);
            await _dbContext.SaveChangesAsync();
            return Ok(villa);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
            $"An error occurred while creating the villa: {ex.Message}"
            );
        }
    }
}
