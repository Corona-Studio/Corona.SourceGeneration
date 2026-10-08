using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Corona.SourceGeneration;

internal static class XmlInputs
{
    /// <summary>Parse tracked AdditionalText content without external entity or disk resolution.</summary>
    public static XDocument Parse(string text)
    {
        using var reader = XmlReader.Create(new StringReader(text), new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        });
        return XDocument.Load(reader);
    }
}
