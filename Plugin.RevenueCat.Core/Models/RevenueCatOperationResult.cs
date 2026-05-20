namespace Plugin.RevenueCat.Models;

public sealed class RevenueCatOperationResult<T>
{
	public bool IsSuccess { get; init; }

	public bool UserCancelled { get; init; }

	public T? Value { get; init; }

	public RevenueCatError? Error { get; init; }

	public static RevenueCatOperationResult<T> Success(T? value)
		=> new()
		{
			IsSuccess = true,
			Value = value
		};

	public static RevenueCatOperationResult<T> Failure(RevenueCatError error)
		=> new()
		{
			IsSuccess = false,
			Error = error
		};

	public static RevenueCatOperationResult<T> Cancelled(T? value = default, RevenueCatError? error = null)
		=> new()
		{
			IsSuccess = false,
			UserCancelled = true,
			Value = value,
			Error = error ?? RevenueCatError.Cancelled()
		};
}
