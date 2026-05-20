using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public sealed class RevenueCatError
{
	public const string SerializedExceptionPrefix = "RevenueCatError:";

	[JsonPropertyName("code")]
	public string? Code { get; set; }

	[JsonPropertyName("message")]
	public string Message { get; set; } = string.Empty;

	[JsonPropertyName("domain")]
	public string? Domain { get; set; }

	[JsonPropertyName("underlying_message")]
	public string? UnderlyingMessage { get; set; }

	[JsonPropertyName("source")]
	public string? Source { get; set; }

	[JsonPropertyName("exception_type")]
	public string? ExceptionType { get; set; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }

	public static RevenueCatError FromException(Exception exception)
	{
		var unwrapped = Unwrap(exception);
		var message = unwrapped.Message;

		if (!string.IsNullOrWhiteSpace(message) &&
			message.StartsWith(SerializedExceptionPrefix, StringComparison.Ordinal))
		{
			var json = message[SerializedExceptionPrefix.Length..];
			try
			{
				var error = JsonSerializer.Deserialize(json, ModelSerializerContext.Default.RevenueCatError);
				if (error is not null)
				{
					return new RevenueCatError
					{
						Code = error.Code,
						Message = error.Message,
						Domain = error.Domain,
						UnderlyingMessage = error.UnderlyingMessage,
						Source = error.Source,
						ExceptionType = unwrapped.GetType().FullName,
						ExtensionData = error.ExtensionData
					};
				}
			}
			catch
			{
				// Fall through to the generic exception mapping below.
			}
		}

		return new RevenueCatError
		{
			Code = unwrapped.GetType().Name,
			Message = string.IsNullOrWhiteSpace(message) ? "RevenueCat operation failed." : message,
			Source = "dotnet",
			ExceptionType = unwrapped.GetType().FullName
		};
	}

	public static RevenueCatError Cancelled(string? message = null)
		=> new()
		{
			Code = "PurchaseCancelledError",
			Message = message ?? "Purchase was cancelled by the user.",
			Source = "revenuecat"
		};

	static Exception Unwrap(Exception exception)
	{
		while (exception is AggregateException { InnerExceptions.Count: 1 } aggregate)
			exception = aggregate.InnerExceptions[0];

		return exception.InnerException is not null &&
			(exception.GetType().FullName?.Contains("ExecutionException", StringComparison.Ordinal) == true ||
			 exception.GetType().FullName?.Contains("Invocation", StringComparison.Ordinal) == true)
			? Unwrap(exception.InnerException)
			: exception;
	}

}
