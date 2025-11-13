using System;
using System.Collections.Generic;
using System.Linq;

namespace MrDHelper.Models
{
    /// <summary>
    /// Đại diện cho một lỗi đơn lẻ, gồm mã lỗi và mô tả hiển thị cho người dùng.
    /// </summary>
    public sealed class Error
    {
        public string Code { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Định dạng lỗi để hiển thị hoặc ghi log.
        /// </summary>
        public override string ToString()
            => $"Mã lỗi: {Code}.{Environment.NewLine}Mô tả lỗi: {Description}";

        /// <summary>
        /// Đối tượng “không có lỗi” dùng chung, nhằm tránh phải kiểm tra null.
        /// </summary>
        public static readonly Error None = new Error(string.Empty, string.Empty);

        public Error(string code, string description)
        {
            Code = code;
            Description = description;
        }
    }

    /// <summary>
    /// Kết quả không generic, có thể chứa một hoặc nhiều lỗi.
    /// Thành công khi không có lỗi nào.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Trả về true nếu không có lỗi.
        /// </summary>
        public bool IsSuccess => Errors.Count == 0;

        /// <summary>
        /// Trả về true nếu có ít nhất một lỗi.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Tập hợp toàn bộ lỗi (có thể rỗng).
        /// </summary>
        public IReadOnlyCollection<Error> Errors { get; }

        /// <summary>
        /// Lỗi đầu tiên trong danh sách, hoặc Error.None nếu không có lỗi nào.
        /// Được lưu sẵn khi khởi tạo để truy cập nhanh.
        /// </summary>
        public Error FirstError { get; }

        /// <summary>
        /// Thông điệp tuỳ chọn cho trường hợp thành công (mặc định null nếu thất bại).
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// Hàm tạo bảo vệ (protected) nhận danh sách lỗi và thông điệp tuỳ chọn.
        /// </summary>
        protected Result(IEnumerable<Error> errors, string? message = null)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            var list = errors.ToList().AsReadOnly();
            Errors = list;

            FirstError = list.FirstOrDefault() ?? Error.None;
            Message = message;
        }

        /// <summary>
        /// Tạo kết quả thành công (không có lỗi).
        /// </summary>
        public static Result Ok(string? message = null)
            => new Result(Array.Empty<Error>(), message);

        /// <summary>
        /// Tạo kết quả thất bại từ một lỗi duy nhất.
        /// </summary>
        public static Result Failure(Error error)
            => new Result(new[] { error });

        /// <summary>
        /// Tạo kết quả thất bại từ mã lỗi và mô tả.
        /// </summary>
        public static Result Failure(string code, string description)
            => new Result(new[] { new Error(code, description) });

        /// <summary>
        /// Tạo kết quả thất bại từ nhiều lỗi.
        /// </summary>
        public static Result Failure(IEnumerable<Error> errors)
            => new Result(errors);

        /// <summary>
        /// Dùng để debug hoặc ghi log: hiển thị “Success” hoặc lỗi đầu tiên.
        /// </summary>
        public override string ToString()
            => IsSuccess
               ? $"Thành công{(string.IsNullOrEmpty(Message) ? "" : $": {Message}")}"
               : "Thất bại: " + FirstError.ToString();

        /// <summary>
        /// Ném ra InvalidOperationException nếu kết quả là thất bại.
        /// Sử dụng FirstError làm thông báo ngoại lệ.
        /// </summary>
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
    /// <typeparam name="T">Kiểu dữ liệu của giá trị thành công.</typeparam>
    public sealed class Result<T> : Result
    {
        /// <summary>
        /// Giá trị được bao bọc khi thành công; null nếu thất bại.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Hàm tạo riêng, khởi tạo giá trị và danh sách lỗi.
        /// Đảm bảo tính nhất quán: thành công thì có Value khác null,
        /// thất bại thì Value phải null.
        /// </summary>
        private Result(T value, IEnumerable<Error> errors, string? message = null)
            : base(errors, message)
        {
            if (IsSuccess && value == null)
                throw new ArgumentNullException(nameof(value), "Kết quả thành công phải có giá trị.");
            if (IsFailure && value != null)
                throw new ArgumentException("Kết quả thất bại không được có giá trị.", nameof(value));

            Value = value!;
        }

        /// <summary>
        /// Tạo kết quả thành công với giá trị được chỉ định.
        /// </summary>
        public static Result<T> Ok(T value, string? message = null)
            => new Result<T>(value, Array.Empty<Error>(), message);

        /// <summary>
        /// Tạo kết quả thất bại với một lỗi duy nhất.
        /// Tham số kiểu generic T được gán mặc định.
        /// </summary>
        public static new Result<T> Failure(Error error)
            => new Result<T>(default!, new[] { error });

        /// <summary>
        /// Tạo kết quả thất bại từ mã lỗi và mô tả.
        /// </summary>
        public static new Result<T> Failure(string code, string description)
            => new Result<T>(default!, new[] { new Error(code, description) });

        /// <summary>
        /// Tạo kết quả thất bại với nhiều lỗi.
        /// </summary>
        public static new Result<T> Failure(IEnumerable<Error> errors)
            => new Result<T>(default!, errors);

        /// <summary>
        /// Áp dụng một trong hai hàm tùy theo trạng thái thành công hay thất bại,
        /// và trả về giá trị kiểu TResult.
        /// </summary>
        /// <typeparam name="TResult">Kiểu dữ liệu trả về.</typeparam>
        /// <param name="onSuccess">Hàm gọi khi thành công (truyền vào Value).</param>
        /// <param name="onFailure">Hàm gọi khi thất bại (truyền vào FirstError).</param>
        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
            => IsSuccess
               ? onSuccess(Value)
               : onFailure(FirstError);

        /// <summary>
        /// Chuyển đổi giá trị thành công T sang TResult,
        /// đồng thời giữ nguyên lỗi nếu thất bại.
        /// </summary>
        /// <typeparam name="TResult">Kiểu đích cần ánh xạ tới.</typeparam>
        /// <param name="mapper">Hàm ánh xạ (Func) từ T sang TResult.</param>
        public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
            => IsSuccess
               ? Result<TResult>.Ok(mapper(Value), Message)
               : Result<TResult>.Failure(FirstError);

        /// <summary>
        /// Gọi chuỗi một hàm khác trả về Result, hợp nhất kết quả.
        /// Nếu thất bại, sẽ truyền lỗi đầu tiên.
        /// </summary>
        /// <typeparam name="TResult">Kiểu giá trị của Result mới.</typeparam>
        /// <param name="binder">Hàm nhận T và trả về Result&lt;TResult&gt;.</param>
        public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
            => IsSuccess
               ? binder(Value)
               : Result<TResult>.Failure(FirstError);
    }
}
