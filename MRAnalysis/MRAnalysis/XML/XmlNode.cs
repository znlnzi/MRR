/*
 * UnityScript Lightweight XML Parser
 * by Fraser McCormick (unityscripts@roguishness.com)
 * http://twitter.com/flimgoblin
 * http://www.roguishness.com/unity/
 *
 * You may use this script under the terms of either the MIT License 
 * or the Gnu Lesser General Public License (LGPL) Version 3. 
 * See:
 * http://www.roguishness.com/unity/lgpl-3.0-standalone.html
 * http://www.roguishness.com/unity/gpl-3.0-standalone.html
 * or
 * http://www.roguishness.com/unity/MIT-license.txt
 */

/* Usage:
 * parser=new XMLParser();
 * var node=parser.Parse("<example><value type=\"String\">Foobar</value><value type=\"Int\">3</value></example>");
 * 
 * Nodes are Boo.Lang.Hash values with text content in "_text" field, other attributes
 * in "@attribute" and any child nodes listed in an array of their nodename.
 * 
 * any XML meta tags <? .. ?> are ignored as are comments <!-- ... -->
 * any CDATA is bundled into the "_text" attribute of its containing node.
 *
 * e.g. the above XML is parsed to:
 * node={ "example": 
 *			[ 
 *			   { "_text":"", 
 *				  "value": [ { "_text":"Foobar", "@type":"String"}, {"_text":"3", "@type":"Int"}]
 *			   } 
 *			],
 *		  "_text":""
 *     }
 *		  
 */

using System.Collections;

namespace MRAnalysis.XML
{
    public class XmlNode : Hashtable
    {
        public XmlNodeList GetNodeList(string path)
        {
            return GetObject(path) as XmlNodeList;
        }

        public XmlNode GetNode(string path)
        {
            return GetObject(path) as XmlNode;
        }

        public string GetValue(string path)
        {
            return GetObject(path) as string;
        }

        private object GetObject(string path)
        {
            string[] bits = path.Split('>');
            XmlNode currentNode = this;
            XmlNodeList currentNodeList = null;
            bool listMode = false;
            object ob;

            for (int i = 0; i < bits.Length; i++)
            {
                if (listMode)
                {
                    currentNode = (XmlNode)currentNodeList[int.Parse(bits[i])];
                    ob = currentNode;
                    listMode = false;
                }
                else
                {
                    ob = currentNode[bits[i]];

                    if (ob is ArrayList)
                    {
                        currentNodeList = (XmlNodeList)(ob as ArrayList);
                        listMode = true;
                    }
                    else
                    {
                        // reached a leaf node/attribute
                        if (i != (bits.Length - 1))
                        {
                            // unexpected leaf node
                            string actualPath = "";
                            for (int j = 0; j <= i; j++)
                            {
                                actualPath = actualPath + ">" + bits[j];
                            }
                        }

                        return ob;
                    }
                }
            }

            if (listMode)
                return currentNodeList;
            else
                return currentNode;
        }
    }
}
