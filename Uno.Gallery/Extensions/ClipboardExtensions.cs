using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Uno.Extensions;
using Uno.Logging;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Uno.Gallery
{
	/// <summary>
	/// Helper class for clipboard manipulation
	/// </summary>
	public class ClipboardExtensions
	{
		/* Helper class for clipboard manipulation:
		 * - CopyTrigger: condition for when the clipboard will be updated with CopyContent
		 * - CopyContent: text to copy; note: null or empty string will prevent the clipboard from being set
		 * - CopySubcription: [reserved] disposable to manage event subscription
		 */

		public enum CopyTrigger { None, ButtonClicked }

		#region Property: CopyTrigger

		public static DependencyProperty CopyTriggerProperty
		{
			[DynamicDependency(nameof(GetCopyTrigger))]
			[DynamicDependency(nameof(SetCopyTrigger))]
			get;
		} = DependencyProperty.RegisterAttached(
			"CopyTrigger",
			typeof(CopyTrigger),
			typeof(ClipboardExtensions),
			new PropertyMetadata(default, (d, e) => d.Maybe<FrameworkElement>(control => OnCopyTriggerChanged(control, e))));

		public static CopyTrigger GetCopyTrigger(FrameworkElement obj) => (CopyTrigger)obj.GetValue(CopyTriggerProperty);
		public static void SetCopyTrigger(FrameworkElement obj, CopyTrigger value) => obj.SetValue(CopyTriggerProperty, value);

		#endregion
		#region Property: CopyContent

		public static DependencyProperty CopyContentProperty
		{
			[DynamicDependency(nameof(GetCopyContent))]
			[DynamicDependency(nameof(SetCopyContent))]
			get;
		} = DependencyProperty.RegisterAttached(
			"CopyContent",
			typeof(string),
			typeof(ClipboardExtensions),
			new PropertyMetadata(default));

		public static string GetCopyContent(FrameworkElement obj) => (string)obj.GetValue(CopyContentProperty);
		public static void SetCopyContent(FrameworkElement obj, string value) => obj.SetValue(CopyContentProperty, value);

		#endregion
		#region Property: CopySubscription (private)

		private static DependencyProperty CopySubscriptionProperty
		{
			[DynamicDependency(nameof(GetCopySubscription))]
			[DynamicDependency(nameof(SetCopySubscription))]
			get;
		} = DependencyProperty.RegisterAttached(
			"CopySubscription",
			typeof(IDisposable),
			typeof(ClipboardExtensions),
			new PropertyMetadata(default));

		private static IDisposable GetCopySubscription(FrameworkElement obj) => (IDisposable)obj.GetValue(CopySubscriptionProperty);
		private static void SetCopySubscription(FrameworkElement obj, IDisposable value) => obj.SetValue(CopySubscriptionProperty, value);

		#endregion

		private static void OnCopyTriggerChanged(FrameworkElement sender, DependencyPropertyChangedEventArgs e)
		{
			GetCopySubscription(sender)?.Dispose();

			if (e.NewValue is CopyTrigger trigger && trigger != CopyTrigger.None)
			{
				var disposable = BindCopyHandler(sender, trigger);

				SetCopySubscription(sender, disposable);
			}
		}

		private static IDisposable BindCopyHandler(FrameworkElement sender, CopyTrigger trigger)
		{
			if (trigger == CopyTrigger.ButtonClicked && sender is ButtonBase button)
			{
				return new AnonymousDisposable(() => button.Click += RoutedEventHandler, () => button.Click -= RoutedEventHandler);
			}
			else
			{
				throw new NotSupportedException($"Trigger '{trigger}' is not supported/implemented for '{sender.GetType()}'");
			}

			void RoutedEventHandler(object s, RoutedEventArgs e) => SetClipboard(sender);
		}

		private static void SetClipboard(FrameworkElement sender)
		{
			if (GetCopyContent(sender) is var content && !string.IsNullOrEmpty(content))
			{
				var package = new DataPackage();
				package.SetText(content);

				Clipboard.SetContent(package);
			}
			else
			{
				typeof(ClipboardExtensions).Log().Warn($"Clipboard is not being set because the copy-content is empty or null.");
			}
		}
	}
}
