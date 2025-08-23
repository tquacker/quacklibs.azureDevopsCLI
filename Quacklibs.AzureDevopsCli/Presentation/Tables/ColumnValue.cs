namespace Quacklibs.AzureDevopsCli.Presentation.Tables;

public class ColumnValue<T>
{
    private readonly Func<T, string?> _columnValueSelector;
    private readonly TableColor _color;
    private readonly bool _isMarkup;

    public ColumnValue(Func<T, string?> columnValueSelector, bool isMarkup = false)
    {
        _columnValueSelector = columnValueSelector;
        //TODO: this default assumes that the user has an black console. 
        _color = TableColor.White;
        _isMarkup = isMarkup;
    }

    public ColumnValue(Func<T, string?> columnValueSelector, TableColor color, bool isMarkup = false) : this(columnValueSelector, isMarkup)
    {
        _color = color;
    }

    public string ToString(T value)
    {
        var columnValue = _columnValueSelector(value) ?? string.Empty;
        var safeColumnValue =  !_isMarkup ? Markup.Escape(columnValue) : columnValue;
        //return $"[{_color.Value}]{columnValue}[/]";
        return safeColumnValue;
    }

}
