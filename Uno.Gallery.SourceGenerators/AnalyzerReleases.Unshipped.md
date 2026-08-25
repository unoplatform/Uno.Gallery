### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
UGG0001 | SamplesGenerator | Error | Unexpected SamplePageAttribute constructor shape
UGG0002 | SamplesGenerator | Error | SamplePageAttribute applied to non-class target
UGG0003 | SamplesGenerator | Error | Unexpected SampleConditionalAttribute constructor shape
UGG0004 | SamplesGenerator | Warning | Duplicate sample title in generated catalog
UGG0005 | SamplesGenerator | Error | Invalid explicit slug (not lowercase ASCII alphanumeric with interior hyphens only)
UGG0006 | SamplesGenerator | Warning | Duplicate final slug (case-insensitive); both samples emit
UGG0007 | SamplesGenerator | Warning | RelatedSamples entry references an unknown final slug (ordinal match required)
UGG0008 | SamplesGenerator | Error | Null or empty element in metadata string array (Tags, RelatedSamples)
UGG0009 | SamplesGenerator | Error | Page type or DataType is abstract or has no accessible parameterless constructor
UGG0010 | SamplesGenerator | Error | Route constant identifier collision after PascalCase transformation; two different slugs produce the same C# identifier — conflicting constants are omitted from SampleRoutes
