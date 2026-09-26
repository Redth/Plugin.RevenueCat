#nullable enable

using System.Text.RegularExpressions;

namespace Plugin.RevenueCat.Paywalls;

public static partial class PaywallTextProcessor
{
	public static string ProcessVariables(string text, IPaywallVariableProvider variableProvider, PaywallVariableContext context)
	{
		return VariableRegex().Replace(text, match =>
		{
			var variableName = match.Groups["name"].Value;
			return variableProvider.Resolve(variableName, context) ?? string.Empty;
		});
	}

	[GeneratedRegex(@"\{\{\s*(?<name>[a-zA-Z0-9_]+)\s*\}\}")]
	private static partial Regex VariableRegex();
}
