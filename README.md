[![Build status](https://ci.appveyor.com/api/projects/status/github/BizTalkComponents/scriptcomponent?branch=master)](https://ci.appveyor.com/api/projects/status/github/BizTalkComponents/scriptcomponent/branch/master)

##Description
Executes a C# snippet that is sent in as a parameter.

This component is useful when you need to perform any simple action on either the message or the message context, like promoting a property or looking up a value in a database.

| Parameter       | Description                         | Type| Validation|
| ----------------|-------------------------------------|-----|-----------|
|Snippet|A C# snippet that should be executed. The snippet should be defined as an anonymous method taking two parameters. The first parameter is the message context and the second is the message represented as a ReadOnlySeekableStream. The method should return a ReadonlySeekableStream. See the samples below|String|Required|

## Remarks ##
Throws ArgumentException if any of the required parameters is not specified.

## Examples ##
```c#
//Promotes the value Test to the property Namespace#Name
(ctx, msg) => 
{
    ctx.Promote(\"Name\",\"Namespace\", \"Test\"); return msg;
};
```
```c#
//Promotes value from message body at xpath 'testmessage1' to the property Namespace#Name2
(ctx, msg) => 
{
    XmlTextReader xmlTextReader = new XmlTextReader(msg);
    XPathCollection xPathCollection = new XPathCollection();
    XPathReader xPathReader = new XPathReader(xmlTextReader, xPathCollection);
    xPathCollection.Add("/*[local-name() = 'testmessage1']");
    string value = string.Empty;

    while (xPathReader.ReadUntilMatch())
    {
        if (xPathReader.Match(0))
        {
            if (xPathReader.NodeType == XmlNodeType.Attribute)
            {
                value = xPathReader.GetAttribute(xPathReader.Name);
            }
            else
            {
                value = xPathReader.ReadString();
            }
            
            ctx.Promote("Name2","Namespace", value);
            break;
        }
    }
    
    if (string.IsNullOrEmpty(value))
    {
        throw new InvalidOperationException("The specified XPath did not exist or contained an empty value.");
    }
 
    msg.Position = 0;

    return msg;
};
```
