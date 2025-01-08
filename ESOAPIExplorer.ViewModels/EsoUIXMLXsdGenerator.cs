// TODO: Mucho debugging required

//using ESOAPIExplorer.Models;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text.Json;
//using System.Xml;

//namespace ESOAPIExplorer.ViewModels;

//public class EsoUIXMLXsdGeneratorService
//{
//    public EsoUIDocumentation Documentation { get; set; }

//    private const string INDENT = "    ";
//    private const string EOL = "\n";

//    private const int ELEMENT_NODE = 1;
//    private const int COMMENT_NODE = 8;

//    private dynamic config;
//    private int version;
//    private Dictionary<string, EsoUIXMLElement> layout;
//    private Dictionary<string, List<string>> globals;
//    private HashSet<string> definedElements;
//    private HashSet<string> usedAttributeTypes;
//    private XmlDocument document;
//    private List<Action> sectionOutput;
//    private List<Action> output;

//    public void Initialise()
//    {
//        version = Documentation.ApiVersion;
//        layout = Documentation.XmlLayout;
//        globals = Documentation.Globals;

//        string configContent = File.ReadAllText("xsdConfig.json");
//        config = JsonSerializer.Deserialize<dynamic>(configContent);
//    }

//    private void AppendChild(XmlElement node, XmlElement child, int indent = 1)
//    {
//        switch (child.NodeType)
//        {
//            case XmlNodeType.Element:
//            case XmlNodeType.Comment:
//                node.AppendChild(CreateIndent(indent));
//                node.AppendChild(child);

//                if (child.HasChildNodes)
//                {
//                    child.InsertBefore(CreateLineBreak(), child.FirstChild);
//                    child.AppendChild(CreateIndent(indent));
//                }

//                node.AppendChild(CreateLineBreak());
//                break;
//            default:
//                node.AppendChild(child);
//                break;
//        }
//    }

//    private XmlText CreateIndent(int level)
//    {
//        return document.CreateTextNode(new string(' ', level * INDENT.Length));
//    }

//    private XmlText CreateLineBreak(int num = 1)
//    {
//        return document.CreateTextNode(new string('\n', num));
//    }

//    private XmlElement CreateXmlElement(string name, string type)
//    {
//        XmlElement node = document.CreateElement("xs:element");

//        name = config.elementNameRename[name] ?? name;
//        node.SetAttribute("name", name);
//        node.SetAttribute("type", type);

//        return node;
//    }

//    private void AppendAllAttributes(XmlElement parent, List<EsoUIArgument> attributes, int indent)
//    {
//        if (attributes != null)
//        {
//            foreach (EsoUIArgument attributeData in attributes)
//            {
//                string type = config.attributeTypeRename[attributeData.Type.Type] ?? attributeData.Type.Type;
//                XmlElement attributeElement = document.CreateElement("xs:attribute");

//                attributeElement.SetAttribute("name", attributeData.Name);
//                attributeElement.SetAttribute("type", type);

//                AppendChild(parent, attributeElement, indent);
//            }
//        }
//    }

//    private XmlElement CreateComplexType(EsoUIXMLElement nodeData, string nameOverride = null)
//    {
//        XmlElement typeElement = document.CreateElement("xs:complexType");
//        typeElement.SetAttribute("name", nameOverride ?? nodeData.Name);

//        if (nodeData.Children != null)
//        {
//            XmlElement choiceElement = document.CreateElement("xs:choice");
//            choiceElement.SetAttribute("minOccurs", "0");
//            choiceElement.SetAttribute("maxOccurs", "unbounded");

//            foreach (EsoUIXMLElement childData in nodeData.Children)
//            {
//                if (!config.ignoredChildElements.ContainsKey(childData.Name))
//                {
//                    XmlElement childElement = CreateXmlElement(childData.Name, childData.Type);
//                    AppendChild(choiceElement, childElement, 3);
//                }
//            }

//            if (choiceElement.HasChildNodes)
//            {
//                AppendChild(typeElement, choiceElement, 2);
//            }
//        }

//        AppendAllAttributes(typeElement, nodeData.Attributes, 2);
//        return typeElement;
//    }

//    private XmlElement CreateComplexSimpleType(EsoUIXMLElement nodeData)
//    {
//        XmlElement extensionElement = document.CreateElement("xs:extension");
//        extensionElement.SetAttribute("base", "xs:string");

//        AppendAllAttributes(extensionElement, nodeData.Attributes, 4);

//        XmlElement contentElement = document.CreateElement("xs:simpleContent");
//        AppendChild(contentElement, extensionElement, 3);

//        XmlElement typeElement = document.CreateElement("xs:complexType");
//        typeElement.SetAttribute("name", nodeData.Name);
//        AppendChild(typeElement, contentElement, 2);

//        return typeElement;
//    }

//    private void AppendDocumentation(XmlElement parent, string text, int indent)
//    {
//        XmlElement annotationElement = document.CreateElement("xs:annotation");
//        XmlElement documentationElement = document.CreateElement("xs:documentation");

//        documentationElement.AppendChild(document.CreateTextNode(new string(' ', (indent + 2) * INDENT.Length) + text + "\n"));

//        AppendChild(annotationElement, documentationElement, indent + 1);
//        AppendChild(parent, annotationElement, indent);
//    }

//    private XmlElement CreateComplexExtensionType(EsoUIXMLElement nodeData, string parentTypeOverride = null)
//    {
//        string parentType = parentTypeOverride ?? nodeData.Parent.Type;

//        XmlElement typeElement = document.CreateElement("xs:complexType");
//        typeElement.SetAttribute("name", nodeData.Name);

//        XmlElement extensionElement = document.CreateElement("xs:extension");
//        extensionElement.SetAttribute("base", parentType);

//        if (nodeData.Documentation != null)
//        {
//            AppendDocumentation(typeElement, nodeData.Documentation, 2);
//        }
//        else
//        {
//            XmlElement choiceElement = document.CreateElement("xs:choice");
//            choiceElement.SetAttribute("minOccurs", "0");
//            choiceElement.SetAttribute("maxOccurs", "unbounded");

//            if (parentType != null)
//            {
//                XmlElement groupElement = document.CreateElement("xs:group");
//                groupElement.SetAttribute("ref", parentType + "Elements");
//                AppendChild(choiceElement, groupElement, 5);
//            }

//            if (nodeData.Children != null)
//            {
//                foreach (EsoUIXMLElement childData in nodeData.Children)
//                {
//                    if (!definedElements.Contains(childData.Name))
//                    {
//                        XmlElement childElement = CreateXmlElement(childData.Name, childData.Type);
//                        AppendChild(choiceElement, childElement, 5);
//                    }
//                }
//            }

//            if (choiceElement.HasChildNodes)
//            {
//                AppendChild(extensionElement, choiceElement, 4);
//            }
//        }

//        AppendAllAttributes(extensionElement, nodeData.Attributes, 4);

//        XmlElement contentElement = document.CreateElement(nodeData.Documentation != null ? "xs:simpleContent" : "xs:complexContent");

//        AppendChild(contentElement, extensionElement, 3);
//        AppendChild(typeElement, contentElement, 2);

//        return typeElement;
//    }

//    private XmlElement CreateEnumType(string name, List<string> enumValues)
//    {
//        string prefix = config.enumPrefixes[name];

//        XmlElement typeElement = document.CreateElement("xs:simpleType");
//        typeElement.SetAttribute("name", name);
//        XmlElement unionElement = document.CreateElement("xs:union");

//        XmlElement integerTypeElement = document.CreateElement("xs:simpleType");
//        XmlElement integerRestrictionElement = document.CreateElement("xs:restriction");
//        integerRestrictionElement.SetAttribute("base", "integer");

//        XmlElement enumTypeElement = document.CreateElement("xs:simpleType");
//        XmlElement enumRestrictionElement = document.CreateElement("xs:restriction");
//        enumRestrictionElement.SetAttribute("base", "xs:string");

//        string newValue;

//        foreach (string value in enumValues)
//        {
//            newValue = value;
//            XmlElement enumValueElement = document.CreateElement("xs:enumeration");

//            if (prefix != null)
//            {
//                newValue = value.Substring(prefix.Length);
//            }

//            enumValueElement.SetAttribute("value", newValue);

//            AppendChild(enumRestrictionElement, enumValueElement, 5);
//        }

//        AppendChild(integerTypeElement, integerRestrictionElement, 4);
//        AppendChild(enumTypeElement, enumRestrictionElement, 4);
//        AppendChild(unionElement, integerTypeElement, 3);
//        AppendChild(unionElement, enumTypeElement, 3);
//        AppendChild(typeElement, unionElement, 2);

//        return typeElement;
//    }

//    private XmlElement CreateElementGroup(List<EsoUIType> children, string name)
//    {
//        XmlElement choiceElement = document.CreateElement("xs:choice");

//        foreach (EsoUIType childData in children)
//        {
//            XmlElement childElement = CreateXmlElement(childData.Name, childData.Type);
//            AppendChild(choiceElement, childElement, 3);
//        }

//        XmlElement groupElement = document.CreateElement("xs:group");

//        groupElement.SetAttribute("name", name);

//        AppendChild(groupElement, choiceElement, 2);

//        return groupElement;
//    }

//    private (List<string>, Dictionary<string, List<EsoUIXMLElement>>, List<EsoUIXMLElement>) FindRemainingElementTypes(Dictionary<string, EsoUIXMLElement> layout)
//    {
//        Dictionary<string, List<EsoUIXMLElement>> subTypes = new Dictionary<string, List<EsoUIXMLElement>>();
//        List<EsoUIXMLElement> basicTypes = new List<EsoUIXMLElement>();

//        foreach (KeyValuePair<string, EsoUIXMLElement> kvp in layout)
//        {
//            string key = kvp.Key;
//            EsoUIXMLElement data = kvp.Value;

//            if (!definedElements.Contains(key))
//            {
//                if (data.Parent != null)
//                {
//                    string type = data.Parent.Type;

//                    if (!subTypes.ContainsKey(type))
//                    {
//                        subTypes[type] = new List<EsoUIXMLElement>();
//                    }
//                    subTypes[type].Add(data);
//                }
//                else
//                {
//                    basicTypes.Add(data);
//                }
//            }
//        }

//        List<string> baseTypes = new List<string>(subTypes.Keys);
//        baseTypes.Sort();

//        return (baseTypes, subTypes, basicTypes);
//    }

//    private void CreateSection(List<Action> output, string title)
//    {
//        output.Add(() =>
//        {
//            output.Add(() => document.CreateComment($" {title} "));
//        });
//    }

//    private void CreateRootNode(List<Action> output, EsoUIXMLElement nodeData)
//    {
//        output.Add(() =>
//        {
//            output.Add(() => CreateComplexType(nodeData, nodeData.Name + "Type"));
//            output.Add(() => CreateXmlElement(nodeData.Name, nodeData.Name + "Type"));
//        });
//    }

//    private void CreateComplexTypeNode(List<Action> output, EsoUIXMLElement nodeData)
//    {
//        output.Add(() => CreateComplexType(nodeData));
//    }

//    private void CreateComplexBaseTypeNode(List<Action> output, EsoUIXMLElement nodeData)
//    {
//        output.Add(() =>
//        {
//            output.Add(() => CreateElementGroup(nodeData.Children, nodeData.Name + "TypeElements"));
//            EsoUIXMLElement typeElement = new EsoUIXMLElement(nodeData.Name + "Type")
//            {
//                Attributes = nodeData.Attributes
//            };
//            output.Add(() => CreateComplexType(typeElement));
//            EsoUIXMLElement emptyElement = new EsoUIXMLElement(nodeData.Name);
//            output.Add(() => CreateComplexExtensionType(emptyElement, nodeData.Name + "Type"));
//        });
//    }

//    private void CreateComplexSimpleTypeNode(List<Action> output, EsoUIXMLElement nodeData)
//    {
//        output.Add(() => CreateComplexSimpleType(nodeData));
//    }

//    private void CreateComplexExtensionTypeNode(List<Action> output, EsoUIXMLElement nodeData)
//    {
//        string parentType = config.parentTypeRename[nodeData.Parent.Type] ?? nodeData.Parent.Type;
//        output.Add(() => CreateComplexExtensionType(nodeData, parentType));
//    }

//    private void CreateEnumTypeNode(List<Action> output, EsoUIEnumTypeData nodeData)
//    {
//        string name = config.attributeTypeRename[nodeData.Name] ?? nodeData.Name;
//        output.Add(() => CreateEnumType(name, nodeData.Values));
//    }

//    private void InitializeDocument()
//    {
//        sectionOutput = new List<Action>();
//        output = new List<Action>();

//        string template = File.ReadAllText(config.templateFile);
//        document = new XmlDocument();
//        document.LoadXml(template);
//    }

//    private void FinaliseDocument()
//    {
//        XmlNode rootNode = document.GetElementsByTagName("xs:schema")[0];
//        List<XmlElement> elements = new List<XmlElement>();

//        foreach (Action node in output)
//        {
//            node();

//            foreach (XmlElement element in elements)
//            {
//                AppendChild((XmlElement)rootNode, element);
//            }

//            elements.Clear();
//        }
//    }

//    private void InitializeDefinedElements()
//    {
//        usedAttributeTypes = new HashSet<string>();
//        definedElements = new HashSet<string>();

//        foreach (string name in config.ignoredElements.Keys)
//        {
//            definedElements.Add(name);
//        }
//    }

//    private void SetElementDefined(EsoUIXMLElement data)
//    {
//        SetAllUsedAttributes(data);
//        definedElements.Add(data.Name);
//    }

//    private void SetAllUsedAttributes(EsoUIXMLElement data)
//    {
//        if (data.Attributes != null)
//        {
//            foreach (EsoUIArgument attributeData in data.Attributes)
//            {
//                usedAttributeTypes.Add(attributeData.Type.Type);
//            }
//        }
//    }

//    private void StartSection(string title)
//    {
//        sectionOutput.Clear();
//        AddNode(CreateSection, title);
//    }

//    private void EndSection()
//    {
//        output.InsertRange(0, sectionOutput);
//    }

//    private void AddNode(Action factory, object data)
//    {
//        sectionOutput.Add(() => factory());
//    }

//    private void CreateElementSectionNodes(List<string> elements, Func<EsoUIXMLElement, Action> getFactory)
//    {
//        foreach (string element in elements)
//        {
//            if (layout.ContainsKey(element))
//            {
//                EsoUIXMLElement data = layout[element];
//                AddNode(getFactory(data), data);
//                SetElementDefined(data);
//            }
//        }
//    }

//    private void CreateTypeSectionNodes(List<EsoUIXMLElement> types, Action<EsoUIXMLElement> factory)
//    {
//        foreach (EsoUIXMLElement data in types)
//        {
//            if (!definedElements.Contains(data.Name))
//            {
//                AddNode(() => factory(data), data);
//                SetAllUsedAttributes(data);
//            }
//        }
//    }

//    private void CreateAttributeSectionNodes(HashSet<string> attributeTypes, Action<EsoUIEnumTypeData> factory)
//    {
//        List<string> usedAttributeTypes = new List<string>(attributeTypes);
//        usedAttributeTypes.Sort();

//        foreach (string name in usedAttributeTypes)
//        {
//            if (globals.ContainsKey(name))
//            {
//                EsoUIEnumTypeData nodeData = new EsoUIEnumTypeData
//                {
//                    Name = name,
//                    Values = globals[name]
//                };

//                AddNode(() => factory(nodeData), nodeData);
//            }
//        }
//    }

//    private StreamWriter CreateWriter(string fileName)
//    {
//        return new StreamWriter(fileName, false);
//    }

//    public void Generate(string outputDir)
//    {
//        InitializeDefinedElements();
//        InitializeDocument();

//        StartSection("root element");
//        EsoUIXMLElement data = layout["GuiXml"];
//        AddNode(() => CreateRootNode(output, data), data);
//        SetElementDefined(data);
//        EndSection();

//        StartSection("container elements");
//        CreateElementSectionNodes(config.containers, data => CreateComplexTypeNode);
//        EndSection();

//        StartSection("other elements");
//        CreateElementSectionNodes(config.others, data => CreateComplexTypeNode);
//        EndSection();

//        (List<string> baseTypes, Dictionary<string, List<EsoUIXMLElement>> subTypes, List<EsoUIXMLElement> basicTypes) = FindRemainingElementTypes(layout);

//        StartSection("element basetypes");
//        CreateElementSectionNodes(baseTypes, data => data.Children != null && data.Children.Count > 0 ? CreateComplexBaseTypeNode : CreateComplexSimpleTypeNode);
//        EndSection();

//        StartSection("element types");

//        foreach (string baseType in baseTypes)
//        {
//            AddNode(() => CreateSection(output, $"{baseType} element types"), null);
//            CreateTypeSectionNodes(subTypes[baseType], CreateComplexExtensionTypeNode);
//        }

//        EndSection();

//        StartSection("basic element types");
//        CreateTypeSectionNodes(basicTypes, CreateComplexTypeNode);
//        EndSection();

//        StartSection("basic attribute types");
//        CreateAttributeSectionNodes(usedAttributeTypes, CreateEnumTypeNode);
//        EndSection();

//        FinaliseDocument();

//        string xsdFile = Path.Combine(outputDir, $"esoui{version}.xsd");
//        Console.WriteLine("write xsd file", xsdFile);

//        using (StreamWriter writer = CreateWriter(xsdFile))
//        {
//            XmlSerializer serializer = new XmlSerializer(typeof(XmlDocument));
//            serializer.Serialize(writer, document);
//        }
//    }
//}
