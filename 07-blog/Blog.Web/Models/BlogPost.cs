using System;

namespace Blog.Web.Models;

public class BlogPost
{
    public required string Slug { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public required string Category { get; set; }
    public required string Subcategory { get; set; }
    public required string FilePath { get; set; }
    public required string ContentMarkdown { get; set; }
    public DateTime LastModified { get; set; }
}
