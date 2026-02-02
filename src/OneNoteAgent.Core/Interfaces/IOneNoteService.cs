using OneNoteAgent.Core.Models;

namespace OneNoteAgent.Core.Interfaces;

/// <summary>
/// Interface for OneNote operations via Microsoft Graph
/// </summary>
public interface IOneNoteService
{
    /// <summary>
    /// Lists all notebooks accessible to the user
    /// </summary>
    Task<IReadOnlyList<NotebookInfo>> ListNotebooksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific notebook by ID
    /// </summary>
    Task<NotebookInfo?> GetNotebookAsync(string notebookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all sections in a notebook
    /// </summary>
    Task<IReadOnlyList<SectionInfo>> ListSectionsAsync(string notebookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all pages in a section
    /// </summary>
    Task<IReadOnlyList<PageInfo>> ListPagesAsync(string sectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the content of a specific page
    /// </summary>
    Task<PageContent?> GetPageContentAsync(string pageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new page in a section
    /// </summary>
    Task<PageInfo> CreatePageAsync(string sectionId, string title, string htmlContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing page's content
    /// </summary>
    Task UpdatePageAsync(string pageId, string htmlContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for pages matching the query
    /// </summary>
    Task<IReadOnlyList<PageInfo>> SearchPagesAsync(string query, CancellationToken cancellationToken = default);
}
