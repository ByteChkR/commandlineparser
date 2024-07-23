// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CommandLine.Infrastructure;

using CSharpx;
namespace CommandLine.Core
{

    internal enum SpecificationType
    {
        Option,
        Value,
    }

    internal enum TargetType
    {
        Switch,
        Scalar,
        Sequence,
    }

    internal abstract class Specification
    {

        protected Specification(SpecificationType tag,
            bool required,
            Maybe<int> min,
            Maybe<int> max,
            Maybe<object> defaultValue,
            string helpText,
            string metaValue,
            IEnumerable<string> enumValues,
            Type conversionType,
            TargetType targetType,
            bool hidden = false)
        {
            this.Tag = tag;
            this.Required = required;
            this.Min = min;
            this.Max = max;
            this.DefaultValue = defaultValue;
            this.ConversionType = conversionType;
            this.TargetType = targetType;
            this.HelpText = helpText;
            this.MetaValue = metaValue;
            this.EnumValues = enumValues;
            this.Hidden = hidden;
        }

        public SpecificationType Tag { get; }

        public bool Required { get; }

        public Maybe<int> Min { get; }

        public Maybe<int> Max { get; }

        public Maybe<object> DefaultValue { get; }

        public string HelpText { get; }

        public string MetaValue { get; }

        public IEnumerable<string> EnumValues { get; }

        /// This information is denormalized to decouple Specification from PropertyInfo.
        public Type ConversionType { get; }

        public TargetType TargetType { get; }

        public bool Hidden { get; }

        public static Specification FromProperty(PropertyInfo property)
        {
            object[] attrs = property.GetCustomAttributes(true);
            IEnumerable<OptionAttribute> oa = attrs.OfType<OptionAttribute>();
            if (oa.Count() == 1)
            {
                OptionSpecification spec = OptionSpecification.FromAttribute(
                    oa.Single(),
                    property.PropertyType,
                    ReflectionHelper.GetNamesOfEnum(property.PropertyType)
                );

                if (spec.ShortName.Length == 0 && spec.LongName.Length == 0)
                {
                    return spec.WithLongName(property.Name.ToLowerInvariant());
                }
                return spec;
            }

            IEnumerable<ValueAttribute> va = attrs.OfType<ValueAttribute>();
            if (va.Count() == 1)
            {
                return ValueSpecification.FromAttribute(
                    va.Single(),
                    property.PropertyType,
                    property.PropertyType.GetTypeInfo().IsEnum
                        ? Enum.GetNames(property.PropertyType)
                        : Enumerable.Empty<string>()
                );
            }

            throw new InvalidOperationException();
        }
    }

}
