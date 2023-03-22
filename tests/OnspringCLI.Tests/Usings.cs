global using System.CommandLine;
global using System.CommandLine.Invocation;
global using System.Globalization;
global using System.Net;

global using CsvHelper;
global using CsvHelper.Configuration;

global using FluentAssertions;

global using Moq;

global using Onspring.API.SDK;
global using Onspring.API.SDK.Enums;
global using Onspring.API.SDK.Models;

global using OnspringCLI.Commands;
global using OnspringCLI.Commands.Attachments;
global using OnspringCLI.Factories;
global using OnspringCLI.Interfaces;
global using OnspringCLI.Maps;
global using OnspringCLI.Models;
global using OnspringCLI.Services;
global using OnspringCLI.Tests.TestData;

global using Serilog;
global using Serilog.Core;
global using Serilog.Events;

global using Xunit;
