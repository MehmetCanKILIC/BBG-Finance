using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace BBGFinance.Core
{
    public static class JsonHelper
    {
        public static string DataTableToJson(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    object val = row[col];
                    if (val == DBNull.Value)
                        dict[col.ColumnName] = null;
                    else if (val is bool)
                        dict[col.ColumnName] = (bool)val;
                    else if (val is DateTime)
                        dict[col.ColumnName] = ((DateTime)val).ToString("yyyy-MM-dd");
                    else
                        dict[col.ColumnName] = val;
                }
                list.Add(dict);
            }
            return JsonConvert.SerializeObject(list);
        }
    }
}
