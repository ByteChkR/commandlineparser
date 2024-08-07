﻿// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

using CSharpx;

using RailwaySharp.ErrorHandling;

namespace CommandLine.Core
{
    internal static class OptionMapper
    {
        public static Result<
                IEnumerable<SpecificationProperty>, Error>
            MapValues(IEnumerable<SpecificationProperty> propertyTuples,
                      IEnumerable<KeyValuePair<string, IEnumerable<string>>> options,
                      Func<IEnumerable<string>, Type, bool, bool, Maybe<object>> converter,
                      StringComparer comparer)
        {
            IEnumerable<Tuple<SpecificationProperty, Maybe<Error>>> sequencesAndErrors = propertyTuples
                .Select(pt =>
                        {
                            Maybe<IEnumerable<KeyValuePair<string, IEnumerable<string>>>> matched = options.Where(s =>
                                        s.Key.MatchName(((OptionSpecification)pt.Specification).ShortName,
                                                        ((OptionSpecification)pt.Specification).LongName,
                                                        comparer
                                                       )
                                    )
                                .ToMaybe();

                            if (matched.IsJust())
                            {
                                IEnumerable<KeyValuePair<string, IEnumerable<string>>> matches =
                                    matched.GetValueOrDefault(Enumerable
                                                                  .Empty<KeyValuePair<string, IEnumerable<string>>>()
                                                             );
                                List<string> values = new List<string>();

                                foreach (KeyValuePair<string, IEnumerable<string>> kvp in matches)
                                {
                                    foreach (string value in kvp.Value)
                                    {
                                        values.Add(value);
                                    }
                                }

                                bool isFlag = pt.Specification.Tag == SpecificationType.Option &&
                                              ((OptionSpecification)pt.Specification).FlagCounter;

                                return converter(values,
                                                 isFlag ? typeof(bool) : pt.Property.PropertyType,
                                                 pt.Specification.TargetType != TargetType.Sequence,
                                                 isFlag
                                                )
                                       .Select(value => Tuple.Create(pt.WithValue(Maybe.Just(value)),
                                                                     Maybe.Nothing<Error>()
                                                                    )
                                              )
                                       .GetValueOrDefault(Tuple.Create<SpecificationProperty, Maybe<Error>>(pt,
                                                               Maybe.Just<Error>(new
                                                                       BadFormatConversionError(((OptionSpecification)
                                                                                    pt.Specification)
                                                                            .FromOptionSpecification()
                                                                           )
                                                                   )
                                                              )
                                                         );
                            }

                            return Tuple.Create(pt, Maybe.Nothing<Error>());
                        }
                       )
                .Memoize();

            return Result.Succeed(sequencesAndErrors.Select(se => se.Item1),
                                  sequencesAndErrors.Select(se => se.Item2)
                                                    .OfType<Just<Error>>()
                                                    .Select(se => se.Value)
                                 );
        }
    }
}
