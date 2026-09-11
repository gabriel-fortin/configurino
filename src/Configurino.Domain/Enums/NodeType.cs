namespace org.g14.Configurino.Domain.Enums;

public enum NodeType
{
    /// Should never be used 
    Unset,
    
    /// The root of the entire configuration tree
    Root, // maybe not needed but maybe useful to find the tree's root quickly
    
    /// A folder in the tree; it exists for ease of management; does not represent an actual config
    Grouping,
    
    /// An object config node – a node that exists in some appsettings file as an object
    IntermediateObject,
    
    /// An array config node – a node that exists in some appsettings file as an array
    IntermediateArray,
    
    // A value config node – a node that exists in some appsettings file as a value
    Leaf,
}