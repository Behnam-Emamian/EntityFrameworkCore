﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class RelationalForeignKeyExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool AreCompatible([NotNull] this IForeignKey foreignKey, [NotNull] IForeignKey duplicateForeignKey, bool shouldThrow)
        {
            var principalAnnotations = foreignKey.PrincipalEntityType.Relational();
            var duplicatePrincipalAnnotations = duplicateForeignKey.PrincipalEntityType.Relational();
            if (!string.Equals(principalAnnotations.Schema, duplicatePrincipalAnnotations.Schema, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(principalAnnotations.TableName, duplicatePrincipalAnnotations.TableName, StringComparison.OrdinalIgnoreCase))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyPrincipalTableMismatch(
                            foreignKey.Properties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            duplicateForeignKey.Properties.Format(),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            Format(foreignKey.DeclaringEntityType.Relational()),
                            foreignKey.Relational().ConstraintName,
                            Format(principalAnnotations),
                            Format(duplicatePrincipalAnnotations)));
                }

                return false;
            }

            if (!foreignKey.Properties.Select(p => p.Relational().ColumnName)
                .SequenceEqual(duplicateForeignKey.Properties.Select(p => p.Relational().ColumnName)))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyColumnMismatch(
                            foreignKey.Properties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            duplicateForeignKey.Properties.Format(),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            Format(foreignKey.DeclaringEntityType.Relational()),
                            foreignKey.Relational().ConstraintName,
                            foreignKey.Properties.FormatColumns(),
                            duplicateForeignKey.Properties.FormatColumns()));
                }

                return false;
            }

            if (!foreignKey.PrincipalKey.Properties
                .Select(p => p.Relational().ColumnName)
                .SequenceEqual(
                    duplicateForeignKey.PrincipalKey.Properties
                        .Select(p => p.Relational().ColumnName)))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyPrincipalColumnMismatch(
                            foreignKey.Properties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            duplicateForeignKey.Properties.Format(),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            Format(foreignKey.DeclaringEntityType.Relational()),
                            foreignKey.Relational().ConstraintName,
                            foreignKey.PrincipalKey.Properties.FormatColumns(),
                            duplicateForeignKey.PrincipalKey.Properties.FormatColumns()));
                }

                return false;
            }

            if (foreignKey.IsUnique != duplicateForeignKey.IsUnique)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyUniquenessMismatch(
                            foreignKey.Properties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            duplicateForeignKey.Properties.Format(),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            Format(foreignKey.DeclaringEntityType.Relational()),
                            foreignKey.Relational().ConstraintName));
                }

                return false;
            }

            if (foreignKey.DeleteBehavior != duplicateForeignKey.DeleteBehavior)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyDeleteBehaviorMismatch(
                            foreignKey.Properties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            duplicateForeignKey.Properties.Format(),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            Format(foreignKey.DeclaringEntityType.Relational()),
                            foreignKey.Relational().ConstraintName,
                            foreignKey.DeleteBehavior,
                            duplicateForeignKey.DeleteBehavior));
                }

                return false;
            }

            return true;
        }

        private static string Format(IRelationalEntityTypeAnnotations annotations)
            => (string.IsNullOrEmpty(annotations.Schema) ? "" : annotations.Schema + ".") + annotations.TableName;
    }
}
