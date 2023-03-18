global using System.Collections.Concurrent;
global using System.CommandLine;
global using System.CommandLine.Builder;
global using System.CommandLine.Hosting;
global using System.CommandLine.Invocation;
global using System.CommandLine.Parsing;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;

global using CsvHelper;
global using CsvHelper.Configuration;
global using CsvHelper.Configuration.Attributes;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Options;

global using Onspring.API.SDK;
global using Onspring.API.SDK.Enums;
global using Onspring.API.SDK.Models;

global using OnspringCLI.Commands;
global using OnspringCLI.Commands.Attachments;
global using OnspringCLI.Commands.Attachments.Download;
global using OnspringCLI.Extensions;
global using OnspringCLI.Interfaces;
global using OnspringCLI.Maps;
global using OnspringCLI.Models;
global using OnspringCLI.Processors;
global using OnspringCLI.Services;

global using Serilog;
global using Serilog.Core;
global using Serilog.Events;
global using Serilog.Sinks.SystemConsole.Themes;
