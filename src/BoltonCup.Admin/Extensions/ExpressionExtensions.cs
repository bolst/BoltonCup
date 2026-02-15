using System.Linq.Expressions;
using System.Reflection;

namespace BoltonCup.Admin.Extensions;

/// Credit: MudBlazor (2026/02/15)
/// https://github.com/MudBlazor/MudBlazor/blob/dev/src/MudBlazor/Utilities/Expressions/PropertyPath.cs

public static class ExpressionExtensions
{
    public static PropertyHolder Visit<TSource, TResult>(this Expression<Func<TSource, TResult>> expression)
    {
        var body = expression.Body;
        var visitor = new PropertyVisitor(body is MemberExpression);
        visitor.Visit(body);
        return visitor.PropertyHolder;
    }
}


public sealed class PropertyHolder
{
    private readonly List<MemberInfo> _members;
    public bool IsBodyMemberExpression { get; }
    public IReadOnlyList<MemberInfo> Members => _members;

    public PropertyHolder(bool isBodyMemberExpression)
    {
        IsBodyMemberExpression = isBodyMemberExpression;
        _members = [];
    }

    public void AddMember(MemberInfo member) 
        => _members.Insert(0, member);

    public string GetPath() 
        => string.Join(".", _members.Select(x => x.Name));

    public string GetLastMemberName()
    {
        var lastMemberName = _members
            .Select(x => x.Name)
            .LastOrDefault();
        return string.IsNullOrEmpty(lastMemberName) 
            ? string.Empty 
            : lastMemberName;
    }

    public override string ToString() 
        => GetPath();
}

public sealed class PropertyVisitor(bool isBodyMemberExpression) 
    : ExpressionVisitor
{
    public PropertyHolder PropertyHolder { get; } = new(isBodyMemberExpression);

    protected override Expression VisitMember(MemberExpression node)
    {
        PropertyHolder.AddMember(node.Member);
        return base.VisitMember(node);
    }
}