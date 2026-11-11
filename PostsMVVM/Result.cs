namespace PostsMVVM;

public record Success<T>(T Input);

public record Error(System.Exception InnerException);

public union Result<T>(Success<T>, Error);
