using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snippets
{
    /// <summary>
    /// Wraps an <see cref="IDataObject"/> object in a save-able, friendly way. Can be converted back into a data object.
    /// </summary>
    internal class SnippetsDataObject : IDisposable
    {
        internal enum FormatType : byte
        {
            /// <summary>
            /// data is System.IO.Stream
            /// </summary>
            Audio = 0,
            /// <summary>
            /// data is System.String[]
            /// </summary>
            FileDropList = 1,
            /// <summary>
            /// data is System.Drawing.Image
            /// </summary>
            Image = 2,
            /// <summary>
            /// data is System.String, use textFormat to determine
            /// </summary>
            Text = 3
        }

        internal bool shouldDispose;
        internal TextDataFormat? textFormat;
        internal FormatType type;

        internal string format;     // DataFormats.cs
        internal object? data;       // The object holding the data.

        private SnippetsDataObject()
        {
            textFormat = null;
            type = FormatType.Audio;
            format = "";
            data = null;
        }
        /// <summary>
        /// Creates a new <see cref="SnippetsDataObject"/> with data filled from the given clipboard object.
        /// </summary>
        /// <param name="source"></param>
        internal SnippetsDataObject(IDataObject _source)
        {
            shouldDispose = false;

            if (_source is not DataObject)
                throw new Exception("Clipboard data was not a DataObject; got " +  _source.GetType().Name);

            DataObject source = (DataObject)_source;

            if(source.ContainsAudio())
            {
                type = FormatType.Audio;
                format = DataFormats.WaveAudio;
                data = source.GetAudioStream();
                return;
            }
            else if(source.ContainsFileDropList())
            {
                type = FormatType.FileDropList;
                format = DataFormats.FileDrop;
                var files = source.GetFileDropList();
                string[] buffer = new string[files.Count];
                files.CopyTo(buffer, 0);
                data = buffer;
                return;
            }
            else if(source.ContainsImage())
            {
                type = FormatType.Image;
                format = DataFormats.Bitmap;
                data = source.GetImage();
                return;
            }

            // string
            type = FormatType.Text;
            data = source.GetText();

            if (source.ContainsText(TextDataFormat.Html))
            {
                textFormat = TextDataFormat.Html;
                format = DataFormats.Html;
            }
            else if (source.ContainsText(TextDataFormat.Rtf))
            {
                textFormat = TextDataFormat.Rtf;
                format = DataFormats.Rtf;
            }
            else if (source.ContainsText(TextDataFormat.UnicodeText))
            {
                textFormat = TextDataFormat.UnicodeText;
                format = DataFormats.UnicodeText;
            }
            else
            {
                textFormat = TextDataFormat.Text;
                format = DataFormats.Text;
            }
        }
        /// <summary>
        /// Converts this <see cref="SnippetsDataObject"/> to a clipboard object containing the data.
        /// </summary>
        /// <returns></returns>
        internal IDataObject ToClipboardObject()
        {
            DataObject output = new();
            output.SetData(format, data);
            return output;
        }

        /// <summary>
        /// Writes this SnippetsDataObject to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="stream"></param>
        internal async void WriteToStream(BinaryWriter stream)
        {
            if (data == null)
                return;

            stream.Write((byte)type);
            stream.Write(format);

            switch (type)
            {
                case FormatType.Text:
                    stream.Write((byte)textFormat!);
                    stream.Write((string)data);
                    break;
                case FormatType.Audio:
                    Stream audioStream = (Stream)data;
                    if (!audioStream.CanRead)
                        return;
                    Memory<byte> memory = new();
                    int numBytesRead = await audioStream.ReadAsync(memory);
                    byte[] bytes = memory.ToArray();
                    stream.Write(numBytesRead);
                    stream.Write(bytes, 0, numBytesRead);
                    break;
                case FormatType.FileDropList:
                    string[] files = (string[])data;
                    stream.Write(files.Length);
                    foreach(string file in files)
                        stream.Write(file);
                    break;
                case FormatType.Image:
                    Image bitmap = (Image)data;
                    using (MemoryStream tempStream = new())
                    {
                        bitmap.Save(tempStream, ImageFormat.Png);
                        stream.Write((int)tempStream.Length);    // write length of bitmap
                        tempStream.Seek(0, SeekOrigin.Begin);
                        tempStream.CopyTo(stream.BaseStream);
                    }
                    break;
            }
        }
        /// <summary>
        /// Reads a SnippetsDataObject from the given <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>null if no valid data could be read.</returns>
        internal static SnippetsDataObject? ReadFromStream(BinaryReader stream)
        {
            if (stream == null)
                return null;

            FormatType type = (FormatType)stream.ReadByte();
            string format = stream.ReadString();

            switch (type)
            {
                case FormatType.Text:
                    TextDataFormat textFormat = (TextDataFormat)stream.ReadByte();
                    string textData = stream.ReadString();
                    return new SnippetsDataObject()
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
                    return new SnippetsDataObject()
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
                    return new SnippetsDataObject()
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

                    return new SnippetsDataObject()
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
        /// <summary>
        /// Release resources used by this SnippetsDataObject, if permitted and not currently held hostage by the clipboard.
        /// </summary>
        public void Dispose()
        {
            if (!shouldDispose)
                return;
            if (data == null)
                return;

            shouldDispose = false;

            if(type == FormatType.Audio)
            {
                Stream audioStream = (Stream)data;
                audioStream.Close();
                audioStream.Dispose();
            }
            else if(type == FormatType.Image)
            {
                Image bitmap = (Image)data;
                bitmap.Dispose();
            }
        }
        public override string? ToString()
        {
            if(data == null)
                return $"Snippet Object ({type}, format '{format}')\n\t<no data>";

            if(type == FormatType.Text)
                return $"Snippet Object ({type}, format '{format}', text format '{textFormat}')\n\t{(string)data}";

            if(type == FormatType.FileDropList)
            {
                string[] files = (string[])data;
                return $"Snippet Object ({type}, format '{format}')\n\t{string.Join(",\n\t", files)}";
            }

            return $"Snippet Object ({type}, format '{format}')\n\t{data}";
        }

        public bool IsPreviewString
        {
            get {
                switch(type)
                {
                    case FormatType.Text: return true;
                    case FormatType.FileDropList: return true;
                    case FormatType.Audio: return true;
                    case FormatType.Image: return false;
                    default: return false;
                }
            }
        }
        public string GetPreviewString()
        {
            if (data == null)
                return "Empty";

            switch (type)
            {
                case FormatType.Audio:
                    Stream audioStream = (Stream)data;
                    return "Audio - " + ((double)(audioStream.Length / 1024) / 1024d) + "MB";
                case FormatType.FileDropList:
                    string[] files = (string[])data;
                    IEnumerable<string> modifiedFiles = files.Select(f => "\t- " + f);
                    return "File Collection:\n" + string.Join("\n", modifiedFiles);
                case FormatType.Text:
                    return (string)data;
                case FormatType.Image:
                default:
                    throw new Exception("No text preview available. Consider checking IsPreviewString beforehand.");
            }
        }


        /// <summary>
        /// Pulls a <see cref="SnippetsDataObject"/> from the user's clipboard.
        /// </summary>
        /// <returns>null if there is no data in the clipboard.</returns>
        public static SnippetsDataObject? FromClipboard()
        {
            IDataObject? dataObject = Clipboard.GetDataObject();

            if (dataObject == null)
                return null;

            return new SnippetsDataObject(dataObject);
        }
    }
}
