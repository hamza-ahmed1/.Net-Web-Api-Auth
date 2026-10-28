namespace Auth.Model.DTOs.Announcements
{
    public enum ResultErrorKind { None, Validation, Forbidden, NotFound }

    public class Result<T>
    {
        public bool Success { get; set; }
        public T? Value { get; set; }
        public string? Error { get; set; }
        public ResultErrorKind ErrorKind { get; set; }

        public static Result<T> Ok(T value) => new() { Success = true, Value = value };
        public static Result<T> Failure(string error) => new() { Success = false, Error = error, ErrorKind = ResultErrorKind.Validation };
        public static Result<T> Forbidden(string error) => new() { Success = false, Error = error, ErrorKind = ResultErrorKind.Forbidden };
        public static Result<T> NotFound(string error) => new() { Success = false, Error = error, ErrorKind = ResultErrorKind.NotFound };
    }
}
