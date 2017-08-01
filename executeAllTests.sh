#!/bin/bash

dotnet restore

dotnet build

dotnet test ./Facebook.Spreadsheets.Tests/Facebook.Spreadsheets.Tests.csproj