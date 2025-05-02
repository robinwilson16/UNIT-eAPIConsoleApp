using System;
using System.Collections.Generic;

namespace NetSuiteIntegration.Models;

public partial class Setting
{
    public int Id { get; set; }

    public string? Enviroment { get; set; }

    public bool? Enabled { get; set; }

    public string? NetSuiteAccountID { get; set; }

    public string? NetSuiteURL { get; set; }

    public string? NetSuiteConsumerKey { get; set; }

    public string? NetSuiteTokenID { get; set; }

    public string? NetSuiteConsumerSecret { get; set; }

    public string? NetSuiteTokenSecret { get; set; }

    public string? UniteBaseURL { get; set; }

    public string? UniteTokenURL { get; set; }

    public string? UniteAPIKey { get; set; }
}
