using BugFlow.Domain;

namespace BugFlow.Behaviors;

public class EmailValidationBehavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.TextChanged += OnTextChanged;
        Validate(bindable, bindable.Text);
    }

    protected override void OnDetachingFrom(Entry bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.TextChanged -= OnTextChanged;
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
            Validate(entry, e.NewTextValue);
    }

    private static void Validate(Entry entry, string? text)
    {
        bool valid = ValidationRules.IsValidEmail(text);
        ValidationBehavior.SetIsValid(entry, valid);
        entry.TextColor = valid ? Colors.Black : Colors.Red;
        entry.BackgroundColor = valid ? Colors.Transparent : Color.FromArgb("#FFEBEE");
    }
}
