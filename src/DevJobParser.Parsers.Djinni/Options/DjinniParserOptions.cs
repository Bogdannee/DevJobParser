using System;

namespace DevJobParser.Parsers.Djinni.Options;

public class DjinniParserOptions
{
    public string SourceName {get; set;} = string.Empty;
    public string SearchLink { get; set; } = string.Empty;
    public int MaxPages { get; set; } = 50;
}