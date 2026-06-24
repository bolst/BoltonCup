namespace BoltonCup.Auth.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AutoFocusAttribute : Attribute
{
    public bool AutoFocus;

    public AutoFocusAttribute(bool autoFocus)
    {
        AutoFocus = true;
    }
    
    public AutoFocusAttribute() : this(true)
    {
    }
}