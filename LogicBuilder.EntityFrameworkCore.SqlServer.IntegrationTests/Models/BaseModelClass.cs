using LogicBuilder.Domain;
using System;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models
{
    abstract public class BaseModelClass : BaseModel
    {
        public string TypeFullName => this.GetType().ToTypeString();
    }

    public static class TypeHelpers
    {
        public static string ToTypeString(this Type type)
            => type.IsGenericType && !type.IsGenericTypeDefinition
                ? type.AssemblyQualifiedName
                : type.FullName;
    }
}
