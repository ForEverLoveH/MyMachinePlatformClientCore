﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
 
using System;
using System.Collections.Generic;
using SqlSugar;

namespace MyMachinePlatformClientCore.Models.Models.dbModels;

public partial class Player
{

    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    [SugarColumn(IsNullable = false,ColumnDescription = "用户名")]
    public string UserName { get; set; }
    [SugarColumn(IsNullable = false, ColumnDescription = "密码")]
    public string Password { get; set; }
    [SugarColumn(IsNullable = false, ColumnDescription = "手机号")]
    public string TelPhone { get; set; }
    [SugarColumn(IsNullable = false, ColumnDescription = "邮箱")]
    public int Coin { get; set; }
}