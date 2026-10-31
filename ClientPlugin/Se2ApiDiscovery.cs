using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Keen.VRage.Library.Diagnostics;

namespace RSSG.ClientPlugin;

internal static class Se2ApiDiscovery
{
    private static readonly string[] CandidateAssemblies = { "SpaceEngineers2", "Game2.Game", "Game2.Simulation", "Game2.Client" };
    private static readonly string[] CandidateTypeNames = { "GameAppComponent", "WorldSessionComponent", "WorldSeedProviderSessionComponent", "VoxelPlanetsSessionComponent", "MyGuiScreenBase", "MyGuiSandbox", "MyGuiControlSlider", "MyGuiControlCheckbox", "MyGuiControlButton" };
    private const BindingFlags DeclaredMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    public static void DumpCandidates()
    {
        Log.Default.WriteLine("[RSSG] Starting read-only SE2 API discovery.");
        var assemblies = LoadCandidateAssemblies();
        foreach (var typeName in CandidateTypeNames)
        {
            var matches = new List<Type>();
            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in GetLoadableTypes(assembly))
                        if (string.Equals(type.Name, typeName, StringComparison.Ordinal)) matches.Add(type);
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine($"[RSSG] Type scan failed in {assembly.GetName().Name}: {ex.GetType().Name}: {ex.Message}");
                }
            }
            if (matches.Count == 0)
            {
                Log.Default.WriteLine($"[RSSG] NOT FOUND: {typeName}");
                continue;
            }
            foreach (var type in matches.OrderBy(t => t.FullName, StringComparer.Ordinal)) DumpType(type);
        }
        Log.Default.WriteLine("[RSSG] SE2 API discovery finished. No game methods were patched.");
    }

    private static List<Assembly> LoadCandidateAssemblies()
    {
        var result = new List<Assembly>();
        foreach (var name in CandidateAssemblies)
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase));
            if (assembly == null)
            {
                try { assembly = Assembly.Load(new AssemblyName(name)); }
                catch (Exception ex)
                {
                    Log.Default.WriteLine($"[RSSG] Assembly unavailable: {name} ({ex.GetType().Name}: {ex.Message})");
                }
            }
            if (assembly != null && !result.Contains(assembly))
            {
                result.Add(assembly);
                Log.Default.WriteLine($"[RSSG] Assembly loaded: {assembly.FullName}");
            }
        }
        return result;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
    }

    private static void DumpType(Type type)
    {
        Log.Default.WriteLine($"[RSSG] TYPE: {type.FullName}");
        Log.Default.WriteLine($"[RSSG]   Assembly: {type.Assembly.GetName().Name}");
        Log.Default.WriteLine($"[RSSG]   Visibility: {(type.IsPublic || type.IsNestedPublic ? "public" : "non-public")}; abstract={type.IsAbstract}; sealed={type.IsSealed}");
        try
        {
            foreach (var ctor in type.GetConstructors(DeclaredMembers).OrderBy(FormatMethod, StringComparer.Ordinal))
                Log.Default.WriteLine($"[RSSG]   CTOR: {FormatMethod(ctor)}");
        }
        catch (Exception ex) { Log.Default.WriteLine($"[RSSG]   Constructor scan failed: {ex.GetType().Name}: {ex.Message}"); }
        try
        {
            foreach (var method in type.GetMethods(DeclaredMembers).OrderBy(FormatMethod, StringComparer.Ordinal))
                Log.Default.WriteLine($"[RSSG]   METHOD: {FormatMethod(method)}");
        }
        catch (Exception ex) { Log.Default.WriteLine($"[RSSG]   Method scan failed: {ex.GetType().Name}: {ex.Message}"); }
        try
        {
            foreach (var field in type.GetFields(DeclaredMembers).OrderBy(f => f.Name, StringComparer.Ordinal))
                Log.Default.WriteLine($"[RSSG]   FIELD: {Visibility(field)} {(field.IsStatic ? "static " : "")}{FormatType(field.FieldType)} {field.Name}");
        }
        catch (Exception ex) { Log.Default.WriteLine($"[RSSG]   Field scan failed: {ex.GetType().Name}: {ex.Message}"); }
        try
        {
            foreach (var property in type.GetProperties(DeclaredMembers).OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                var accessors = property.GetAccessors(true);
                var access = accessors.Length == 0 ? "unknown" : string.Join("/", accessors.Select(Visibility));
                Log.Default.WriteLine($"[RSSG]   PROPERTY: {access} {FormatType(property.PropertyType)} {property.Name}");
            }
        }
        catch (Exception ex) { Log.Default.WriteLine($"[RSSG]   Property scan failed: {ex.GetType().Name}: {ex.Message}"); }
    }

    private static string FormatMethod(MethodBase method)
    {
        var parameters = string.Join(", ", method.GetParameters().Select(p =>
            $"{(p.IsOut ? "out " : p.ParameterType.IsByRef ? "ref " : "")}{FormatType(p.ParameterType)} {p.Name}"));
        var returnType = method is MethodInfo info ? FormatType(info.ReturnType) + " " : "";
        return $"{Visibility(method)} {(method.IsStatic ? "static " : "")}{returnType}{method.Name}({parameters})";
    }

    private static string Visibility(MethodBase method)
    {
        if (method.IsPublic) return "public";
        if (method.IsFamily) return "protected";
        if (method.IsAssembly) return "internal";
        if (method.IsPrivate) return "private";
        return "non-public";
    }

    private static string Visibility(FieldInfo field)
    {
        if (field.IsPublic) return "public";
        if (field.IsFamily) return "protected";
        if (field.IsAssembly) return "internal";
        if (field.IsPrivate) return "private";
        return "non-public";
    }

    private static string Visibility(MethodInfo method) => Visibility((MethodBase)method);

    private static string FormatType(Type type)
    {
        if (type.IsByRef) return FormatType(type.GetElementType()) + "&";
        if (type.IsPointer) return FormatType(type.GetElementType()) + "*";
        if (type.IsArray) return FormatType(type.GetElementType()) + "[]";
        if (!type.IsGenericType) return type.FullName ?? type.Name;
        var definition = type.GetGenericTypeDefinition().FullName ?? type.Name;
        var tick = definition.IndexOf((char)96);
        if (tick >= 0) definition = definition.Substring(0, tick);
        return definition + "<" + string.Join(", ", type.GetGenericArguments().Select(FormatType)) + ">";
    }
}
