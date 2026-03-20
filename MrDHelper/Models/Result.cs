using System;
using System.Collections.Generic;
using System.Linq;

namespace MrDHelper.Models
{
    /// <summary>
    /// Represents a single error with a code and display description.
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
            => $"Error code: {Code}.{Environment.NewLine}Error description: {Description}";
    }

    /// <summary>
    /// Represents a non-generic result that can contain one or more errors.
    /// It is successful when there are no errors.
    /// `Message` is a lightweight message for both success and failure and can be null.
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

            // Message is a lightweight optional value. Keep it null instead of inventing a sentinel.
            Message = NormalizeMessage(message);
        }

        public static Result Ok(string? message = null)
            => new Result(Array.Empty<Error>(), NormalizeMessage(message));

        /// <summary>
        /// Failure with only a message. `Message` keeps that value or null if it is empty.
        /// `FirstError.Description` uses the same message or an empty string if null.
        /// </summary>
        public static Result Failure(string? message = null)
        {
            var m = NormalizeMessage(message);
            return new Result([new Error("UNDEFINED-CODE", NormalizeDescription(m))], m);
        }

        /// <summary>
        /// Failure from a single `Error`, keeping `Message` aligned with `Error.Description`.
        /// </summary>
        public static Result Failure(Error error)
        {
            if (error is null) throw new ArgumentNullException(nameof(error));

            var desc = NormalizeMessage(error.Description);
            return new Result([new Error(error.Code, NormalizeDescription(desc))], desc);
        }

        /// <summary>
        /// Failure from a code and description, keeping `Message` aligned with the description.
        /// </summary>
        public static Result Failure(string code, string description)
        {
            var desc = NormalizeMessage(description);
            return new Result([new Error(code, NormalizeDescription(desc))], desc);
        }

        /// <summary>
        /// Failure from multiple errors, prioritizing `FirstError.Description` as the message.
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
        /// Failure from multiple errors with an optional lightweight message provided by the caller.
        /// If the message is null or empty, `Message` remains null.
        /// </summary>
        public static Result Failure(IEnumerable<Error> errors, string? message)
        {
            if (errors is null) throw new ArgumentNullException(nameof(errors));

            var m = NormalizeMessage(message);
            return new Result(errors, m);
        }

        public override string ToString()
            => IsSuccess
                ? (Message is null ? "Success" : $"Success: {Message}")
                : (Message is null ? $"Failure: {FirstError}" : $"Failure: {Message}{Environment.NewLine}{FirstError}");

        public void ThrowIfFailure()
        {
            if (IsFailure)
                throw new InvalidOperationException(FirstError.ToString());
        }
    }

    /// <summary>
    /// Represents a generic result that contains a value of type `T` on success,
    /// or one or more errors on failure.
    /// </summary>
    public sealed class Result<T> : Result
    {
        public T Value { get; }

        private Result(T value, IEnumerable<Error> errors, string? message = null)
            : base(errors, message)
        {
            // Preserve invariants:
            // - Success => Value is not null for reference types
            // - Failure => Value must be default
            if (IsSuccess && value is null)
                throw new ArgumentNullException(nameof(value), "A successful result must contain a value.");
            if (IsFailure && !EqualityComparer<T>.Default.Equals(value, default!))
                throw new ArgumentException("A failed result cannot contain a value.", nameof(value));

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

        public static new Result<T> Failure(IEnumerable<Error> errors, string? message)
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
