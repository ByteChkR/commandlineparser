﻿// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

using CSharpx;

namespace CommandLine.Core
{
    internal static class TypeLookup
    {
        public static Maybe<TypeDescriptor> FindTypeDescriptorAndSibling(string name,
                                                                         IEnumerable<OptionSpecification>
                                                                             specifications,
                                                                         StringComparer comparer)
        {
            Maybe<TypeDescriptor> info =
                specifications.SingleOrDefault(a => name.MatchName(a.ShortName, a.LongName, comparer))
                              .ToMaybe()
                              .Map(first =>
                                   {
                                       TypeDescriptor descr = TypeDescriptor.Create(first.TargetType, first.Max);

                                       Maybe<TypeDescriptor> next = specifications
                                                                    .SkipWhile(s => s.Equals(first))
                                                                    .Take(1)
                                                                    .SingleOrDefault(x => x.IsValue())
                                                                    .ToMaybe()
                                                                    .Map(second =>
                                                                             TypeDescriptor.Create(second.TargetType,
                                                                                  second.Max
                                                                                 )
                                                                        );

                                       return descr.WithNextValue(next);
                                   }
                                  );

            return info;
        }
    }
}
