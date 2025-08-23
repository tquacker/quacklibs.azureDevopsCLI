﻿namespace Quacklibs.AzureDevopsCli.Presentation.Tables;

/// <summary>
/// Easily create tables. This objects ensures that all our tables will look the same. 
/// </summary>
/// <typeparam name="T"></typeparam>
public class TableBuilder<T> : ISubColumnBuilder<T>, ITableColumnBuilder<T>
{
    private readonly Table _table;
    private readonly List<T> _rows = [];
    private readonly List<ColumnValue<T>> _columnValues = [];

    private TableBuilder()
    {
        _table = new Table()
                 .Border(TableBorder.Minimal)
                 .BorderColor(Color.Grey);
    }

    public static ITableColumnBuilder<T> Create() => new TableBuilder<T>();

    public ISubColumnBuilder<T> WithColumn(string name, ColumnValue<T> valueSelector)
    {
        _table.AddColumn(name);
        _columnValues.Add(valueSelector);

        return this;
    }

    public ISubColumnBuilder<T> WithSubColumn(string name, ColumnValue<T> valueSelector)
    {
        throw new NotImplementedException();
    }

    public ITableBuilder<T> WithRows(IEnumerable<T> rows)
    {
        foreach (var item in rows)
        {
            _rows.Add(item);
        }

        return this;
    }

    public ITableBuilder<T> WithOptions(Action<Table> dostuff)
    {
        dostuff(_table);
        return this;
    }

    public Table Build()
    {
        foreach (var row in _rows)
        {
            var columnValues = _columnValues.Select(columnValues => columnValues.ToString(row))
                                            .ToArray();
            _table.AddRow(columnValues);
        }

        return _table;
    }
}

public interface ITableColumnBuilder<T>
{
    public ISubColumnBuilder<T> WithColumn(string name, ColumnValue<T> valueSelector);
}

public interface ITableBuilder<T> : ITableColumnBuilder<T>
{
    public ITableBuilder<T> WithRows(IEnumerable<T> rows);
    public ITableBuilder<T> WithOptions(Action<Table> dostuff);
    public Table Build();
}

public interface ISubColumnBuilder<T> : ITableBuilder<T>
{
    public ISubColumnBuilder<T> WithSubColumn(string name, ColumnValue<T> valueSelector);
}