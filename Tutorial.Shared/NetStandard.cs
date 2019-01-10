namespace Tutorial
{
    using System;
    using System.Reflection;

    internal static class NetStandard
    {
        internal static Assembly Assembly { get; } = Assembly.LoadFrom(
            @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.1.6\netstandard.dll");
            // Linux: /usr/share/dotnet/shared/Microsoft.NETCore.App/2.1.6/netstandard.dll
            // macOS: /usr/local/share/dotnet/shared/Microsoft.NETCore.App/2.1.6/netstandard.dll

        // internal static Type[] Types => Assembly.GetForwardedTypes();
    }
}
