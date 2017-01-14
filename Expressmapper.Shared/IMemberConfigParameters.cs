using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public interface IMemberConfigParameters
    {
        List<KeyValuePair<MemberExpression, Expression>> CustomMembers { get; }
        List<KeyValuePair<MemberExpression, Expression>> FlattenMembers { get; }
        List<KeyValuePair<MemberExpression, Expression>> CustomFunctionMembers { get; }
        List<string> IgnoreMemberList { get; }
        bool Flattened { get; set; }
        bool CaseSensetiveMember { get; set; }
        bool CaseSensetiveOverride { get; set; }
        CompilationTypes CompilationTypeMember { get; set; }
        bool CompilationTypeOverride { get; set; }
    }
}
