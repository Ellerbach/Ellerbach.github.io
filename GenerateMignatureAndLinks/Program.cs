using System.Text;
using System.Text.RegularExpressions;

Console.WriteLine("Creating blog entries");

string? repoRoot = FindRepoRoot(Environment.CurrentDirectory);

if (repoRoot is null)
{
    Console.WriteLine("Error: not in a git repository");
    return;
}


string blogPath = Path.Combine(repoRoot, "blog-posts");
string assetsPath = Path.Combine(repoRoot, "assets");

List<string> blogs = new();

foreach (string file in Directory.EnumerateFiles(blogPath, "*.md"))
{
    if (string.Compare(file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar) + 1), "index.md", true) == 0)
    {
        continue;
    }

    blogs.Add(file);
}

// Generate the content
StringBuilder sb = new StringBuilder();
foreach (var file in blogs)
{
    // extract the date from the file name
    var date = file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar) + 1, 10);

    // Check if we have an asset file with this name
    var asset = Directory.EnumerateFiles(assetsPath, $"{date}-thumb.*").FirstOrDefault();

    if (string.IsNullOrEmpty(asset))
    {
        asset = Path.Combine(assetsPath, "nothumbnail.jpg");
    }

    sb.AppendLine($"[![thumb](./{asset.Substring(asset.LastIndexOf("assets")).Replace("\\","/")}) {GetCleanedFileName(file)}](./{file.Substring(file.LastIndexOf("blog-posts")).Replace("\\", "/")})\r\n");
}

// Open the index and replace the <bloglist> tag
ReplacePlaceholder(Path.Combine(repoRoot, "index.md"), "bloglist", sb.ToString());

// Open the index and replace the <bloglist> tag
ReplacePlaceholder(Path.Combine(repoRoot, "blog-posts", "index.md"), "bloglist", sb.ToString().Replace("./assets/", "../assets/").Replace("./blog-posts/", "./"));

string GetCleanedFileName(string cleanedName)
{
    // For markdownfile, open the file, read the line up to the first #, extract the tile
    using (StreamReader toRead = File.OpenText(cleanedName))
    {
        while (!toRead.EndOfStream)
        {
            string strTitle = toRead.ReadLine();
            if (strTitle.TrimStart(' ').StartsWith("# ", StringComparison.OrdinalIgnoreCase))
            {
                cleanedName = strTitle.Substring(2);
                break;
            }
        }
    }

    return ToTitleCase(cleanedName);
}

/// <summary>
/// Uppercase first character and remove unwanted characters.
/// </summary>
/// <param name="title">The name to clean.</param>
/// <returns>A clean name.</returns>
string ToTitleCase(string title)
{
    if (string.IsNullOrEmpty(title))
    {
        return string.Empty;
    }

    string cleantitle = title.First().ToString().ToUpperInvariant() + title.Substring(1);
    cleantitle = Regex.Replace(cleantitle, @"[-_+]", " ");
    return Regex.Replace(cleantitle, @"([\[\]\:`\\{}()#\*]|\.md)", string.Empty);
}

string? FindRepoRoot(string dir)
{
    if (dir is { Length: > 0 })
    {
        if (Directory.Exists(Path.Combine(dir, ".git")))
        {
            return dir;
        }
        else
        {
            DirectoryInfo? parentDir = new DirectoryInfo(dir).Parent;
            return parentDir?.FullName == null ? null : FindRepoRoot(parentDir.FullName);
        }
    }

    return null;
}

void ReplacePlaceholder(string filePath, string placeholderName, string newContent)
{
    string fileContent = File.ReadAllText(filePath);

    string startTag = $"<{placeholderName}>";
    string endTag = $"</{placeholderName}>";

    int startIdx = fileContent.IndexOf(startTag);
    int endIdx = fileContent.IndexOf(endTag);

    if (startIdx == -1 || endIdx == -1)
    {
        throw new Exception($"`{startTag}` not found in `{filePath}`");
    }

    startIdx += startTag.Length;

    File.WriteAllText(
        filePath,
        fileContent.Substring(0, startIdx) +
        Environment.NewLine +
        // Extra empty line is needed so that github does not break bullet points
        Environment.NewLine +
        newContent +
        fileContent.Substring(endIdx));
}
