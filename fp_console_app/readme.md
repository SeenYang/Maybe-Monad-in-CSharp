# The Maybe Monad (C#)

This project is try to demo how we can improve the way we handle `null` value more elegantly.
In side the `Program.cs`, there's three main approach about handling a result that return by a method, which could be string or nothing.

`Maybe<T>` is a type of monad that presents 2 status:
1. A string
2. Nothing

`UsageOfMaybeAsStruct` also contains demo about extension of `IEnumerable<T>`.