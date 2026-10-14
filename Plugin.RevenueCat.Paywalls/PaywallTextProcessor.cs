#nullable enable

using System.Text.RegularExpressions;

namespace Plugin.RevenueCat.Paywalls;

public static partial class PaywallTextProcessor
{
	public static string ProcessVariables(string text, IPaywallVariableProvider variableProvider, PaywallVariableContext context)
		=> ProcessVariables(text, variableProvider, context, null);

	public static string ProcessVariables(
		string text, IPaywallVariableProvider variableProvider, PaywallVariableContext context, Action<string>? diagnostic)
	{
		return VariableRegex().Replace(text, match =>
		{
			var variableName = match.Groups["name"].Value;
			var value = variableProvider.Resolve(variableName, context);
			if (value is null)
			{
				diagnostic?.Invoke($"Paywall variable '{variableName}' is unavailable in this context.");
			}
			return value ?? string.Empty;
		});
	}

	[GeneratedRegex(@"\{\{\s*(?<name>[a-zA-Z0-9_]+)\s*\}\}")]
	private static partial Regex VariableRegex();
}
