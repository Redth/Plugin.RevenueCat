using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Plugin.RevenueCat.Models;
using Plugin.RevenueCat.Paywalls;
using Plugin.RevenueCat.Paywalls.Rendering.Maui;

namespace Tests;

[TestClass]
[DoNotParallelize]
public sealed class PaywallAccessibilityTests
{
	[TestInitialize]
	public void InitializeDispatcher() => DispatcherProvider.SetCurrent(new TestDispatcherProvider());

	[TestCleanup]
	public void ResetDispatcher() => DispatcherProvider.SetCurrent(null);

	[TestMethod]
	public void Package_Selection_Updates_The_Same_Accessible_Action_And_Its_Name()
	{
		var view = CreateView(new PaywallPackageComponent
		{
			Id = "annual",
			PackageId = "annual",
			Stack = new() { Components = [new PaywallTextComponent { Id = "card-copy", TextLocalizationId = "Annual plan" }] }
		}, new PaywallPackageComponent { Id = "monthly", PackageId = "monthly" });
		var annual = Find<Border>(view, "annual");
		var monthly = Find<Border>(view, "monthly");
		var action = PaywallAccessibility.GetSurface(annual)!;
		var originalRoot = view.Content;

		Assert.AreEqual("Annual plan", SemanticProperties.GetDescription(action));
		Assert.AreEqual("Select option", SemanticProperties.GetHint(action));
		Assert.IsTrue(AutomationProperties.GetIsInAccessibleTree(action) == true);
		Assert.AreEqual(false, AutomationProperties.GetIsInAccessibleTree(Find<Label>(view, "card-copy")));
		Assert.IsFalse(annual.IsEnabled == false);

		Activate(annual);
		Assert.AreSame(originalRoot, view.Content);
		Assert.AreSame(action, PaywallAccessibility.GetSurface(annual));
		Assert.IsTrue(PaywallAccessibility.GetSelected(annual));

		Activate(monthly);
		Assert.IsFalse(PaywallAccessibility.GetSelected(annual));
		Find<Label>(view, "card-copy").Text = "Yearly plan";
		Assert.AreEqual("Yearly plan", SemanticProperties.GetDescription(action));
	}

	[TestMethod]
	public void Disabled_Action_Cannot_Execute_Either_Activation_Path()
	{
		var view = CreateView(new PaywallPackageComponent { Id = "missing", PackageId = "missing" });
		var card = Find<Border>(view, "missing");
		var action = PaywallAccessibility.GetSurface(card)!;
		Assert.IsFalse(card.IsEnabled);
		Assert.IsFalse(action.IsEnabled);
		Activate(card);
		action.Command.Execute(null);
		Assert.IsFalse(card.IsEnabled);
	}

	[TestMethod]
	public void Busy_Action_Disables_Other_Controls_And_Rejects_Duplicate_Activation()
	{
		var pending = new TaskCompletionSource<CustomerInfo?>();
		var calls = 0;
		var view = CreateView(new PaywallPurchaseButtonComponent { Id = "buy" },
			new PaywallPurchaseButtonComponent { Id = "buy-again" });
		view.ActionHandler = new PurchaseHandler(() =>
		{
			calls++;
			return pending.Task;
		});
		var first = Find<Border>(view, "buy");
		var second = Find<Border>(view, "buy-again");
		var firstAction = PaywallAccessibility.GetSurface(first)!;
		var secondAction = PaywallAccessibility.GetSurface(second)!;

		firstAction.Command.Execute(null);
		Assert.AreEqual(1, calls);
		Assert.IsFalse(firstAction.IsEnabled);
		Assert.IsFalse(secondAction.IsEnabled);
		PaywallAccessibility.SetEnabled(second, true);
		Assert.IsFalse(secondAction.IsEnabled);
		Activate(first);
		secondAction.Command.Execute(null);
		Assert.AreEqual(1, calls);
		pending.SetResult(null);
		Assert.IsTrue(firstAction.IsEnabled);
		Assert.IsTrue(secondAction.IsEnabled);
	}

	[TestMethod]
	public void Toggle_Uses_Custom_Track_With_One_Accessible_Checkable_Action()
	{
		var view = CreateView(new PaywallTabsComponent
		{
			Id = "tabs",
			Control = System.Text.Json.JsonSerializer.SerializeToElement(new
			{
				stack = new
				{
					type = "stack",
					components = new[] { new { type = "tab_control_toggle", id = "plan-toggle" } }
				}
			}),
			Tabs =
			[
				new() { Id = "monthly", Name = "Monthly", Stack = new() },
				new() { Id = "annual", Name = "Annual", Stack = new() }
			]
		});
		var toggle = Find<Border>(view, "plan-toggle");
		Assert.IsFalse(Descendants(toggle).OfType<Switch>().Any());
		Assert.AreEqual("Change plan", SemanticProperties.GetDescription(PaywallAccessibility.GetSurface(toggle)!));
		Activate(toggle);
		Assert.IsTrue(PaywallAccessibility.GetSelected(Find<Border>(view, "plan-toggle")));
	}

	static RevenueCatPaywallView CreateView(params PaywallComponent[] components) => new()
	{
		Packages =
		[
			new() { Identifier = "annual", StoreProduct = new() { PriceString = "$49.99" } },
			new() { Identifier = "monthly", StoreProduct = new() { PriceString = "$6.99" } }
		],
		PaywallData = new() { ComponentsConfig = new() { Base = new() { Stack = new() { Components = components.ToList() } } } }
	};

	static T Find<T>(View view, string id) where T : View =>
		Descendants(view).OfType<T>().Single(v => v.AutomationId == id);

	static void Activate(View view)
	{
		var command = view.GestureRecognizers.OfType<TapGestureRecognizer>().Single().Command;
		Assert.IsNotNull(command);
		command.Execute(null);
	}

	static IEnumerable<View> Descendants(View view)
	{
		yield return view;
		IEnumerable<View> children = view switch
		{
			Layout layout => layout.Children.OfType<View>(),
			Border { Content: View child } => [child],
			ContentView { Content: View child } => [child],
			ScrollView { Content: View child } => [child],
			_ => []
		};
		foreach (var child in children.SelectMany(Descendants))
		{
			yield return child;
		}
	}

	sealed class TestDispatcherProvider : IDispatcherProvider
	{
		public IDispatcher GetForCurrentThread() => new TestDispatcher();
	}

	sealed class TestDispatcher : IDispatcher
	{
		public bool IsDispatchRequired => false;
		public bool Dispatch(Action action) { action(); return true; }
		public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotSupportedException();
		public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
	}

	sealed class PurchaseHandler(Func<Task<CustomerInfo?>> purchase) : IPaywallActionHandler
	{
		public Task<CustomerInfo?> PurchaseAsync(PaywallPurchaseRequest request, CancellationToken cancellationToken = default) => purchase();
		public Task<CustomerInfo?> RestoreAsync(CancellationToken cancellationToken = default) => Task.FromResult<CustomerInfo?>(null);
		public Task DismissAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
		public Task NavigateAsync(PaywallNavigationRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
	}
}
