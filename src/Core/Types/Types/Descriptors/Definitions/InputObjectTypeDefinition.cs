using HotChocolate.Language;

namespace HotChocolate.Types.Descriptors.Definitions
{
    public class InputObjectTypeDefinition
        : TypeDefinitionBase<InputObjectTypeDefinitionNode>
    {
        public IBindableList<InputFieldDefinition> Fields { get; }
            = new BindableList<InputFieldDefinition>();        
    }
}