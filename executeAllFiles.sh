#!/bin/bash

dotnet restore

dotnet build 

echo ""
echo "Valid Spreadsheets"
echo ""
for file in ./Facebook.Spreadsheets.Tests/testFiles/valid/*.txt
do
  dotnet run --project ./Facebook.SpreadsheetConsole/Facebook.SpreadsheetConsole.csproj -- "$file" "output.txt"
  
  if cmp --silent "${file}.out" "output.txt" ; then
    echo 'Actual Output File Equals Expected Output File'
  else
    echo "ERROR: The actual output file and the expected output file are different"
    exit 1
  fi

  echo ""
done

echo "Files with parsing errors"
echo ""
for file in ./Facebook.Spreadsheets.Tests/testFiles/invalidParsing/*
do
  dotnet run --project ./Facebook.SpreadsheetConsole/Facebook.SpreadsheetConsole.csproj -- "$file" "output.txt"
  echo ""
done

echo "Files with evaluation errors"
echo ""
for file in ./Facebook.Spreadsheets.Tests/testFiles/invalidEval/*
do
  dotnet run --project ./Facebook.SpreadsheetConsole/Facebook.SpreadsheetConsole.csproj -- "$file" "output.txt"
  echo ""
done

