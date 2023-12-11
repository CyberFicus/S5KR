using System;
using System.Collections.Generic;

namespace Simple_RSC;

public partial class Exception
{
    public int Id { get; set; }

    public string InputString { get; set; } = null!;

    public string? Message { get; set; }

    public string? StackTrace { get; set; }
}
