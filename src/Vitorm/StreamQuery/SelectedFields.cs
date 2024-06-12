using System.Linq;

using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.StreamQuery
{
    public class SelectedFields
    {
        // root value of ExpressionNode_Member is IStream
        public ExpressionNode_New fields;

        public bool? isDefaultSelect { get; set; }
        internal bool TryGetField(string fieldName, out ExpressionNode field)
        {
            field = null;

            var fieldInfo = fields?.memberArgs?.FirstOrDefault(m => m.name == fieldName);

            fieldInfo ??= fields?.constructorArgs?.FirstOrDefault(m => m.name == fieldName);

            if (fieldInfo != null)
            {
                field = fieldInfo.value;
                return true;
            }
            return false;
        }
    }
}
