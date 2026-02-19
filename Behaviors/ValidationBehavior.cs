namespace BugFlow.Behaviors;

public class ValidationBehavior : Behavior<View>
{
    public static readonly BindableProperty IsValidProperty =
        BindableProperty.CreateAttached("IsValid", typeof(bool), typeof(ValidationBehavior), true);

    public static bool GetIsValid(BindableObject view) => (bool)view.GetValue(IsValidProperty);
    public static void SetIsValid(BindableObject view, bool value) => view.SetValue(IsValidProperty, value);

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        if (bindable is Entry entry)
        {
            entry.TextChanged += OnTextChanged;
            Validate(entry, entry.Text);
        }
        else if (bindable is Editor editor)
        {
            editor.TextChanged += OnTextChanged;
            Validate(editor, editor.Text);
        }
    }

    protected override void OnDetachingFrom(View bindable)
    {
        base.OnDetachingFrom(bindable);
        if (bindable is Entry entry)
            entry.TextChanged -= OnTextChanged;
        else if (bindable is Editor editor)
            editor.TextChanged -= OnTextChanged;
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is View view)
            Validate(view, e.NewTextValue);
    }

    private static void Validate(View view, string? text)
    {
        bool valid = !string.IsNullOrWhiteSpace(text);
        SetIsValid(view, valid);
        view.BackgroundColor = valid ? Colors.Transparent : Color.FromArgb("#FFEBEE"); // light red tint on invalid
    }
}
