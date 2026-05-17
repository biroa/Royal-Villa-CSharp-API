using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVillaApi.Data;
using RoyalVillaApi.Models;
using RoyalVillaApi.Models.DTO;
using AutoMapper;
namespace RoyalVillaApi.Controllers;

[ApiController]
[Route("api/villa")]
[ApiExplorerSettings(GroupName = "v1")]
public class VillaController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    public VillaController(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
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

    /// <summary>
    ///     POST a new villa: accept input as JSON, persist it, and return 201 Created with a Location header.
    /// </summary>
    /// <remarks>
    ///     <b>HTTP and routing</b>
    ///     <para>
    ///         <c>[HttpPost]</c> — Handles HTTP POST on <c>api/villa</c> (no <c>{id}</c> in the path).
    ///         POST is the conventional verb for <b>creating</b> a resource, as opposed to GET (read)
    ///         or PUT/PATCH (update). The request body is JSON deserialized into
    ///         <see cref="VillaCreateDTO"/> (model binding), similar to <c>@RequestBody</c> in Spring or
    ///         <c>req.body</c> in Express when a JSON middleware is enabled.
    ///     </para>
    ///     <para>
    ///         <c>[ProducesResponseType(...)]</c> — Documents 201, 400, and 500 for OpenAPI/Swagger; they
    ///         do not change runtime behavior.
    ///     </para>
    ///     <b>DTO vs entity</b>
    ///     <para>
    ///         <see cref="VillaCreateDTO"/> is a <b>Data Transfer Object</b>: the shape the client sends
    ///         when creating a villa (name, details, rate, etc.). It intentionally omits server-owned fields
    ///         such as <see cref="Villa.Id"/> — the database generates the id on insert. This separation
    ///         is common in APIs (request DTO vs persistence model) and avoids clients supplying primary keys.
    ///     </para>
    ///     <para>
    ///         <c>_mapper.Map&lt;Villa&gt;(villaCreateDTO)</c> — AutoMapper copies matching properties from
    ///         the DTO into a new <see cref="Villa"/> entity (like MapStruct in Java or manual field mapping
    ///         in other stacks). Mapping is configured at startup in <c>Program.cs</c>.
    ///     </para>
    ///     <b>Database write (EF Core)</b>
    ///     <para>
    ///         <c>AddAsync(villa)</c> — Stages the entity in the change tracker as <b>Added</b>; no SQL
    ///         INSERT runs yet. Think of it as “queue this row for insert.”
    ///     </para>
    ///     <para>
    ///         <c>SaveChangesAsync()</c> — Flushes pending changes to the database (here, one INSERT).
    ///         After this call, <c>villa.Id</c> is populated if the key is database-generated (identity column).
    ///     </para>
    ///     <b>Return type and 201 Created</b>
    ///     <para>
    ///         <c>Task&lt;ActionResult&lt;Villa&gt;&gt;</c> — Asynchronous action that eventually returns an
    ///         HTTP wrapper; on success the body is the created <see cref="Villa"/> (including the new id).
    ///     </para>
    ///     <para>
    ///         <c>CreatedAtAction(nameof(GetVillaById), new { id = villa.Id }, villa)</c> — Returns
    ///         <b>201 Created</b> with:
    ///         (1) a <c>Location</c> header pointing at the GET-by-id URL (e.g. <c>api/villa/5</c>),
    ///         (2) the created resource in the response body.
    ///         <c>nameof(GetVillaById)</c> resolves to the string <c>"GetVillaById"</c> at compile time
    ///         (refactor-safe). <c>new { id = villa.Id }</c> is an anonymous object supplying route values
    ///         for that action — like building path params for a redirect URL.
    ///     </para>
    ///     <b>Validation and errors</b>
    ///     <para>
    ///         <c>if (villaCreateDTO == null)</c> — Guards a missing body; returns 400. Data annotations on
    ///         <see cref="VillaCreateDTO"/> (e.g. <c>[Required]</c>, <c>[MaxLength]</c>) can also produce 400
    ///         when model validation runs (if enabled globally or via <c>[ApiController]</c>).
    ///     </para>
    ///     <para>
    ///         <c>try</c> / <c>catch</c> — Unexpected failures (database, mapping) become 500 with
    ///         <c>ex.Message</c> in the body instead of an unhandled exception page.
    ///     </para>
    ///     <b>Typical questions from other languages</b>
    ///     <para>
    ///         <b>Why not return 200 OK?</b> — REST conventions use <b>201 Created</b> for successful creation,
    ///         often with <c>Location</c> set to the new resource’s URL.
    ///     </para>
    ///     <para>
    ///         <b>Why map DTO → entity instead of saving the DTO directly?</b> — The entity matches the
    ///         database table and EF Core’s model; the DTO is the public API contract and can differ (extra
    ///         fields omitted, different names, validation only on input).
    ///     </para>
    ///     <para>
    ///         <b>When is the id available?</b> — Only after <c>SaveChangesAsync</c>; before that,
    ///         <c>villa.Id</c> is typically 0 or default.
    ///     </para>
    /// </remarks>
    /// <param name="villaCreateDTO">JSON body with villa fields to create; must not be null.</param>
    /// <returns>201 Created with Location and the new villa, 400 if the body is missing, 500 on failure.</returns>
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

            var villa = _mapper.Map<Villa>(villaCreateDTO);

            await _dbContext.Villas.AddAsync(villa);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVillaById), new { id = villa.Id }, villa);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while creating the villa: {ex.Message}");
        }
    }

    /// <summary>
    ///     PUT an existing villa by route id: accept JSON, reject duplicate names with 409 Conflict, update the row,
    ///     and return 200 OK with the updated entity.
    /// </summary>
    /// <remarks>
    ///     <b>HTTP and routing</b>
    ///     <para>
    ///         <c>[HttpPut("{id:int}")]</c> — Handles HTTP PUT on <c>api/villa/{id}</c>. PUT is the conventional
    ///         verb for <b>replacing or updating</b> a resource at a known URL. The <c>{id:int}</c> route
    ///         constraint ensures only integer path segments match (same as <see cref="GetVillaById"/>). The
    ///         matched value is bound to <paramref name="id"/>; the JSON body is bound to
    ///         <see cref="VillaUpdateDTO"/>.
    ///     </para>
    ///     <para>
    ///         <c>[ProducesResponseType(...)]</c> — Documents 200, 400, 404, 409, and 500 for OpenAPI/Scalar; they do
    ///         not change runtime behavior.
    ///     </para>
    ///     <b>DTO vs entity</b>
    ///     <para>
    ///         <see cref="VillaUpdateDTO"/> mirrors <see cref="VillaCreateDTO"/>: updatable fields only (name,
    ///         details, rate, etc.). It omits <see cref="Villa.Id"/> — the client identifies the resource via
    ///         the URL, not the body. AutoMapper is configured to <b>ignore</b> <see cref="Villa.Id"/> when
    ///         mapping so the primary key cannot be overwritten by mistake.
    ///     </para>
    ///     <para>
    ///         <c>_mapper.Map(villaUpdateDTO, existingVilla)</c> — Copies matching properties from the DTO onto
    ///         an <b>existing</b> tracked <see cref="Villa"/> (the two-argument overload), unlike create which
    ///         maps into a new entity. Server-owned fields not on the DTO (e.g. <see cref="Villa.CreatedDate"/>)
    ///         are left unchanged.
    ///     </para>
    ///     <b>Database update (EF Core)</b>
    ///     <para>
    ///         <c>FirstOrDefaultAsync(v =&gt; v.Id == id)</c> — Loads the row to update or returns <c>null</c>
    ///         if missing (→ 404). The entity is <b>tracked</b> by the change tracker after this query.
    ///     </para>
    ///     <para>
    ///         <c>FirstOrDefaultAsync(... Name ... &amp;&amp; v.Id != id)</c> — Looks for another villa whose
    ///         name matches the request (case-insensitive). If one exists, the update would violate a unique
    ///         business rule, so the action stops before mapping or saving.
    ///     </para>
    ///     <para>
    ///         After mapping, <c>existingVilla.UpdatedDate = DateTime.Now.ToUniversalTime()</c> sets the audit
    ///         timestamp. <c>SaveChangesAsync()</c> issues an SQL <c>UPDATE</c> for modified columns, not an
    ///         <c>INSERT</c>.
    ///     </para>
    ///     <b>Return type and 200 OK</b>
    ///     <para>
    ///         <c>Task&lt;ActionResult&lt;Villa&gt;&gt;</c> — Asynchronous action that eventually returns an
    ///         HTTP wrapper; on success the body is the updated <see cref="Villa"/> (same id, new field values).
    ///     </para>
    ///     <para>
    ///         <c>return Ok(existingVilla)</c> — <b>200 OK</b> with the updated resource in the body (REST
    ///         convention for successful update when returning the representation).
    ///     </para>
    ///     <b>Validation and errors</b>
    ///     <para>
    ///         <c>if (id &lt;= 0)</c> — Returns 400 when the route id is not a positive business key (the route
    ///         allows any integer, including 0 and negatives).
    ///     </para>
    ///     <para>
    ///         <c>if (villaUpdateDTO == null)</c> — Returns 400 when the body failed to bind (e.g. missing body).
    ///         Data annotations on <see cref="VillaUpdateDTO"/> (e.g. <c>[Required]</c>, <c>[MaxLength]</c>)
    ///         can also produce 400 via <c>[ApiController]</c> model validation. Numeric fields in JSON must be
    ///         numbers, not empty strings, or deserialization fails before this action runs.
    ///     </para>
    ///     <para>
    ///         <c>if (existingVilla == null)</c> — Returns 404 when no villa exists for <paramref name="id"/>.
    ///     </para>
    ///     <para>
    ///         <c>if (duplicateVilla != null)</c> — Returns <b>409 Conflict</b> via
    ///         <c>Conflict(...)</c>. In ASP.NET Core, <c>Conflict</c> is a helper that sets the HTTP status to
    ///         <b>409</b> and puts a short message in the body (e.g. “Villa with name … already exists”).
    ///         <b>409 Conflict</b> means: the request was understood and the target villa exists, but the change
    ///         cannot be applied because it clashes with another record (here, another villa already uses that
    ///         name). This is not a syntax error (400) or “wrong id” (404); it is a <b>state conflict</b>, like
    ///         trying to rename a user to an email that is already taken. Clients should show the message to the
    ///         user and ask for a different name rather than retrying the same payload.
    ///     </para>
    ///     <para>
    ///         <c>try</c> / <c>catch</c> — Unexpected failures (database, mapping) become 500 with
    ///         <c>ex.Message</c> in the body.
    ///     </para>
    ///     <b>Typical questions from other languages</b>
    ///     <para>
    ///         <b>PUT vs PATCH?</b> — This action performs a full update of the DTO-shaped fields on the entity.
    ///         PATCH would send partial changes; here the client supplies the full updatable shape each time.
    ///     </para>
    ///     <para>
    ///         <b>Why is id only in the URL?</b> — Avoids duplicate id in path and body, matches create (no client
    ///         id), and prevents binding errors from sending <c>""</c> for an <c>int</c> in JSON.
    ///     </para>
    ///     <para>
    ///         <b>Why not return 204 No Content?</b> — Returning 200 with the updated body lets clients refresh
    ///         UI state without a follow-up GET.
    ///     </para>
    ///     <para>
    ///         <b>What is Conflict / 409?</b> — HTTP status <b>409</b> is named “Conflict”. Frameworks expose it
    ///         as a named result (here <c>Conflict</c>) so you do not hard-code <c>409</c> everywhere. Use it when
    ///         the client sent valid data but the server refuses because of a rule about existing data (duplicate
    ///         name, version mismatch, etc.). Compare: <b>400</b> bad input, <b>404</b> resource missing,
    ///         <b>409</b> valid request but incompatible with current data.
    ///     </para>
    /// </remarks>
    /// <param name="id">Primary key of the villa to update; must be greater than zero.</param>
    /// <param name="villaUpdateDTO">JSON body with fields to update; must not be null.</param>
    /// <returns>200 OK with the updated villa; 400 if id or body is invalid; 404 if not found; 409 if the name is already used by another villa; 500 on failure.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Villa>> UpdateVilla(int id, VillaUpdateDTO villaUpdateDTO)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("Id is required");
            }
            if (villaUpdateDTO == null)
            {
                return BadRequest("Villa is required");
            }
            var existingVilla = await _dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);
            if (existingVilla == null)
            {
                return NotFound($"Villa with id {id} not found");
            }
            var duplicateVilla = await _dbContext.Villas.FirstOrDefaultAsync(v => v.Name.ToLower() == villaUpdateDTO.Name.ToLower() && v.Id != id);
            
            if (duplicateVilla != null)
            {
                return Conflict($"Villa with name {villaUpdateDTO.Name} already exists");
            }

            _mapper.Map(villaUpdateDTO, existingVilla);
            existingVilla.UpdatedDate = DateTime.Now.ToUniversalTime();
            await _dbContext.SaveChangesAsync();
            return Ok(existingVilla);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the villa: {ex.Message}");
        }
    }

    /// <summary>
    ///     DELETE a villa by route id: remove the row from the database and return 204 No Content.
    /// </summary>
    /// <remarks>
    ///     <b>HTTP and routing</b>
    ///     <para>
    ///         <c>[HttpDelete("{id:int}")]</c> — Handles HTTP DELETE on <c>api/villa/{id}</c>. DELETE is the
    ///         conventional verb for <b>removing</b> a resource. The <c>{id:int}</c> route constraint ensures only
    ///         integer path segments match (same as <see cref="GetVillaById"/> and <see cref="UpdateVilla"/>). The
    ///         matched value is bound to <paramref name="id"/>; there is no request body.
    ///     </para>
    ///     <para>
    ///         <c>[ProducesResponseType(...)]</c> — Documents 204, 400, 404, and 500 for OpenAPI/Scalar; they do
    ///         not change runtime behavior.
    ///     </para>
    ///     <b>Database delete (EF Core)</b>
    ///     <para>
    ///         <c>FirstOrDefaultAsync(v =&gt; v.Id == id)</c> — Loads the row to delete or returns <c>null</c>
    ///         if missing (→ 404). The entity must exist and be tracked before removal.
    ///     </para>
    ///     <para>
    ///         <c>Remove(existingVilla)</c> — Marks the entity as <b>Deleted</b> in the change tracker; no SQL
    ///         <c>DELETE</c> runs until <c>SaveChangesAsync()</c>.
    ///     </para>
    ///     <para>
    ///         <c>SaveChangesAsync()</c> — Flushes the pending delete to the database (one <c>DELETE</c> statement
    ///         for this row).
    ///     </para>
    ///     <b>Return type and 204 No Content</b>
    ///     <para>
    ///         <c>Task&lt;ActionResult&lt;Villa&gt;&gt;</c> — The generic type documents the resource kind for
    ///         OpenAPI; on success this action returns <b>no body</b>.
    ///     </para>
    ///     <para>
    ///         <c>return NoContent()</c> — <b>204 No Content</b>, the usual REST response for a successful delete
    ///         when the client does not need the deleted representation in the response.
    ///     </para>
    ///     <b>Validation and errors</b>
    ///     <para>
    ///         <c>if (id &lt;= 0)</c> — Returns 400 when the route id is not a positive business key.
    ///     </para>
    ///     <para>
    ///         <c>if (existingVilla == null)</c> — Returns 404 when no villa exists for <paramref name="id"/>.
    ///     </para>
    ///     <para>
    ///         <c>try</c> / <c>catch</c> — Unexpected failures (database) become 500 with <c>ex.Message</c> in
    ///         the body.
    ///     </para>
    ///     <b>Typical questions from other languages</b>
    ///     <para>
    ///         <b>Hard delete vs soft delete?</b> — This action <b>physically removes</b> the row. A soft delete
    ///         would set a flag (e.g. <c>IsDeleted</c>) and filter it out of queries instead of calling
    ///         <c>Remove</c>.
    ///     </para>
    ///     <para>
    ///         <b>Why 204 instead of 200 with the deleted villa?</b> — After deletion the resource no longer exists;
    ///         204 signals success without implying a retrievable body. Clients that need the last snapshot should
    ///         GET before DELETE.
    ///     </para>
    ///     <para>
    ///         <b>Is DELETE idempotent?</b> — Deleting the same id twice: first call → 204; second call → 404
    ///         because the row is already gone (not strictly idempotent in the strictest sense, but safe to retry).
    ///     </para>
    /// </remarks>
    /// <param name="id">Primary key of the villa to delete; must be greater than zero.</param>
    /// <returns>204 No Content on success, 400 if id is invalid, 404 if not found, 500 on failure.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Villa>> DeleteVilla(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("Id is required");
            }
            var existingVilla = await _dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);
            if (existingVilla == null)
            {
                return NotFound($"Villa with id {id} not found");
            }
    
            _dbContext.Villas.Remove(existingVilla);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting the villa: {ex.Message}");
        }
    }

}
