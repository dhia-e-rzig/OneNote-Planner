namespace OneNoteAgent.Core.Interfaces;

/// <summary>
/// Interface for authentication services with scope minimization support
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Gets an access token for the specified scopes.
    /// Uses silent authentication if possible, otherwise prompts interactively.
    /// </summary>
    Task<string> GetAccessTokenAsync(string[] scopes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a read-only access token (Notes.Read scope)
    /// </summary>
    Task<string> GetReadOnlyTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a read-write access token (Notes.ReadWrite scope).
    /// Only request when write operations are needed.
    /// </summary>
    Task<string> GetReadWriteTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs out the current user and clears all tokens
    /// </summary>
    Task SignOutAsync();

    /// <summary>
    /// Returns whether the user is currently signed in
    /// </summary>
    Task<bool> IsSignedInAsync();

    /// <summary>
    /// Gets the current user's display name if signed in
    /// </summary>
    Task<string?> GetCurrentUserNameAsync(CancellationToken cancellationToken = default);
}
