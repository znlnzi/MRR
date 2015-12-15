
namespace MRAnalysis.XML
{
    public class XmlParser
    {
        private char _lt = '<';
        private char _gt = '>';
        private char _space = ' ';
        private char _quote = '"';
        private char _quote2 = '\'';
        private char _slash = '/';
        private char _qmark = '?';
        private char _equals = '=';
        private char _exclamation = '!';
        private char _dash = '-';
        //private char SQL  = '[';
        private char _sqr = ']';

        public XmlNode Parse(string content)
        {
            XmlNode rootNode = new XmlNode();
            rootNode["_text"] = "";

            string nodeContents = "";

            bool inElement = false;
            bool collectNodeName = false;
            bool collectAttributeName = false;
            bool collectAttributeValue = false;
            bool quoted = false;
            string attName = "";
            string attValue = "";
            string nodeName = "";
            string textValue = "";

            bool inMetaTag = false;
            bool inComment = false;
            bool inCdata = false;

            XmlNodeList parents = new XmlNodeList();

            XmlNode currentNode = rootNode;

            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                char cn = '~';  // unused char
                char cnn = '~'; // unused char
                char cp = '~';  // unused char

                if ((i + 1) < content.Length) cn = content[i + 1];
                if ((i + 2) < content.Length) cnn = content[i + 2];
                if (i > 0) cp = content[i - 1];

                if (inMetaTag)
                {
                    if (c == _qmark && cn == _gt)
                    {
                        inMetaTag = false;
                        i++;
                    }

                    continue;
                }
                else
                {
                    if (!quoted && c == _lt && cn == _qmark)
                    {
                        inMetaTag = true;
                        continue;
                    }
                }

                if (inComment)
                {
                    if (cp == _dash && c == _dash && cn == _gt)
                    {
                        inComment = false;
                        i++;
                    }

                    continue;
                }
                else
                {
                    if (!quoted && c == _lt && cn == _exclamation)
                    {

                        if (content.Length > i + 9 && content.Substring(i, 9) == "<![CDATA[")
                        {
                            inCdata = true;
                            i += 8;
                        }
                        else
                        {
                            inComment = true;
                        }

                        continue;
                    }
                }

                if (inCdata)
                {
                    if (c == _sqr && cn == _sqr && cnn == _gt)
                    {
                        inCdata = false;
                        i += 2;
                        continue;
                    }

                    textValue += c;
                    continue;
                }


                if (inElement)
                {
                    if (collectNodeName)
                    {
                        if (c == _space)
                        {
                            collectNodeName = false;
                        }
                        else if (c == _gt)
                        {
                            collectNodeName = false;
                            inElement = false;
                        }



                        if (!collectNodeName && nodeName.Length > 0)
                        {
                            if (nodeName[0] == _slash)
                            {
                                // close tag
                                if (textValue.Length > 0)
                                {
                                    currentNode["_text"] += textValue;
                                }

                                textValue = "";
                                nodeName = "";
                                currentNode = parents.Pop();
                            }
                            else
                            {
                                if (textValue.Length > 0)
                                {
                                    currentNode["_text"] += textValue;
                                }

                                textValue = "";
                                XmlNode newNode = new XmlNode();
                                newNode["_text"] = "";
                                newNode["_name"] = nodeName;

                                if (currentNode[nodeName] == null)
                                {
                                    currentNode[nodeName] = new XmlNodeList();
                                }

                                XmlNodeList a = (XmlNodeList)currentNode[nodeName];
                                a.Push(newNode);
                                parents.Push(currentNode);
                                currentNode = newNode;
                                nodeName = "";
                            }
                        }
                        else
                        {
                            nodeName += c;
                        }
                    }
                    else
                    {
                        if (!quoted && c == _slash && cn == _gt)
                        {
                            inElement = false;
                            collectAttributeName = false;
                            collectAttributeValue = false;
                            if (attName.Length > 0)
                            {
                                if (attValue.Length > 0)
                                {
                                    currentNode["@" + attName] = attValue;
                                }
                                else
                                {
                                    currentNode["@" + attName] = true;
                                }
                            }

                            i++;
                            currentNode = parents.Pop();
                            attName = "";
                            attValue = "";
                        }
                        else if (!quoted && c == _gt)
                        {
                            inElement = false;
                            collectAttributeName = false;
                            collectAttributeValue = false;
                            if (attName.Length > 0)
                            {
                                currentNode["@" + attName] = attValue;
                            }

                            attName = "";
                            attValue = "";
                        }
                        else
                        {
                            if (collectAttributeName)
                            {
                                if (c == _space || c == _equals)
                                {
                                    collectAttributeName = false;
                                    collectAttributeValue = true;
                                }
                                else
                                {
                                    attName += c;
                                }
                            }
                            else if (collectAttributeValue)
                            {
                                if (c == _quote || c == _quote2)
                                {
                                    if (quoted)
                                    {
                                        collectAttributeValue = false;
                                        currentNode["@" + attName] = attValue;
                                        attValue = "";
                                        attName = "";
                                        quoted = false;
                                    }
                                    else
                                    {
                                        quoted = true;
                                    }
                                }
                                else
                                {
                                    if (quoted)
                                    {
                                        attValue += c;
                                    }
                                    else
                                    {
                                        if (c == _space)
                                        {
                                            collectAttributeValue = false;
                                            currentNode["@" + attName] = attValue;
                                            attValue = "";
                                            attName = "";
                                        }
                                    }
                                }
                            }
                            else if (c == _space)
                            {

                            }
                            else
                            {
                                collectAttributeName = true;
                                attName = "" + c;
                                attValue = "";
                                quoted = false;
                            }
                        }
                    }
                }
                else
                {
                    if (c == _lt)
                    {
                        inElement = true;
                        collectNodeName = true;
                    }
                    else
                    {
                        textValue += c;
                    }
                }
            }

            return rootNode;
        }
    }
}
