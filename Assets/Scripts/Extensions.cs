using System;
using System.Collections.Generic;
using Colyseus.Schema;

public static class Extensions
{
    public static void OnDataChange<T>(this T obj, Action<DataChange> handleChange) where T : Schema
    {
        obj.OnChange += (List<DataChange> changes) => changes.ForEach(change =>
        {
            handleChange(change);
        });
    }
}
