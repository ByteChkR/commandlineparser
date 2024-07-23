// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Reflection;

using CSharpx;
namespace CommandLine.Core
{

    internal class SpecificationProperty
    {

        private SpecificationProperty(Specification specification, PropertyInfo property, Maybe<object> value)
        {
            this.Property = property;
            this.Specification = specification;
            this.Value = value;
        }

        public Specification Specification { get; }

        public PropertyInfo Property { get; }

        public Maybe<object> Value { get; }

        public static SpecificationProperty Create(Specification specification, PropertyInfo property, Maybe<object> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return new SpecificationProperty(specification, property, value);
        }
    }

}
