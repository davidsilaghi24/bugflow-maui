namespace BugFlow.Behaviors;

public class FutureDateValidationBehavior : Behavior<DatePicker>
{
    public static readonly BindableProperty IsValidProperty =
        BindableProperty.CreateAttached("IsValid", typeof(bool), typeof(FutureDateValidationBehavior), true);

    public static bool GetIsValid(BindableObject view) => (bool)view.GetValue(IsValidProperty);
    public static void SetIsValid(BindableObject view, bool value) => view.SetValue(IsValidProperty, value);

    protected override void OnAttachedTo(DatePicker bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.DateSelected += OnDateSelected;
        Validate(bindable, bindable.Date);
    }

    protected override void OnDetachingFrom(DatePicker bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.DateSelected -= OnDateSelected;
    }

    private void OnDateSelected(object? sender, DateChangedEventArgs e)
    {
        if (sender is DatePicker picker)
            Validate(picker, e.NewDate);
    }

    // same red background as the text validators, keeps the UI consistent
    private static void Validate(DatePicker picker, DateTime date)
    {
        bool valid = date.Date >= DateTime.Today;
        SetIsValid(picker, valid);
        picker.BackgroundColor = valid ? Colors.Transparent : Color.FromArgb("#FFEBEE");
    }
}
