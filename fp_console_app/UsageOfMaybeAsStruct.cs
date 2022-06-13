namespace fp_console_app;

public class UsageOfMaybeAsStruct
{
    static void Test2()
    {
        var contents = GetLogContents(1);
        Console.WriteLine(contents.TryGetValue(out var value) ? value : "Log file not found");
    }

    static void Test3()
    {
        var contents = GetLogContents(1);

        contents.Match(some: value => { Console.WriteLine(value); },
            none: () => { Console.WriteLine("Log file not found"); });
    }

    static void Test4()
    {
        var errorDescriptionMaybe =
            GetLogContents(13)
                .Bind(contents => FindErrorCode(contents))
                .Bind(errorCode => GetErrorDescription(errorCode));
    }

    static void Test5()
    {
        var errorDescriptionMaybe =
            GetLogContents(13)
                .Bind(contents => FindErrorCode(contents)
                    .Bind(errorCode => GetErrorDescription(errorCode, contents)));
    }

    static void Test6()
    {
        var errorDescriptionMaybe =
            from contents in GetLogContents(13)
            from errorCode in FindErrorCode(contents)
            from errorDescription in GetErrorDescription(errorCode, contents)
            select errorDescription;
    }

    static void Test7()
    {
        var errorDescriptionMaybe =
            from contents in GetLogContents(13)
            from errorCode in FindErrorCode(contents)
            where errorCode < 1000
            from errorDescription in GetErrorDescription(errorCode, contents)
            select errorDescription;
    }

    static void Test8()
    {
        var errorMessage =
            GetErrorDescription(15)
                .ValueOr("Unknown error");
    }

    static void Test9()
    {
        var errorMessage =
            GetErrorDescription(15)
                .ValueOr(() => GetDefaultErrorMessage());
    }

    static void Test10()
    {
        var errorMessage =
            GetErrorDescription(15)
                .ValueOrMaybe(GetErrorDescriptionViaWebService(15))
                .ValueOr("Unknown error");
    }

    static void Test11()
    {
        var errorMessage =
            GetErrorDescription(15)
                .ValueOrMaybe(() => GetErrorDescriptionViaWebService(15))
                .ValueOr("Unknown error");
    }

    static void Test12()
    {
        var logContents =
            GetLogContents(1)
                .ValueOrThrow("Unable to get log contents");
    }

    static void Test14()
    {
        List<string> multipleLogContents =
            Enumerable.Range(1, 20)
                .Select(x => GetLogContents(x))
                .GetItemsWithValue()
                .ToList();
    }

    static void Test15()
    {
        List<string> multipleLogContents =
            Enumerable.Range(1, 20)
                .Select(x => GetLogContents(x))
                .IfAllHaveValues()
                .ValueOrThrow("Some logs are not available")
                .ToList();
    }

    static void Test16()
    {
        MaybeAsStruct<string> logMaybe = MaybeAsStruct.Some("entry9");

        var list = new List<string>()
        {
            "entry1",
            logMaybe.ToAddIfHasValue(),
            "entry2"
        };
    }

    static string GetDefaultErrorMessage()
    {
        return File.ReadAllText("c:\\defaultErrorMessage.txt");
    }

    static MaybeAsStruct<string> GetLogContents(int id)
    {
        var filename = "c:\\logs\\" + id + ".log";

        if (File.Exists(filename))
            return File.ReadAllText(filename);

        return MaybeAsStruct.None;
    }

    static MaybeAsStruct<int> FindErrorCode(string logContents)
    {
        var logLines = logContents.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        return
            logLines
                .FirstOrNone(x => x.StartsWith("Error code: "))
                .Map(x => x.Substring("Error code: ".Length))
                .Bind(x => x.TryParseToInt());
    }

    static MaybeAsStruct<string> GetErrorDescription(int errorCode)
    {
        var filename = "c:\\errorCodes\\" + errorCode + ".txt";

        if (File.Exists(filename))
            return File.ReadAllText(filename);

        return MaybeAsStruct.None;
    }

    static MaybeAsStruct<string> GetErrorDescription(int errorCode, string logContents)
    {
        var logLines = logContents.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        var linePrefix = "Error description for code " + errorCode + ": ";

        return
            logLines
                .FirstOrNone(x => x.StartsWith(linePrefix))
                .Map(x => x.Substring(linePrefix.Length));
    }

    static MaybeAsStruct<string> GetErrorDescriptionViaWebService(int errorCode)
    {
        //Real code would call a web service
        return MaybeAsStruct.None;
    }
}

public static class ExtensionMethods
{
    public static MaybeAsStruct<T> FirstOrNone<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
    {
        return enumerable.Where(predicate).FirstOrNone();
    }

    public static MaybeAsStruct<T> FirstOrNone<T>(this IEnumerable<T> enumerable)
    {
        switch (enumerable)
        {
            case null:
                throw new ArgumentNullException(nameof(enumerable));
            case IList<T> list:
            {
                if (list.Count > 0) return list[0];
                break;
            }
            default:
            {
                using var e = enumerable.GetEnumerator();
                if (e.MoveNext()) return e.Current;

                break;
            }
        }

        return new MaybeAsStruct<T>();
    }

    public static MaybeAsStruct<int> TryParseToInt(this string str)
    {
        if (int.TryParse(str, out var result))
            return result;

        return MaybeAsStruct.None;
    }

    public static T[] ValueOrEmptyArray<T>(this MaybeAsStruct<T[]> maybe)
    {
        return maybe.ValueOr(Array.Empty<T>());
    }

    public static string ValueOrEmptyString(this MaybeAsStruct<string> maybe)
    {
        return maybe.ValueOr(string.Empty);
    }

    public static IEnumerable<T> GetItemsWithValue<T>(this IEnumerable<MaybeAsStruct<T>> enumerable)
    {
        foreach (var maybe in enumerable)
        {
            if (maybe.TryGetValue(out var value))
                yield return value;
        }
    }

    public static MaybeAsStruct<IEnumerable<T>> IfAllHaveValues<T>(this IEnumerable<MaybeAsStruct<T>> enumerable)
    {
        var result = new List<T>();

        foreach (var maybe in enumerable)
        {
            if (!maybe.TryGetValue(out var value))
                return MaybeAsStruct.None;

            result.Add(value);
        }

        return result;
    }

    public static AddIfHasValue<T> ToAddIfHasValue<T>(this MaybeAsStruct<T> maybe)
    {
        return new AddIfHasValue<T>(maybe);
    }

    public static void Add<T>(this ICollection<T> collection, AddIfHasValue<T> addIfHasValue)
    {
        if (addIfHasValue.Maybe.TryGetValue(out var value))
        {
            collection.Add(value);
        }
    }
}

public struct AddIfHasValue<T>
{
    public MaybeAsStruct<T> Maybe { get; }

    public AddIfHasValue(MaybeAsStruct<T> maybe)
    {
        Maybe = maybe;
    }
}