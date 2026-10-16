#if !WINDOWS
using Windows.Foundation;

// Uno 7.0 migration: stands in for the Uno.CommunityToolkit.WinUI WrapPanel, which has no 7.0 build.
namespace CommunityToolkit.WinUI.UI.Controls;

public partial class WrapPanel : Panel
{
	public static DependencyProperty HorizontalSpacingProperty { get; } = DependencyProperty.Register(
		nameof(HorizontalSpacing), typeof(double), typeof(WrapPanel), new PropertyMetadata(0d, OnLayoutPropertyChanged));

	public static DependencyProperty VerticalSpacingProperty { get; } = DependencyProperty.Register(
		nameof(VerticalSpacing), typeof(double), typeof(WrapPanel), new PropertyMetadata(0d, OnLayoutPropertyChanged));

	public double HorizontalSpacing
	{
		get => (double)GetValue(HorizontalSpacingProperty);
		set => SetValue(HorizontalSpacingProperty, value);
	}

	public double VerticalSpacing
	{
		get => (double)GetValue(VerticalSpacingProperty);
		set => SetValue(VerticalSpacingProperty, value);
	}

	private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((WrapPanel)d).InvalidateMeasure();

	protected override Size MeasureOverride(Size availableSize)
	{
		foreach (var child in Children)
		{
			child.Measure(availableSize);
		}

		return Layout(availableSize.Width, arrange: false);
	}

	protected override Size ArrangeOverride(Size finalSize) => Layout(finalSize.Width, arrange: true);

	private Size Layout(double width, bool arrange)
	{
		double x = 0, y = 0, rowHeight = 0, maxWidth = 0;
		foreach (var child in Children)
		{
			var size = child.DesiredSize;
			if (x > 0 && x + size.Width > width)
			{
				x = 0;
				y += rowHeight + VerticalSpacing;
				rowHeight = 0;
			}

			if (arrange)
			{
				child.Arrange(new Rect(x, y, size.Width, size.Height));
			}

			x += size.Width + HorizontalSpacing;
			rowHeight = Math.Max(rowHeight, size.Height);
			maxWidth = Math.Max(maxWidth, x - HorizontalSpacing);
		}

		return new Size(maxWidth, y + rowHeight);
	}
}
#endif
