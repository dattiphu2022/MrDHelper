using System;
using System.Collections.Generic;
using System.Linq;

namespace MrDHelper.Models
{
    /// <summary>
    /// Đại diện cho một lỗi đơn lẻ, gồm mã lỗi và mô tả hiển thị.
    /// </summary>
    public sealed class Error
    {
        public string Code { get; set; }
        public string Description { get; set; }

        public static readonly Error None = new(string.Empty, string.Empty);

        public Error(string code, string description)
        {
            Code = code;
            Description = description;
        }

        public override string ToString()
            => $"Mã lỗi: {Code}.{Environment.NewLine}Mô tả lỗi: {Description}";
    }

    /// <summary>
    /// Kết quả không generic, có thể chứa một hoặc nhiều lỗi.
    /// Thành công khi không có lỗi nào.
    /// Message là thông điệp dùng nhanh cho cả success/failure (có thể null).
    /// </summary>
    public class Result
    {
        private static string? NormalizeMessage(string? message)
            => string.IsNullOrWhiteSpace(message) ? null : message.Trim();

        private static string NormalizeDescription(string? description)
            => NormalizeMessage(description) ?? string.Empty;

        public bool IsSuccess => Errors.Count == 0;
        public bool IsFailure => !IsSuccess;

        public IReadOnlyCollection<Error> Errors { get; }
        public Error FirstError { get; }
        public string? Message { get; }

        protected Result(IEnumerable<Error> errors, string? message = null)
        {
            if (errors is null) throw new ArgumentNullException(nameof(errors));

            var list = errors.ToList().AsReadOnly();
            Errors = list;
            FirstError = list.FirstOrDefault() ?? Error.None;

            // Message là "dùng nhanh": có thì có, không thì null. Không nhét sentinel.
            Message = NormalizeMessage(message);
        }

        public static Result Ok(string? message = null)
            => new Result(Array.Empty<Error>(), NormalizeMessage(message));

        /// <summary>
        /// Failure chỉ có message: Message sẽ là message (hoặc null nếu rỗng).
        /// FirstError.Description sẽ là message (hoặc chuỗi rỗng nếu null).
        /// </summary>
        public static Result Failure(string? message = null)
        {
            var m = NormalizeMessage(message);
            return new Result([new Error("UNDEFINED-CODE", NormalizeDescription(m))], m);
        }

        /// <summary>
        /// Failure từ 1 Error: Message đồng bộ theo Error.Description (trim/null nếu rỗng).
        /// </summary>
        public static Result Failure(Error error)
        {
            if (error is null) throw new ArgumentNullException(nameof(error));

            var desc = NormalizeMessage(error.Description);
            return new Result([new Error(error.Code, NormalizeDescription(desc))], desc);
        }

        /// <summary>
        /// Failure từ code + description: Message đồng bộ theo description (trim/null nếu rỗng).
        /// </summary>
        public static Result Failure(string code, string description)
        {
            var desc = NormalizeMessage(description);
            return new Result([new Error(code, NormalizeDescription(desc))], desc);
        }

        /// <summary>
        /// Failure từ nhiều lỗi: Message ưu tiên FirstError.Description (trim/null nếu rỗng).
        /// </summary>
        public static Result Failure(IEnumerable<Error> errors)
        {
            if (errors is null) throw new ArgumentNullException(nameof(errors));

            var list = errors.ToList();
            var first = list.FirstOrDefault();
            if (first is null)
                return new Result(Array.Empty<Error>(), message: null);

            var firstDesc = NormalizeMessage(first.Description);
            list[0] = new Error(first.Code, NormalizeDescription(firstDesc));

            return new Result(list, firstDesc);
        }

        /// <summary>
        /// Failure từ nhiều lỗi, kèm message "dùng nhanh" do caller quyết định.
        /// Nếu message null/rỗng thì Message sẽ null (không tự bịa).
        /// </summary>
        public static Result Failure(IEnumerable<Error> errors, string? message)
        {
            if (errors is null) throw new ArgumentNullException(nameof(errors));

            var m = NormalizeMessage(message);
            return new Result(errors, m);
        }

        public override string ToString()
            => IsSuccess
                ? (Message is null ? "Thành công" : $"Thành công: {Message}")
                : (Message is null ? $"Thất bại: {FirstError}" : $"Thất bại: {Message}{Environment.NewLine}{FirstError}");

        public void ThrowIfFailure()
        {
            if (IsFailure)
                throw new InvalidOperationException(FirstError.ToString());
        }
    }

    /// <summary>
    /// Kết quả generic chứa giá trị kiểu T khi thành công,
    /// hoặc một hay nhiều lỗi khi thất bại.
    /// </summary>
    public sealed class Result<T> : Result
    {
        public T Value { get; }

        private Result(T value, IEnumerable<Error> errors, string? message = null)
            : base(errors, message)
        {
            // Giữ đúng invariant:
            // - Success => Value không null (đối với reference type)
            // - Failure => Value phải default
            if (IsSuccess && value is null)
                throw new ArgumentNullException(nameof(value), "Kết quả thành công phải có giá trị.");
            if (IsFailure && value is not null)
                throw new ArgumentException("Kết quả thất bại không được có giá trị.", nameof(value));

            Value = value!;
        }

        public static Result<T> Ok(T value, string? message = null)
            => new Result<T>(value, Array.Empty<Error>(), message);

        public static new Result<T> Failure(string? message = null)
        {
            var m = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
            return new Result<T>(default!, [new Error("UNDEFINED-CODE", m ?? string.Empty)], m);
        }

        public static new Result<T> Failure(Error error)
        {
            if (error is null) throw new ArgumentNullException(nameof(error));

            var desc = string.IsNullOrWhiteSpace(error.Description) ? null : error.Description.Trim();
            return new Result<T>(default!, [new Error(error.Code, desc ?? string.Empty)], desc);
        }

        public static new Result<T> Failure(string code, string description)
        {
            var desc = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            return new Result<T>(default!, [new Error(code, desc ?? string.Empty)], desc);
        }

        public static new Result<T> Failure(IEnumerable<Error> errors)
        {
            if (errors is null) throw new ArgumentNullException(nameof(errors));

            var list = errors.ToList();
            var first = list.FirstOrDefault();

            if (first is null)
                return new Result<T>(default!, Array.Empty<Error>(), null);

            var firstDesc = string.IsNullOrWhiteSpace(first.Description) ? null : first.Description.Trim();
            list[0] = new Error(first.Code, firstDesc ?? string.Empty);

            return new Result<T>(default!, list, firstDesc);
        }

        public static Result<T> Failure(IEnumerable<Error> errors, string? message)
        {
            if (errors is null) throw new ArgumentNullException(nameof(errors));

            var m = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
            return new Result<T>(default!, errors, m);
        }

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
            => IsSuccess ? onSuccess(Value) : onFailure(FirstError);

        public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
            => IsSuccess ? Result<TResult>.Ok(mapper(Value), Message) : Result<TResult>.Failure(Errors, Message);

        public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
            => IsSuccess ? binder(Value) : Result<TResult>.Failure(Errors, Message);
    }
}