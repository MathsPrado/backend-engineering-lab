using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blog.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Blog.Web.Services;

public class FileSystemPostRepository
{
    private readonly string _repoRootPath;
    private readonly ILogger<FileSystemPostRepository> _logger;

    public FileSystemPostRepository(IWebHostEnvironment env, ILogger<FileSystemPostRepository> logger)
    {
        _logger = logger;
        string current = env.ContentRootPath;
        DirectoryInfo? dir = new DirectoryInfo(current);
        
        while (dir != null)
        {
            try
            {
                if (dir.GetDirectories().Any(d => Regex.IsMatch(d.Name, @"^0[1-6]-")))
                {
                    _repoRootPath = dir.FullName;
                    _logger.LogInformation("Raiz do repositório identificada em: {RepoRoot}", _repoRootPath);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao inspecionar diretório: {Dir}", dir.FullName);
            }
            dir = dir.Parent;
        }

        _repoRootPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", ".."));
        _logger.LogInformation("Fallback para raiz do repositório: {RepoRoot}", _repoRootPath);
    }

    public Task<List<BlogPost>> GetAllPostsAsync()
    {
        var posts = new List<BlogPost>();

        if (!Directory.Exists(_repoRootPath))
        {
            _logger.LogWarning("Diretório raiz não existe: {RepoRoot}", _repoRootPath);
            return Task.FromResult(posts);
        }

        var moduleDirs = Directory.GetDirectories(_repoRootPath)
            .Where(d => Regex.IsMatch(Path.GetFileName(d), @"^0[1-6]-"))
            .OrderBy(d => Path.GetFileName(d))
            .ToList();

        _logger.LogInformation("Módulos encontrados: {Count}", moduleDirs.Count);

        foreach (var moduleDir in moduleDirs)
        {
            string categoryName = Path.GetFileName(moduleDir);

            var mdFiles = Directory.GetFiles(moduleDir, "*.md", SearchOption.AllDirectories)
                .Where(f => !f.Contains(".github") 
                         && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") 
                         && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                         && !f.Contains("BenchmarkDotNet.Artifacts"))
                .OrderBy(f => f)
                .ToList();

            _logger.LogInformation("Arquivos Markdown em {Category}: {Count}", categoryName, mdFiles.Count);

            foreach (var filePath in mdFiles)
            {
                var post = ParseMarkdownFile(filePath, moduleDir, categoryName);
                if (post != null)
                {
                    posts.Add(post);
                }
            }
        }

        return Task.FromResult(posts);
    }

    public async Task<BlogPost?> GetPostBySlugAsync(string slug)
    {
        var allPosts = await GetAllPostsAsync();
        return allPosts.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var allPosts = await GetAllPostsAsync();
        return allPosts.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
    }

    private BlogPost? ParseMarkdownFile(string filePath, string moduleDir, string categoryName)
    {
        try
        {
            string content = File.ReadAllText(filePath);
            FileInfo fileInfo = new FileInfo(filePath);

            string relativePath = Path.GetRelativePath(_repoRootPath, filePath);
            string subcategory = Path.GetFileName(Path.GetDirectoryName(filePath) ?? moduleDir);
            
            if (subcategory.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
            {
                subcategory = Path.GetFileNameWithoutExtension(filePath);
            }

            string slug = relativePath
                .Replace(".md", "")
                .Replace(Path.DirectorySeparatorChar.ToString(), "__")
                .Replace("/", "__");

            string title = ExtractTitle(content) ?? CleanName(Path.GetFileNameWithoutExtension(filePath));
            string summary = ExtractSummary(content);

            return new BlogPost
            {
                Slug = slug,
                Title = title,
                Summary = summary,
                Category = categoryName,
                Subcategory = subcategory,
                FilePath = filePath,
                ContentMarkdown = content,
                LastModified = fileInfo.LastWriteTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar arquivo Markdown: {FilePath}", filePath);
            return null;
        }
    }

    private string? ExtractTitle(string markdown)
    {
        using var reader = new StringReader(markdown);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("# ") && trimmed.Length > 2)
            {
                return trimmed.Substring(2).Trim('#', ' ');
            }
        }
        return null;
    }

    private string ExtractSummary(string markdown)
    {
        using var reader = new StringReader(markdown);
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            string trimmed = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            if (trimmed.StartsWith("#")) continue;
            if (trimmed.StartsWith("```")) continue;
            if (trimmed.StartsWith("|")) continue;
            if (trimmed.StartsWith("---")) continue;
            if (trimmed.StartsWith("![")) continue;

            string cleanText = Regex.Replace(trimmed, @"[*_\[\]()`#>\-]", "").Trim();
            if (cleanText.Length > 15)
            {
                if (cleanText.Length > 200)
                {
                    return cleanText.Substring(0, 197) + "...";
                }
                return cleanText;
            }
        }

        return "Estudo prático contendo roteiro técnico, código-fonte e análises de engenharia de software.";
    }

    private string CleanName(string name)
    {
        return name.Replace("_", " ").Replace("-", " ");
    }
}
