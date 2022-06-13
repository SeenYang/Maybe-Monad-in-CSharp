namespace fp_console_app;

public struct MaybeAsStruct<T>
{
    private readonly T _value;

    private readonly bool _hasValue;

    private MaybeAsStruct(T value)
    {
        this._value = value;
        _hasValue = true;
    }

    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
    {
        return _hasValue ? some(_value) : none();
    }

    public void Match(Action<T> some, Action none)
    {
        if (_hasValue)
        {
            some(_value);
        }
        else
        {
            none();
        }
    }

    public static implicit operator MaybeAsStruct<T>(T value)
    {
        return value == null ? new MaybeAsStruct<T>() : new MaybeAsStruct<T>(value);
    }

    public static implicit operator MaybeAsStruct<T>(MaybeAsStruct.MaybeNone value)
    {
        return new MaybeAsStruct<T>();
    }

    public bool TryGetValue(out T value)
    {
        if (_hasValue)
        {
            value = this._value;
            return true;
        }

        value = default(T);
        return false;
    }

    public MaybeAsStruct<TResult> Map<TResult>(Func<T, TResult> convert)
    {
        return !_hasValue ? new MaybeAsStruct<TResult>() : convert(_value);
    }

    public MaybeAsStruct<TResult> Select<TResult>(Func<T, TResult> convert)
    {
        return !_hasValue ? new MaybeAsStruct<TResult>() : convert(_value);
    }

    public MaybeAsStruct<TResult> Bind<TResult>(Func<T, MaybeAsStruct<TResult>> convert)
    {
        return !_hasValue ? new MaybeAsStruct<TResult>() : convert(_value);
    }

    public MaybeAsStruct<TResult> SelectMany<T2, TResult>(
        Func<T, MaybeAsStruct<T2>> convert,
        Func<T, T2, TResult> finalSelect)
    {
        if (!_hasValue)
            return new MaybeAsStruct<TResult>();

        var converted = convert(_value);

        return !converted._hasValue ? new MaybeAsStruct<TResult>() : finalSelect(_value, converted._value);
    }

    public MaybeAsStruct<T> Where(Func<T, bool> predicate)
    {
        if (!_hasValue)
            return new MaybeAsStruct<T>();

        return predicate(_value) ? this : new MaybeAsStruct<T>();
    }

    public T ValueOr(T defaultValue)
    {
        return _hasValue ? _value : defaultValue;
    }

    public T ValueOr(Func<T> defaultValueFactory)
    {
        return _hasValue ? _value : defaultValueFactory();
    }

    public MaybeAsStruct<T> ValueOrMaybe(MaybeAsStruct<T> alternativeValue)
    {
        return _hasValue ? this : alternativeValue;
    }

    public MaybeAsStruct<T> ValueOrMaybe(Func<MaybeAsStruct<T>> alternativeValueFactory)
    {
        return _hasValue ? this : alternativeValueFactory();
    }

    public T ValueOrThrow(string errorMessage)
    {
        if (_hasValue)
            return _value;

        throw new Exception(errorMessage);
    }
}

public static class MaybeAsStruct
{
    public class MaybeNone
    {
    }

    public static MaybeNone None { get; } = new MaybeNone();

    public static MaybeAsStruct<T> Some<T>(T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        return value;
    }
}