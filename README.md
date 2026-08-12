# Mrz.NET

ICAO 9303 machine-readable-zone parser and validator for passports and ID cards. Parses TD1, TD2, and TD3 layouts and verifies every check digit against the official 7-3-1 algorithm. Zero external dependencies.

Every passport, national ID card, and visa printed to ICAO Doc 9303 carries two or three lines of OCR-B text at the bottom: the machine-readable zone. It packs the document type, issuing state, holder name, document number, nationality, date of birth, sex, expiry date, and a set of check digits into a handful of fixed-width lines. Reading it correctly is the first step in any KYC or border-control pipeline, and getting the check-digit math wrong is the easiest way to silently accept a forged or mistyped document. On NuGet there was no small, dependency-free, ICAO-verified library for this. Mrz.NET is that library: it embeds the ICAO worked examples as test fixtures and asserts exact field parsing and exact check-digit results against them, including deliberately corrupted input.

## Install

```
dotnet add package Mrz.Net
```

## Quickstart

```csharp
using Mrz;

string passportMrz = """
    P<UTOERIKSSON<<ANNA<MARIA<<<<<<<<<<<<<<<<<<<
    L898902C36UTO7408122F1204159ZE184226B<<<<<10
    """;

MrzDocument document = MrzParser.Parse(passportMrz);

Console.WriteLine($"{document.Surname}, {document.GivenNames}");   // ERIKSSON, ANNA MARIA
Console.WriteLine(document.DocumentNumber);                        // L898902C3
Console.WriteLine(document.Validation.IsValid);                    // True
```

## Checking whether a scan is trustworthy

A scanner, a phone camera, or a human at a counter can all introduce a single wrong character. `MrzDocument.Validation` tells you exactly which field's check digit failed instead of just handing you a broken parse:

```csharp
using Mrz;

if (!MrzParser.TryParse(scannedLines, out MrzDocument? document))
{
    // Structurally not an MRZ: wrong line count, wrong line length, or a
    // character outside the permitted MRZ character set.
    return Reject("Unreadable machine-readable zone.");
}

if (!document!.Validation.IsValid)
{
    if (!document.Validation.DocumentNumberCheckDigitValid)
    {
        return Reject("Document number check digit failed; re-scan the document.");
    }

    if (!document.Validation.CompositeCheckDigitValid)
    {
        return Reject("Composite check digit failed; the document may be tampered with.");
    }
}

Accept(document);
```

## Feeding a KYC screening pipeline

`MrzDocument` gives you the exact surname and given names ICAO 9303 defines, ready to hand to a sanctions or watchlist screen, alongside the raw document identifiers:

```csharp
using Mrz;

MrzDocument document = MrzParser.Parse(idCardLines);

var screeningRequest = new
{
    Surname = document.Surname,
    GivenNames = document.GivenNames,
    Nationality = document.Nationality,
    DateOfBirth = document.DateOfBirth, // "YYMMDD" as printed
    DocumentNumber = document.DocumentNumber,
};

// Pass screeningRequest to a sanctions/PEP screening service, e.g. Sanctions.Net.
```

## What it parses

| Format | Lines | Line length | Typical use |
|---|---|---|---|
| TD1 | 3 | 30 | National ID cards |
| TD2 | 2 | 36 | ID cards and visas |
| TD3 | 2 | 44 | Passports |

`MrzParser.Parse` auto-detects the format from the number and length of lines supplied, so you do not need to tell it which one you have.

Every `MrzDocument` exposes: `DocumentType`, `DocumentCode`, `IssuingState`, `Surname`, `GivenNames`, `IsNameTruncated`, `DocumentNumber`, `Nationality`, `DateOfBirth`, `Sex`, `DateOfExpiry`, `PersonalNumber`, `SupplementalOptionalData` (TD1's second optional data field, next to the document number), the raw `Lines`, and `Validation`.

`DateOfBirth` and `DateOfExpiry` are the six raw digits (`YYMMDD`) exactly as printed. ICAO 9303 does not define how to resolve the two-digit year to a century, so Mrz.NET does not guess; interpret it with whatever domain knowledge you have (a passport application date, an OCR timestamp, a plausible age range).

## Check digits

`MrzCheckDigitCalculator` implements the ICAO 9303 algorithm directly if you need it standalone: every character is converted to a value (digits keep their value, `A`-`Z` map to 10-35, the filler `<` maps to 0), multiplied by a weight that cycles 7, 3, 1 from the first character, summed, and reduced modulo 10.

```csharp
using Mrz;

char checkDigit = MrzCheckDigitCalculator.Compute("L898902C3"); // '6'
bool isValid = MrzCheckDigitCalculator.Verify("L898902C3", '6'); // true
```

`MrzDocument.Validation` runs this for every check-digited field in the document (document number, date of birth, date of expiry, the TD3 personal number, and the composite check digit) and exposes both the per-field results and a single `IsValid` flag. TD1 and TD2 have no independently check-digited personal number field, so `PersonalNumberCheckDigitValid` is `null` for those two formats rather than a misleading `true`.

## Known limitations

- **Extended document number (TD1 and TD2).** ICAO 9303 lets a document number longer than 9 characters overflow into the adjacent optional data field, signaled by a filler character in place of the document number check digit. Mrz.NET does not implement this mechanism for either TD1 or TD2 (the two formats that carry an optional data field next to the document number). Such a document parses, but `DocumentNumber` is truncated to the first 9 characters and `Validation.DocumentNumberCheckDigitValid` is `false` rather than the document being recognized as using extended numbering. TD3 (passports) has no adjacent optional data field, so this mechanism does not apply there.
- **Date of birth accepts filler, date of expiry does not.** ICAO 9303 permits the filler character in date-of-birth positions the issuing authority does not know (for example an approximate birth year for an undocumented minor), so `DateOfBirth` accepts digits or filler and computes/verifies its check digit the same way either way. Date of expiry has no such allowance in the standard and is still validated as strict digits.
- **Sex marker 'X'.** ICAO 9303 itself only defines `M`, `F`, and the filler character for the sex position. Mrz.NET additionally accepts the nonconformant `X` marker some real-world issuers print for an unspecified or non-binary sex, mapping it to `MrzSex.Unspecified` the same as filler. This is a deliberate leniency, not a spec requirement.
- **Composite check digit is always a digit.** Unlike the other check-digit positions, the filler character is never valid in the composite check-digit position; a filler (or any non-digit) there throws `MrzFormatException` as a structural error rather than being accepted and reported as an invalid check digit.

## Why this exists

Machine-readable-zone parsing is a solved problem in principle: it is a fixed-width text format from a public standard. In practice, the .NET ecosystem's NuGet offerings in this space are either unmaintained, undocumented, or get the check-digit weighting or the character-to-value mapping subtly wrong, which is the one place a KYC pipeline cannot afford to be subtly wrong. Mrz.NET verifies its own check-digit math against ICAO's own published worked examples for all three formats, plus deliberately corrupted variants of each, so a build that passes its test suite is a build you can trust with a passport number.

## Zero dependencies, AOT-friendly

Mrz.NET has no runtime dependencies beyond the .NET 8 base class library. It uses no reflection, no dynamic code generation, and no `System.Text.Json` or regular expressions; parsing is plain string and span slicing. It is fully compatible with Native AOT and trimming.

## License

MIT. See [LICENSE](LICENSE).
