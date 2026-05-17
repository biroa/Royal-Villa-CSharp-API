namespace RoyalVillaApi.Models.DTO;

public class ApiResponse<TData>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public TData? Data { get; set; }
    public object? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<TData> Create(bool success, int statusCode, string message, TData? data = default, object? errors = null)
    {
        return new ApiResponse<TData> {
            Success = success,
            StatusCode = statusCode,
            Message = message,
            Data = data,
            Errors = errors
        };
    }

    public static ApiResponse<TData> NotFound(string message) => 
        Create(false, StatusCodes.Status404NotFound, message);

    public static ApiResponse<TData> BadRequest(string message, object? errors = null) => 
        Create(false, StatusCodes.Status400BadRequest, message, errors: errors);

    public static ApiResponse<TData> InternalServerError(string message) => 
        Create(false, StatusCodes.Status500InternalServerError, message);

    public static ApiResponse<TData> Ok(TData data) => 
        Create(true, StatusCodes.Status200OK, "Success", data);

    public static ApiResponse<TData> Created(TData data) => 
        Create(true, StatusCodes.Status201Created, "Created", data);

    public static ApiResponse<TData> CreatedAtAction(string actionName, object? routeValues = null, TData? data = default) => 
        Create(true, StatusCodes.Status201Created, "Created", data, new { actionName, routeValues });

    public static ApiResponse<TData> CreatedAtRoute(string routeName, object? routeValues = null, TData? data = default) => 
        Create(true, StatusCodes.Status201Created, "Created", data, new { routeName, routeValues });

    public static ApiResponse<TData> NoContent() => 
        Create(true, StatusCodes.Status204NoContent, "No content");

    public static ApiResponse<TData> Accepted() => 
        Create(true, StatusCodes.Status202Accepted, "Accepted");

    public static ApiResponse<TData> Redirect(string url) => 
        Create(true, StatusCodes.Status302Found, "Redirect", errors: new { url });
        
    public static ApiResponse<TData> Unauthorized() => 
        Create(false, StatusCodes.Status401Unauthorized, "Unauthorized");

    public static ApiResponse<TData> Forbidden() => 
        Create(false, StatusCodes.Status403Forbidden, "Forbidden");

    public static ApiResponse<TData> Conflict(string message) => 
        Create(false, StatusCodes.Status409Conflict, message);

    public static ApiResponse<TData> TooManyRequests(string message) => 
        Create(false, StatusCodes.Status429TooManyRequests, message);

    public static ApiResponse<TData> BadGateway(string message) => 
        Create(false, StatusCodes.Status502BadGateway, message);
        
    public static ApiResponse<TData> ServiceUnavailable(string message) => 
        Create(false, StatusCodes.Status503ServiceUnavailable, message);

    public static ApiResponse<TData> GatewayTimeout(string message) => 
        Create(false, StatusCodes.Status504GatewayTimeout, message);

    public static ApiResponse<TData> PreconditionFailed(string message) => 
        Create(false, StatusCodes.Status412PreconditionFailed, message);
        
}