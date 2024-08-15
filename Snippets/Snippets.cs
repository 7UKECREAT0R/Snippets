using System.Diagnostics;

namespace Snippets;

/// <summary>
///     API for managing the Snippets data system and running file I/O.
/// </summary>
public class Snippets : IDisposable
{
    public const string SNIPPETS_FOLDER = "snippets";
    internal readonly Dictionary<string, SnippetsDataObject> snippets;

    private bool _isDisposed;

    internal Snippets()
    {
        this.snippets = new Dictionary<string, SnippetsDataObject>(StringComparer.OrdinalIgnoreCase);
    }
    public void Dispose()
    {
        if (this._isDisposed)
            return;

        this._isDisposed = true;

        foreach (KeyValuePair<string, SnippetsDataObject> snippet in this.snippets)
            snippet.Value.Dispose();
        this.snippets.Clear();
    }

    public static string GetFilePath(string key)
    {
        return Path.Combine(SNIPPETS_FOLDER, key.ToLower() + ".bin");
    }

    internal void Create(string key, SnippetsDataObject value)
    {
        this.snippets[key] = value;
    }
    internal bool CreateFromClipboard(string key, out SnippetsDataObject? data)
    {
        data = SnippetsDataObject.FromClipboard();

        if (data == null)
            return false;

        this.snippets[key] = data;
        return true;
    }
    internal bool RemoveByKey(string key)
    {
        return this.snippets.Remove(key);
    }

    /// <summary>
    ///     Saves all snippets to disk.
    /// </summary>
    internal void Save()
    {
        if (!Directory.Exists(SNIPPETS_FOLDER))
            Directory.CreateDirectory(SNIPPETS_FOLDER);

        foreach (KeyValuePair<string, SnippetsDataObject> snippet in this.snippets)
            SaveSnippet(snippet.Key, snippet.Value);
    }
    /// <summary>
    ///     Saves the given snippet to disk.
    /// </summary>
    /// <param name="key">The key that this snippet should be saved under in the file-system.</param>
    /// <param name="value">The <see cref="SnippetsDataObject" /> to save.</param>
    /// <exception cref="Exception"></exception>
    private void SaveSnippet(string key, SnippetsDataObject value)
    {
        if (this._isDisposed)
            throw new Exception("Attempted to save a Snippets instance that has been disposed.");

        string fileName = GetFilePath(key);
        long bytes;

        if (File.Exists(fileName))
            File.Delete(fileName);

        using (FileStream stream = File.OpenWrite(fileName))
        using (BinaryWriter writer = new(stream))
        {
            value.WriteToStream(writer);
            bytes = stream.Length;
        }

        Debug.WriteLine($"Written {bytes} bytes.");
    }
    /// <summary>
    ///     Load all snippets that are held in the <see cref="SNIPPETS_FOLDER" />.
    /// </summary>
    /// <exception cref="Exception">If this object has been disposed.</exception>
    internal void Load()
    {
        if (this._isDisposed)
            throw new Exception("Attempted to load data into a Snippets instance that has been disposed.");

        if (!Directory.Exists(SNIPPETS_FOLDER))
            Directory.CreateDirectory(SNIPPETS_FOLDER);

        string[] allFiles = Directory.GetFiles(SNIPPETS_FOLDER, "*.bin", SearchOption.AllDirectories);
        this.snippets.Clear();

        foreach (string file in allFiles)
            LoadSnippet(file);
    }
    /// <summary>
    ///     Loads a snippet based on its .bin file path and add it to <see cref="snippets" />. The file name is used as the
    ///     key.
    /// </summary>
    /// <param name="path">The full path to the snippet binary file.</param>
    /// <exception cref="Exception">If this object has been disposed.</exception>
    /// <exception cref="FileNotFoundException">If the file provided doesn't exist.</exception>
    private void LoadSnippet(string path)
    {
        if (this._isDisposed)
            throw new Exception("Attempted to save a Snippets instance that has been disposed.");
        if (!File.Exists(path))
            throw new FileNotFoundException($"The file '{path}' does not exist.");

        string key = Path.GetFileNameWithoutExtension(path);
        string fileName = GetFilePath(key);

        using (FileStream stream = File.OpenRead(fileName))
        using (BinaryReader reader = new(stream))
        {
            SnippetsDataObject? dataObject = SnippetsDataObject.ReadFromStream(reader);

            if (dataObject == null)
                return;

            this.snippets[key] = dataObject;
        }
    }
}