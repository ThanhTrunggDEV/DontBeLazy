using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DontBeLazy.Ports.DTOs;

namespace DontBeLazy.WPF.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}

public class InverseBoolConverter : IValueConverter
{
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
