#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $SearchRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$assemblies = @(Get-ChildItem $SearchRoot -Recurse -File -Filter 'Uno.Gallery.dll' |
    Where-Object { $_.FullName -match '[\\/]linked[\\/]' } |
    Sort-Object LastWriteTimeUtc -Descending)
if ($assemblies.Count -eq 0) {
    throw "No linked Uno.Gallery.dll found under '$SearchRoot'."
}
$assembly = $assemblies[0]

$stream = [System.IO.File]::OpenRead($assembly.FullName)
try {
    $peReader = [System.Reflection.PortableExecutable.PEReader]::new($stream)
    try {
        $metadata = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($peReader)
        $registrationForm = $null
        $patternJsonContext = $null
        foreach ($handle in $metadata.TypeDefinitions) {
            $type = $metadata.GetTypeDefinition($handle)
            $name = $metadata.GetString($type.Name)
            $namespace = $metadata.GetString($type.Namespace)
            if ("$namespace.$name" -eq 'Uno.Gallery.ExtensionsPatterns.Core.RegistrationForm') {
                $registrationForm = $type
            }
            elseif ("$namespace.$name" -eq 'Uno.Gallery.ExtensionsPatterns.Core.PatternJsonContext') {
                $patternJsonContext = $type
            }
        }
        if ($null -eq $registrationForm) {
            throw 'Linked assembly does not contain RegistrationForm.'
        }
        if ($null -eq $patternJsonContext) {
            throw 'Linked assembly does not contain the storage JSON serialization context.'
        }

        $methods = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($methodHandle in $registrationForm.GetMethods()) {
            [void]$methods.Add($metadata.GetString($metadata.GetMethodDefinition($methodHandle).Name))
        }
        foreach ($getter in @('get_Name', 'get_Email', 'get_Age')) {
            if (-not $methods.Contains($getter)) {
                throw "Linked RegistrationForm is missing '$getter'; DataAnnotations validation was trimmed."
            }

            $jsonContextMethods = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
            foreach ($methodHandle in $patternJsonContext.GetMethods()) {
                [void]$jsonContextMethods.Add(
                    $metadata.GetString($metadata.GetMethodDefinition($methodHandle).Name))
            }
            foreach ($methodName in @('get_String', 'GetTypeInfo')) {
                if (-not $jsonContextMethods.Contains($methodName)) {
                    throw "Linked PatternJsonContext is missing '$methodName'."
                }
            }
        }

        $expectedAttributes = [ordered]@{
            Name = @(
                'System.ComponentModel.DataAnnotations.RequiredAttribute',
                'System.ComponentModel.DataAnnotations.StringLengthAttribute'
            )
            Email = @(
                'System.ComponentModel.DataAnnotations.RequiredAttribute',
                'System.ComponentModel.DataAnnotations.EmailAddressAttribute'
            )
            Age = @('System.ComponentModel.DataAnnotations.RangeAttribute')
        }
        $properties = @{}
        foreach ($propertyHandle in $registrationForm.GetProperties()) {
            $property = $metadata.GetPropertyDefinition($propertyHandle)
            $propertyName = $metadata.GetString($property.Name)
            $attributes = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
            foreach ($attributeHandle in $property.GetCustomAttributes()) {
                $attribute = $metadata.GetCustomAttribute($attributeHandle)
                if ($attribute.Constructor.Kind -ne [System.Reflection.Metadata.HandleKind]::MemberReference) {
                    continue
                }
                $constructor = $metadata.GetMemberReference(
                    [System.Reflection.Metadata.MemberReferenceHandle]$attribute.Constructor)
                if ($constructor.Parent.Kind -ne [System.Reflection.Metadata.HandleKind]::TypeReference) {
                    continue
                }
                $attributeType = $metadata.GetTypeReference(
                    [System.Reflection.Metadata.TypeReferenceHandle]$constructor.Parent)
                [void]$attributes.Add(
                    "$($metadata.GetString($attributeType.Namespace)).$($metadata.GetString($attributeType.Name))")
            }
            $properties[$propertyName] = $attributes
        }
        foreach ($entry in $expectedAttributes.GetEnumerator()) {
            if (-not $properties.ContainsKey($entry.Key)) {
                throw "Linked RegistrationForm is missing property '$($entry.Key)'."
            }
            foreach ($attributeName in $entry.Value) {
                if (-not $properties[$entry.Key].Contains($attributeName)) {
                    throw "Linked RegistrationForm.$($entry.Key) is missing '$attributeName'."
                }
            }
        }
    } finally {
        $peReader.Dispose()
    }
} finally {
    $stream.Dispose()
}

Write-Host "Linked validation properties preserved in $($assembly.FullName)"
