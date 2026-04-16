using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DomainEnums = DontBeLazy.Domain.Enums;

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
        if (value is DomainEnums.TaskStatus status)
        {
            return status switch
            {
                DomainEnums.TaskStatus.Done => "#4CAF50",
                DomainEnums.TaskStatus.Active => "#7C4DFF",
                DomainEnums.TaskStatus.Abandoned => "#F44336",
                _ => "#9E9E9E"
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
        if (value is DomainEnums.TaskStatus status)
        {
            return status switch
            {
                DomainEnums.TaskStatus.Done => "Hoàn thành",
                DomainEnums.TaskStatus.Active => "Đang làm",
                DomainEnums.TaskStatus.Abandoned => "Bỏ cuộc",
                _ => "Chờ"
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
        if (value is DomainEnums.ProfileEntryType type)
        {
            return type switch
            {
                DomainEnums.ProfileEntryType.Website => "Web",
                DomainEnums.ProfileEntryType.App => "DesktopWindows",
                _ => "Help"
            };
        }
        return "Help";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
