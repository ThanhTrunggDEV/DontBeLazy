using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DontBeLazy.Ports.DTOs;

namespace DontBeLazy.WPF.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = value is true;
        if (parameter is string p && p == "Inverse") flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}

public class InverseBoolConverter : IValueConverter
{
    public static readonly InverseBoolConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}

public class TaskStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TaskStatusDto status)
        {
            return status switch
            {
                TaskStatusDto.Done      => "#4CAF50",
                TaskStatusDto.Active    => "#7C4DFF",
                TaskStatusDto.Abandoned => "#F44336",
                _                       => "#9E9E9E"
            };
        }
        return "#9E9E9E";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class TaskStatusToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TaskStatusDto status)
        {
            return status switch
            {
                TaskStatusDto.Done      => "Hoàn thành",
                TaskStatusDto.Active    => "Đang làm",
                TaskStatusDto.Abandoned => "Bỏ cuộc",
                _                       => "Chờ"
            };
        }
        return "Chờ";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ProfileEntryTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProfileEntryTypeDto type)
        {
            return type switch
            {
                ProfileEntryTypeDto.Website => "Web",
                ProfileEntryTypeDto.App     => "DesktopWindows",
                _                           => "Help"
            };
        }
        return "Help";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Maps hours (0–4+) to a pixel bar height (4–120).</summary>
public class HoursToHeightConverter : IValueConverter
{
    private const double MaxHours  = 4.0;
    private const double MaxHeight = 120.0;
    private const double MinHeight = 4.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double hours)
            return Math.Max(MinHeight, Math.Min(MaxHeight, hours / MaxHours * MaxHeight));
        return MinHeight;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Returns the right check icon kind based on status.</summary>
public class TaskStatusToCheckIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is TaskStatusDto.Done ? "CheckCircle" : "CheckCircleOutline";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Compares TaskStatusDto to a string parameter.
/// Supports: "Abandoned" → Visible when Abandoned, "NotActive" → true when not Active.
/// Returns Visibility when targetType is Visibility, bool otherwise.
/// </summary>
public class TaskStatusEqualConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TaskStatusDto status || parameter is not string param)
            return targetType == typeof(Visibility) ? Visibility.Collapsed : (object)false;

        bool result = param switch
        {
            "Abandoned" => status == TaskStatusDto.Abandoned,
            "NotActive" => status != TaskStatusDto.Active,
            "Weekly"    => param == "Weekly",
            "Custom"    => param == "Custom",
            _           => false
        };

        return targetType == typeof(Visibility)
            ? (result ? Visibility.Visible : Visibility.Collapsed)
            : (object)result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>String equality converter for conditional visibility (e.g. RecurringType == "Weekly").</summary>
public class StringEqualConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool match = value?.ToString() == parameter?.ToString();
        return targetType == typeof(Visibility)
            ? (match ? Visibility.Visible : Visibility.Collapsed)
            : (object)match;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
