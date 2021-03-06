﻿using System;
using System.Reflection;
using AutoFixture.AutoNSubstitute.Extensions;
using AutoFixture.Kernel;
using NSubstitute.Core;
using NSubstitute.Exceptions;

namespace AutoFixture.AutoNSubstitute
{
    /// <summary>
    /// If the type of the object being substituted contains any fields and/or non-virtual/sealed
    /// settable properties, this initializer will resolve them from a given context.
    /// </summary>
    public class NSubstituteSealedPropertiesCommand : ISpecimenCommand
    {
        private static readonly AutoPropertiesCommand AutoPropertiesCommand =
            new AutoPropertiesCommand(new NSubstituteSealedPropertySpecification());

        /// <summary>
        /// If the type of the object being substituted contains any fields and/or non-virtual/sealed
        /// settable properties, this initializer will resolve them from a given context.
        /// </summary>
        /// <param name="specimen">The substitute object.</param>
        /// <param name="context">The context.</param>
        public void Execute(object specimen, ISpecimenContext context)
        {
            if (specimen == null) throw new ArgumentNullException(nameof(specimen));
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                SubstitutionContext.Current.GetCallRouterFor(specimen);
            }
            catch (NotASubstituteException)
            {
                return;
            }

            AutoPropertiesCommand.Execute(specimen, context);
        }

        private class NSubstituteSealedPropertySpecification : IRequestSpecification
        {
            /// <summary>
            /// Satisfied by any fields and non-virtual/sealed properties.
            /// </summary>
            public bool IsSatisfiedBy(object request)
            {
                switch (request)
                {
                    // exclude non-sealed properties
                    case PropertyInfo pi:
                        return pi.GetSetMethod().IsSealed();

                    // exclude interceptor fields
                    case FieldInfo fi:
                        return !IsDynamicProxyMember(fi);
                }

                return false;
            }

            /// <summary>
            /// Checks whether a <see cref="FieldInfo"/> belongs to a dynamic proxy.
            /// </summary>
            private static bool IsDynamicProxyMember(FieldInfo fi)
            {
                return string.Equals(fi.Name, "__interceptors", StringComparison.Ordinal) ||
                       string.Equals(fi.Name, "__mixin_NSubstitute_Core_ICallRouter", StringComparison.Ordinal);
            }
        }
    }
}
