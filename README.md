#Atlas Xml Serializer

Atlas Xml Serializer is a fast and easy to use library for .net Applications, like .net's standart serialization. Primary scope of this library is to serialize/deserialize objects with no additinal attributing/coding effort, in high performance way.

###Why did I code this?

The short answer is; I'm lazy :) For many years, I've used .net's serialization. Later me and my collegue decided to write custom methods on data classes for better performance & flexibility (like versioning, inherited classes etc). Doing so, we had the control over serialization, but had below disadvantages;

 - Had to write serialization code for each property / field, 
 - Node names were hardcoded (it was before nameof()), so refactoring was pain in the a..rms, 
 - Serialization code was larger than data code, 

After few years, I see that we're writing the same code for each data class over and over again. Also it was a "mine field" where you can easily forget to code serialization for a property, or copy paste the code but forget to change attribute name etc. 

That's why I decided to code this library. So that I don't need to code anymore for serialization, but the library will do it for me. 

## How it works?

Basically, you use a serializer class with static methods to serialize / deserialize.  

    string serialized = Serializer.Serialize(myClassInstance); 
    var b = Serializer.Deserialize<MyClass>(serialized);

Same as many libraries eh? Let me describe how it works with a list;

 - Uses .net's XmlReader / XmlWriter  for serialization
 - Like .net's, serialization is done by dynamically compiled serializers (one for each type)
 - Serializers are cached, so compilation will occour only on first usage
 - No attributes are needed (even [Serializable] etc)
 - Has XmlSerializationMember and and XmlSerializationType attributes that join all serialization features
 - Reads .Net attributes also (Like XmlElement, XmlArrayItem etc)
 - Can serialize / deserialize inherited types (even type of object can be serialized)
 - Faster.. (Compared to .net's, ~30% faster for flat types, ~60% faster for lists etc)
 - Supports serialization of dictionaries (IDictionary)
 - Supports attribute overrides/defaults for types that are not in our control (e.g. System.List)
 - You may write custom serialization methods for single property, or for whole type.
 - Privates, even static properties/fields can be serialized.
 - Support for CData and Auto node types, where compiler will decide by type of member
 
## What's not included (yet)?
Current version is enough for my projects, so I didn't code some features. I'll try to keep the library up, with feature requests.

Below are my notes about current rules / missing features. I'm writing to say "I'm aware of them" with my explanations.

 - .net 4.5.1 is used in library

It's all because nameof() operator.. I tought, what if nobody use this library for 2 years and then after, I'll never need to downgrade?
 - One type can only be serialized in one way.

Since serializer is compiled on first usage, without un-registering/changing current serializer, or providing a custom serializer, it'll always use same xml structure for that type. (Only xml node types, not the names. Also not inheriting types, they'll have their own serializers). 
 - Two types cannot be join in one xml node. 

Dictionary is an exception to this, where a child may contain both the key and the value. Otherwise, one xml element must be about one type.
 - Nulls are not serialized 

If a property/field is null, it's not written to xml. There are some exceptions to this, like List with a null value writes special attribute to child node that marks xml element as null value. 
 - Defaults are not checked

Only nullable types are checked agains null during serialization. Default values (I mean the [DefaultValue] attribute) are not checked. In the feature I'll add this feature if requested by me or others.
 - If exists, type attribute must be the first attribute on element

Serializer will write an attribute like _type="MyInheritingClass" if needed, to decide using inheriting classes serializer on deserialization. This attribute check is done for first attribute only. This is for better performance on deserialization. (Sealed classes/value types does not have this check at all)
 - Enumerables must have it's own root

.Net serializer supports this, but it looks not right to me to include A.B[0] as an element under A.
 - No depth / circular reference checks are done

So, be careful :) You need to mark such properties/fields with [XmlIgnore] or [NonSerialized] so that they are not serialized into xml.
 - If enumerable does not have any properties / fields for serialization, child element names are not checked.
 - Pre-compiled serializer assembly generation is not supported 

Didn't think about it yet, but feel like it's needed. 
