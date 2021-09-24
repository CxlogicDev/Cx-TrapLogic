namespace CxUtility.Cx_Console.DisplayMethods;

/// <summary>
/// A method for displaying Out a Table View
/// </summary>
public record TableHeaderItem(string Title, System.Reflection.PropertyInfo? objectPlace)
{
    /// <summary>
    /// Calculated Max Column Char Size based on biggest display string Length. Limit max of 100 Chars. 
    /// </summary>
    public int MaxValueCount { get; internal set; }
}

public record DisplayTable
{
    //ToDo: Handle Null Conditions 
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private DisplayTable()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        //validate();
    }

    //public DisplayTable(TableHeaderItem[] Columns)
    //{
    //    TableColumn = Columns;
    //    validate();
    //}

    void validate()
    {
        if (TableColumn == null || TableColumn.Length <= 0)
            throw new ArgumentNullException($"The {nameof(TableColumn)} field does not have any values.");
    }

    public static DisplayTable CreateTableObject<displayObject>((string propName, string DisplayName)[] ColumnHeaderProps, bool showRowCount = true) where displayObject : class
        => CreateTableObject<displayObject>(ColumnHeaderProps, (showRowCount, ""));
    /// <summary>
    /// Will build a table base off of a specific object 
    /// </summary>
    /// <typeparam name="displayObject">The Object used to build the Table</typeparam>
    /// <param name="ColumnHeaderProps">The columns that are being set up to display</param>
    /// <returns></returns>
    public static DisplayTable CreateTableObject<displayObject>((string propName, string DisplayName)[] ColumnHeaderProps, (bool showRowCount, string RowTitle) AutoInc) where displayObject : class
    {
        var showRowCount = AutoInc.showRowCount;

        TableHeaderItem[] TBI = new TableHeaderItem[ColumnHeaderProps.Length];

        if (showRowCount)
        {
            TBI = new TableHeaderItem[ColumnHeaderProps.Length + 1];
            TBI[0] = new TableHeaderItem("-ct", null)
            {
                MaxValueCount = (AutoInc.RowTitle ?? "").Length
            };
        }

        for (int i = 0; i < ColumnHeaderProps.Length; i++)
        {
            var Col = ColumnHeaderProps[i];
            var chProp = typeof(displayObject).GetProperty(Col.propName);

            if (chProp == null)
                continue;

            TBI[showRowCount ? i + 1 : i] = new TableHeaderItem(ColumnHeaderProps[i].DisplayName, chProp)
            {
                MaxValueCount = ColumnHeaderProps[i].DisplayName.Length
            };
        }

        if (TBI.Length > 0)
            return new DisplayTable()
            {
                TableColumn = TBI,
                autoInc = AutoInc,
                //showRowCount = showRowCount
            };

        throw new FormatException("Could not generate any Table Columns. Please check your column name match the object you are tring to create a table for ");
    }

    /// <summary>
    /// The Table Column muat be from the same Class or record. 
    /// I suggest Creating a Class or record and allowing that transfer the products
    /// </summary>

    public TableHeaderItem[] TableColumn { get; init; }

    //bool showRowCount { get; set; }

    (bool showRowCount, string RowTitle) autoInc { get; set; }

    List<TableRow> Rows { get; } = new List<TableRow>();

    private Type? TableType
    {
        get
        {
            if (TableColumn.Length > 0)
            {
                var tType = TableColumn.Where(w => !w.Title.Equals("-ct") && w.objectPlace != null).Select(s => s.objectPlace?.DeclaringType).Distinct().ToArray();

                if (tType.Length == 1)
                    return tType.FirstOrDefault();
            }

            throw new FormatException("The Table Is not formatted correctly. There Are Multiple different Column Types. The table Must have a Single Display class or record");
        }
    }

    /// <summary>
    /// This object must match the PropertyInfo object from the TableHeaderItem will 
    /// </summary>
    /// <param name="rowobjects"></param>
    public void AddRowObject(params object[] rowobjects)
    {

        if (rowobjects == null || rowobjects.Length <= 0)
            return;

        var RowTypes = rowobjects.Select(s => s.GetType()).Distinct();
        if (RowTypes.Count() > 1)//
            throw new FormatException($"The Table rowObjects must be of the same type used to calculate the TableHeaderItems. ColumnType: {TableType?.FullName}; Row Types Found: {string.Join(", ", RowTypes.Select(s => s.FullName))}");

        var RowType = RowTypes.FirstOrDefault();

        if (RowType != TableType)
            throw new FormatException($"The Row Type: [{RowType?.FullName}] does not match the Header Type [{TableType?.FullName}] ");

        int ct = Rows.Count();

        foreach (var item in rowobjects)
        {
            ct++;

            var row = new TableRow(item, ct);

            _ = TableColumn.Select(s => ud_maxCt(row, s)).ToArray();

            //Below Line was used to test column maxcount
            //System.Console.WriteLine(string.Join(" | ", t_out));

            Rows.Add(row);
        }

        //System.Console.WriteLine();
        //System.Console.WriteLine();

        int ud_maxCt(TableRow iTtem, TableHeaderItem hItm)
        {
            if (hItm.Title.Equals("-ct"))
            {
                hItm.MaxValueCount = autoInc.RowTitle.Length > iTtem.rowId.ToString().Length ? autoInc.RowTitle.Length : iTtem.rowId.ToString().Length;
                return hItm.MaxValueCount;
            }

            var val = hItm.objectPlace?.GetValue(iTtem.rowValue)?.ToString() ?? "";

            hItm.MaxValueCount = val.Length < hItm.MaxValueCount ? hItm.MaxValueCount : val.Length;

            return hItm.MaxValueCount;
        }

    }

    internal string[] writeConsoleTableLines()
    {
        List<string> lines = new List<string>();

        var HeaderLine = TableColumn.Select(s => $"{(s.Title.Equals("-ct") ? autoInc.RowTitle : s.Title)}{new string(' ', s.MaxValueCount - (s.Title.Equals("-ct") ? autoInc.RowTitle : s.Title).Length)}");
        var headerLine = $"   {string.Join(" | ", HeaderLine)}";

        lines.Add($"   {string.Join(" | ", HeaderLine)}");
        lines.Add($"   {string.Join("---", HeaderLine.Select(s => new string('-', s.Length)))}");

        if (Rows.Count > 0)
        {
            //var ct = 0;
            foreach (var i in Rows)
            {
                //ct++;
                var lineparts = TableColumn.Select(s => p_title(i, s));// $"{(s.objectPlace.GetValue(i)?.ToString() ?? "")}{new string(' ', s.MaxValueCount - (s.objectPlace.GetValue(i)?.ToString() ?? "").Length)}");
                var line = $"   {string.Join(" | ", lineparts)}";
                lines.Add(line);
            }
        }

        return lines.ToArray();

        //Write the Body
        string p_title(TableRow iTtem, TableHeaderItem hItm)
        {
            if (hItm.Title.Equals("-ct"))
            {
                //hItm.MaxValueCount = iTtem.rowId.ToString().Length;
                return $"{new string(' ', hItm.MaxValueCount - iTtem.rowId.ToString().Length)}{iTtem.rowId}";
            }

            var val = hItm.objectPlace?.GetValue(iTtem.rowValue)?.ToString() ?? "";

            //hItm.MaxValueCount = val.Length < hItm.MaxValueCount ? hItm.MaxValueCount : val.Length;

            return $"{val}{new string(' ', hItm.MaxValueCount - val.Length)}";
        }
    }

    record TableRow(object rowValue, int rowId);
}


/*
     Infomation: 
        This will display information in a table style to the console Screen. The Screen size may be limited so watch how you many things you try to display. 


    Table Records: 

        DisplayTable >> Collects a array of header Objects to display using the display props 

            Ex: 
            var tableDisplay = new DisplayTable {
            
                TableColumn = new[] {
                    new TableHeaderItem("{Column-1-Title}", {Table Fill object propertyInfo}),
                    new TableHeaderItem("{Column-2-Title}", {Table Fill object propertyInfo}),
                    new TableHeaderItem("{Column-3-Title}", {Table Fill object propertyInfo}),
                    ...,
                    ...,
                    ...,
                    new TableHeaderItem("{Column-n-Title}", {Table Fill object propertyInfo}),
                }

            }

     
     */