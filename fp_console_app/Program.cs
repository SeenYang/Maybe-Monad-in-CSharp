// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using fp_console_app;

var ctx1 = new CustomiseContext { KeyValuePairs = new Dictionary<string, string> { { "otherKeys", "fake-org-id" } } };

#region Test 1 use Safe navigation operator

try
{
    var orgId1 = TryGetOrganisationIdFromContext(ctx1);
    Console.WriteLine($"Test1: Return type is {orgId1?.GetType()} ");
    Console.WriteLine(
        string.IsNullOrWhiteSpace(orgId1)
            ? "Test1: ==> Org ID is empty or white spaces."
            : $"Test1: ==> Organisation Id: {orgId1}."
    );
    var msg = $"Test1: ==> Here we can use the `string?` in this way: {orgId1 ?? "Here's no OrgId...."}";
    // Or in this way:
    orgId1 ??= "Just assign value if it's null.".Insert(0, "at index 0 --> ");
    Console.WriteLine($"Test1: ==> Now the orgId1 is: {orgId1}");
    Console.WriteLine();
}
catch (Exception e)
{
    Console.WriteLine("***** Something goes wrong:\n" + e + "\n*****\n");
}

string? TryGetOrganisationIdFromContext(CustomiseContext context)
{
    context.KeyValuePairs.TryGetValue("OrgId", out var orgId);
    return string.IsNullOrWhiteSpace(orgId) ? null : orgId;
}

#endregion

#region Test 2 use Maybe<T> monad as a class.

// In the previous section, because we defined Maybe as a class, null is a valid value.
// That is, Maybe<string> can actually be one of three things:
// 1. null,
// 2. Maybe<string>.None and
// 3. Maybe<string>.Some.
var content2 = TryGetOrgIdFromContextMaybe(ctx1);
// Event though your IDE will warn you that `content2` won't be null,
// but below code will break cuz the method returns null.
try
{
    content2.Match(
        some: s => { Console.WriteLine($"Test2: ==> OrgId is: {s}"); },
        none: () => { Console.WriteLine("Test2: ==> Org ID is empty or white spaces."); }
    );
}
catch (Exception e)
{
    Console.WriteLine("Test2: ==> TryGetOrgIdFromContextMaybe() return null.");
}

Maybe<string> TryGetOrgIdFromContextMaybe(CustomiseContext context)
{
    return context.KeyValuePairs.TryGetValue("OrgId", out var orgId) ? new Maybe<string>.Some(orgId) : null;
}

#endregion

#region Test3 use Maybe<T> as Strcut

// Structs in C# cannot have the value null. For example, this code is invalid:
// int a = null;
var orgIdFromContextMaybeAsStruct = TryGetOrgIdFromContextMaybeAsStruct(ctx1);
Console.WriteLine(orgIdFromContextMaybeAsStruct.TryGetValue(out var value)
    ? $"Test3: ==> normal out value that everyone can access: {value}."
    : "Test3: ==> Can't get Org Id.");

orgIdFromContextMaybeAsStruct.Match(
    some: s => Console.WriteLine($"Test3: ==> normal out value that everyone can access: {s}."),
    none: () => Console.WriteLine("Test3: ==> Can't get Org Id."));

MaybeAsStruct<string> TryGetOrgIdFromContextMaybeAsStruct(CustomiseContext context)
{
    return context.KeyValuePairs.TryGetValue("OrgId", out var orgId) ? orgId : MaybeAsStruct.None;
}

#endregion

internal class CustomiseContext
{
    public CustomiseContext()
    {
        KeyValuePairs = new Dictionary<string, string>();
    }

    public CustomiseContext(Dictionary<string, string> keyValuePairs)
    {
        KeyValuePairs = keyValuePairs;
    }

    public Dictionary<string, string> KeyValuePairs { get; set; }
}