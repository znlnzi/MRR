using System.Collections;

namespace MRAnalysis.XML
{
    public class XmlNodeList : ArrayList
    {
        public XmlNode Pop()
        {
            XmlNode item = null;

            item = (XmlNode)this[this.Count - 1];
            this.Remove(item);

            return item;
        }

        public int Push(XmlNode item)
        {
            Add(item);

            return this.Count;
        }
    }
}
