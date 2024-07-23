using System;
using System.Reflection;
namespace CommandLine.Infrastructure
{

    internal class LocalizableAttributeProperty
    {
        private PropertyInfo _localizationPropertyInfo;
        private readonly string _propertyName;
        private Type _type;
        private string _value;

        public LocalizableAttributeProperty(string propertyName)
        {
            _propertyName = propertyName;
        }

        public string Value
        {
            get => GetLocalizedValue();
            set
            {
                _localizationPropertyInfo = null;
                _value = value;
            }
        }

        public Type ResourceType
        {
            set
            {
                _localizationPropertyInfo = null;
                _type = value;
            }
        }

        private string GetLocalizedValue()
        {
            if (string.IsNullOrEmpty(_value) || _type == null)
            {
                return _value;
            }
            if (_localizationPropertyInfo == null)
            {
                // Static class IsAbstract
                if (!_type.IsVisible)
                {
                    throw new ArgumentException($"Invalid resource type '{_type.FullName}'! {_type.Name} is not visible for the parser! Change resources 'Access Modifier' to 'Public'", _propertyName);
                }
                PropertyInfo propertyInfo = _type.GetProperty(_value, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Static);
                if (propertyInfo == null || !propertyInfo.CanRead || propertyInfo.PropertyType != typeof(string) && !propertyInfo.PropertyType.CanCast<string>())
                {
                    throw new ArgumentException($"Invalid resource property name! Localized value: {_value}", _propertyName);
                }
                _localizationPropertyInfo = propertyInfo;
            }

            return _localizationPropertyInfo.GetValue(null, null).Cast<string>();
        }
    }

}
