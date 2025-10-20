using System.Reflection;
using Microsoft.Data.SqlClient;

namespace ShipManagement.Tests;

internal static class SqlExceptionFactory
{
    public static SqlException Create(string message)
    {
        var errorCollection = (SqlErrorCollection)Activator.CreateInstance(typeof(SqlErrorCollection), nonPublic: true)!;

        var errorConstructor = typeof(SqlError)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(c => c.GetParameters().Length == 8);

        var error = (SqlError)errorConstructor.Invoke(new object?[]
        {
            0, (byte)0, (byte)0, "server", message, "procedure", 0, null
        });

        typeof(SqlErrorCollection)
            .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(errorCollection, new object[] { error });

        var createException = typeof(SqlException).GetMethod(
            "CreateException",
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            types: new[] { typeof(SqlErrorCollection), typeof(string) },
            modifiers: null)!;

        return (SqlException)createException.Invoke(null, new object?[] { errorCollection, "11.0.0" })!;
    }
}
