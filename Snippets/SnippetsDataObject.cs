using System.Collections.Specialized;
using System.Drawing.Imaging;

namespace Snippets;

/// <summary>
///     Wraps an <see cref="IDataObject" /> object in a save-able, friendly way. Can be converted back into a data object.
/// </summary>
internal class SnippetsDataObject : IDisposable
{
    internal object? data; // The object holding the data.

    internal string format; // DataFormats.cs

    internal bool shouldDispose;
    internal TextDataFormat? textFormat;
    internal FormatType type;

    private SnippetsDataObject()
    {
        this.textFormat = null;
        this.type = FormatType.Audio;
        this.format = "";
        this.data = null;
    }
    /// <summary>
    ///     Creates a new <see cref="SnippetsDataObject" /> with data filled from the given clipboard object.
    /// </summary>
    /// <param name="source"></param>
    internal SnippetsDataObject(IDataObject _source)
    {
        this.shouldDispose = false;

        if (_source is not DataObject)
            throw new Exception("Clipboard data was not a DataObject; got " + _source.GetType().Name);

        var source = (DataObject) _source;

        if (source.ContainsAudio())
        {
            this.type = FormatType.Audio;
            this.format = DataFormats.WaveAudio;
            this.data = source.GetAudioStream();
            return;
        }

        if (source.ContainsFileDropList())
        {
            this.type = FormatType.FileDropList;
            this.format = DataFormats.FileDrop;
            StringCollection files = source.GetFileDropList();
            string[] buffer = new string[files.Count];
            files.CopyTo(buffer, 0);
            this.data = buffer;
            return;
        }

        if (source.ContainsImage())
        {
            this.type = FormatType.Image;
            this.format = DataFormats.Bitmap;
            this.data = source.GetImage();
            return;
        }

        // string
        this.type = FormatType.Text;
        this.data = source.GetText();

        if (source.ContainsText(TextDataFormat.Html))
        {
            this.textFormat = TextDataFormat.Html;
            this.format = DataFormats.Html;
        }
        else if (source.ContainsText(TextDataFormat.Rtf))
        {
            this.textFormat = TextDataFormat.Rtf;
            this.format = DataFormats.Rtf;
        }
        else if (source.ContainsText(TextDataFormat.UnicodeText))
        {
            this.textFormat = TextDataFormat.UnicodeText;
            this.format = DataFormats.UnicodeText;
        }
        else
        {
            this.textFormat = TextDataFormat.Text;
            this.format = DataFormats.Text;
        }
    }

    public bool IsPreviewString
    {
        get
        {
            switch (this.type)
            {
                case FormatType.Text:
                    return true;
                case FormatType.FileDropList:
                    return true;
                case FormatType.Audio:
                    return true;
                case FormatType.Image:
                    return false;
                default:
                    return false;
            }
        }
    }
    /// <summary>
    ///     Release resources used by this SnippetsDataObject, if permitted and not currently held hostage by the clipboard.
    /// </summary>
    public void Dispose()
    {
        if (!this.shouldDispose)
            return;
        if (this.data == null)
            return;

        this.shouldDispose = false;

        if (this.type == FormatType.Audio)
        {
            var audioStream = (Stream) this.data;
            audioStream.Close();
            audioStream.Dispose();
        }
        else if (this.type == FormatType.Image)
        {
            var bitmap = (Image) this.data;
            bitmap.Dispose();
        }
    }
    /// <summary>
    ///     Converts this <see cref="SnippetsDataObject" /> to a clipboard object containing the data.
    /// </summary>
    /// <returns></returns>
    internal IDataObject ToClipboardObject()
    {
        DataObject output = new();
        output.SetData(this.format, this.data);
        return output;
    }

    /// <summary>
    ///     Writes this SnippetsDataObject to the given <see cref="BinaryWriter" />.
    /// </summary>
    /// <param name="stream"></param>
    internal async void WriteToStream(BinaryWriter stream)
    {
        if (this.data == null)
            return;

        stream.Write((byte) this.type);
        stream.Write(this.format);

        switch (this.type)
        {
            case FormatType.Text:
                stream.Write((byte) this.textFormat!);
                stream.Write((string) this.data);
                break;
            case FormatType.Audio:
                var audioStream = (Stream) this.data;
                if (!audioStream.CanRead)
                    return;
                Memory<byte> memory = new();
                int numBytesRead = await audioStream.ReadAsync(memory);
                byte[] bytes = memory.ToArray();
                stream.Write(numBytesRead);
                stream.Write(bytes, 0, numBytesRead);
                break;
            case FormatType.FileDropList:
                string[] files = (string[]) this.data;
                stream.Write(files.Length);
                foreach (string file in files)
                    stream.Write(file);
                break;
            case FormatType.Image:
                var bitmap = (Image) this.data;
                using (MemoryStream tempStream = new())
                {
                    bitmap.Save(tempStream, ImageFormat.Png);
                    stream.Write((int) tempStream.Length); // write length of bitmap
                    tempStream.Seek(0, SeekOrigin.Begin);
                    tempStream.CopyTo(stream.BaseStream);
                }

                break;
        }
    }
    /// <summary>
    ///     Reads a SnippetsDataObject from the given <see cref="BinaryReader" />.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns>null if no valid data could be read.</returns>
    internal static SnippetsDataObject? ReadFromStream(BinaryReader stream)
    {
        if (stream == null)
            return null;

        var type = (FormatType) stream.ReadByte();
        string format = stream.ReadString();

        switch (type)
        {
            case FormatType.Text:
                var textFormat = (TextDataFormat) stream.ReadByte();
                string textData = stream.ReadString();
                return new SnippetsDataObject
                {
                    shouldDispose = true,
                    data = textData,
                    textFormat = textFormat,
                    format = format,
                    type = type
                };
            case FormatType.Audio:
                int audioLength = stream.ReadInt32();
                Stream audioStream = new MemoryStream();
                stream.BaseStream.CopyTo(audioStream, audioLength);
                stream.BaseStream.Seek(audioLength, SeekOrigin.Current);
                return new SnippetsDataObject
                {
                    shouldDispose = true,
                    data = audioStream,
                    format = format,
                    type = type
                };
            case FormatType.FileDropList:
                int numFiles = stream.ReadInt32();
                string[] files = new string[numFiles];
                for (int i = 0; i < numFiles; i++)
                    files[i] = stream.ReadString();
                return new SnippetsDataObject
                {
                    shouldDispose = true,
                    data = files,
                    format = format,
                    type = type
                };
            case FormatType.Image:
                int bitmapLength = stream.ReadInt32();
                MemoryStream imageStream = new(bitmapLength);
                stream.BaseStream.CopyTo(imageStream, bitmapLength);
                Image bitmap = Image.FromStream(imageStream);

                return new SnippetsDataObject
                {
                    shouldDispose = true,
                    data = bitmap,
                    format = format,
                    type = type
                };
            default:
                return null;
        }
    }
    public override string? ToString()
    {
        if (this.data == null)
            return $"Snippet Object ({this.type}, format '{this.format}')\n\t<no data>";

        if (this.type == FormatType.Text)
            return
                $"Snippet Object ({this.type}, format '{this.format}', text format '{this.textFormat}')\n\t{(string) this.data}";

        if (this.type == FormatType.FileDropList)
        {
            string[] files = (string[]) this.data;
            return $"Snippet Object ({this.type}, format '{this.format}')\n\t{string.Join(",\n\t", files)}";
        }

        return $"Snippet Object ({this.type}, format '{this.format}')\n\t{this.data}";
    }
    public string GetPreviewString()
    {
        if (this.data == null)
            return "Empty";

        switch (this.type)
        {
            case FormatType.Audio:
                var audioStream = (Stream) this.data;
                return "Audio - " + audioStream.Length / 1024 / 1024d + "MB";
            case FormatType.FileDropList:
                string[] files = (string[]) this.data;
                IEnumerable<string> modifiedFiles = files.Select(f => "\t- " + f);
                return "File Collection:\n" + string.Join("\n", modifiedFiles);
            case FormatType.Text:
                return (string) this.data;
            case FormatType.Image:
            default:
                throw new Exception("No text preview available. Consider checking IsPreviewString beforehand.");
        }
    }

    /// <summary>
    ///     Pulls a <see cref="SnippetsDataObject" /> from the user's clipboard.
    /// </summary>
    /// <returns>null if there is no data in the clipboard.</returns>
    public static SnippetsDataObject? FromClipboard()
    {
        IDataObject? dataObject = Clipboard.GetDataObject();

        if (dataObject == null)
            return null;

        return new SnippetsDataObject(dataObject);
    }

    internal enum FormatType : byte
    {
        /// <summary>
        ///     data is System.IO.Stream
        /// </summary>
        Audio = 0,
        /// <summary>
        ///     data is System.String[]
        /// </summary>
        FileDropList = 1,
        /// <summary>
        ///     data is System.Drawing.Image
        /// </summary>
        Image = 2,
        /// <summary>
        ///     data is System.String, use textFormat to determine
        /// </summary>
        Text = 3
    }
}