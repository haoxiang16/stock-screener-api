﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace StockAPI.Models;

public partial class Company
{
    public string CompanyCode { get; set; }

    public string CompanyName { get; set; }

    public virtual ICollection<CashFlowStatement> CashFlowStatements { get; set; } = new List<CashFlowStatement>();

    public virtual ICollection<IncomeStatement> IncomeStatements { get; set; } = new List<IncomeStatement>();
}