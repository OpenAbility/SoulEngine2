using System.Reflection;
using SoulEngine.Props;

namespace SoulEngine.Util;

/// <summary>
/// Generic class that aims to wrap <see cref="PropertyInfo"/> and <see cref="FieldInfo"/> instances, but can be expanded upon.
/// </summary>
public abstract class MemberWrapper
{
    private MemberInfo member;

    protected MemberWrapper(MemberInfo member)
    {
        this.member = member;
    }
    
    public abstract object? GetValue(object? target);
    public abstract void SetValue(object? target, object? value);
    
    public abstract Type MemberType { get; }

    public string Name => member.Name;

    public MemberInfo MemberInfo => member;
    
    public abstract bool CanRead { get; }
    public abstract bool CanWrite { get; }

    public static MemberWrapper GetWrapper(PropertyInfo property)
    {
        return new PropertyMemberWrapper(property);
    }
    
    public static MemberWrapper GetWrapper(FieldInfo fieldInfo)
    {
        return new FieldMemberWrapper(fieldInfo);
    }
    
    public static MemberWrapper GetWrapper(MemberInfo memberInfo)
    {
        if (memberInfo is FieldInfo fieldInfo)
            return GetWrapper(fieldInfo);
        if (memberInfo is PropertyInfo propertyInfo)
            return GetWrapper(propertyInfo);
        throw new Exception("Cannot get wrapper for member type " + memberInfo.MemberType);
    }

    public static IEnumerable<MemberWrapper> GetWrappers(Type type, BindingFlags bindingFlags)
    {
       
        foreach (var member in  type.GetMembers(bindingFlags))
        {
            // We only accept fields and properties
            if(member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property)
                continue;
            
            yield return GetWrapper(member);
        }
    }
    
    
    private class PropertyMemberWrapper : MemberWrapper
    {
        private readonly PropertyInfo propertyInfo;


        public PropertyMemberWrapper(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            this.propertyInfo = propertyInfo;
        }

        public override object? GetValue(object? target)
        {
            return propertyInfo.GetValue(target);
        }

        public override void SetValue(object? target, object? value)
        {
            propertyInfo.SetValue(target, value);
        }

        public override Type MemberType => propertyInfo.PropertyType;
        

        public override bool CanRead => propertyInfo.CanRead;
        
        public override bool CanWrite => propertyInfo.CanWrite;
    }
    
    private class FieldMemberWrapper : MemberWrapper
    {
        private readonly FieldInfo fieldInfo;


        public FieldMemberWrapper(FieldInfo fieldInfo) : base(fieldInfo)
        {
            this.fieldInfo = fieldInfo;
        }

        public override object? GetValue(object? target)
        {
            return fieldInfo.GetValue(target);
        }

        public override void SetValue(object? target, object? value)
        {
            fieldInfo.SetValue(target, value);
        }

        public override Type MemberType => fieldInfo.FieldType;

        public override bool CanRead => true;
        
        public override bool CanWrite => !fieldInfo.IsInitOnly;
    }
}

