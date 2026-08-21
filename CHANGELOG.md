# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2026-08-21

### Added

- Machine-readable visa support: the `MrzDocumentType.MrvA` (two lines of 44, sharing TD3 geometry) and `MrzDocumentType.MrvB` (two lines of 36, sharing TD2 geometry) formats from ICAO Doc 9303 Part 7. `MrzParser.Parse` auto-detects a visa when the line geometry matches TD3 or TD2 and the document code begins with `V`, routing it to `MrvA` or `MrvB` respectively; passports (`P`) and existing TD1/TD2/TD3 ID paths are unchanged.
- Verified against MRV-A and MRV-B specimens whose document number, date of birth, and date of expiry segments (with their check digits) reuse the ICAO worked example verbatim, plus corruption variants asserting that a corrupted per-field check digit is still caught.

### Changed

- A machine-readable visa carries no overall composite check digit (ICAO Doc 9303 Part 7 defines the trailing line-2 positions as optional data), so the parser no longer computes or validates a composite check digit for `MrvA` and `MrvB`. `MrzValidationResult.CompositeCheckDigitValid` is now `bool?` and is `null` for a visa (as is `PersonalNumberCheckDigitValid`), rather than reporting a misleading composite-check failure on a valid visa. The composite check digit is still computed and validated unchanged for TD1, TD2, and TD3. A visa's optional-data region is exposed raw through `MrzDocument.PersonalNumber`.

## [0.1.0] - 2026-08-12

### Added

- `MrzParser` static API: `Parse(string)`, `Parse(IReadOnlyList<string>)`, and non-throwing `TryParse` overloads of both, auto-detecting the TD1 (3 x 30), TD2 (2 x 36), or TD3 (2 x 44) layout from the lines supplied.
- `MrzDocument` record exposing document type, document code, issuing state, surname, given names, name-truncation flag, document number, nationality, date of birth, sex, date of expiry, personal number, TD1's supplemental optional data field, the raw lines, and a `MrzValidationResult`.
- `MrzCheckDigitCalculator` static API implementing the ICAO 9303 7-3-1 weighted modulus-10 check-digit algorithm: `Compute` and `Verify`, including the ICAO-defined leniency that accepts the filler character in place of a check digit for an entirely unused optional field.
- Full ICAO 9303 character-to-value mapping (digits keep their value, `A`-`Z` map to 10-35, the filler character `<` maps to 0) and per-position charset validation, so malformed input fails with a descriptive `MrzFormatException` instead of a silent wrong parse.
- Per-field check-digit validation (document number, date of birth, date of expiry, TD3 personal number, and the composite check digit) plus a single `IsValid` flag on every parsed document.
- Name field parsing per ICAO 9303: the double-filler separator between primary and secondary identifiers, single fillers resolved to spaces within each identifier, and truncation detection when a name field is filled exactly to capacity.
- Verified against the ICAO Doc 9303 "ERIKSSON, ANNA MARIA" worked example (Parts 4, 5, and 6: TD3, TD1, and TD2 respectively), embedded as fixtures with exact field and check-digit assertions, plus deliberately corrupted variants asserting that only the affected check digit fails.
- Zero runtime dependencies; no reflection, no `System.Text.Json`, no regular expressions; fully Native AOT and trimming compatible.
- SourceLink (GitHub), deterministic CI builds, and `.snupkg` symbol packages.
- Date of birth accepts the filler character alongside digits (unlike date of expiry), matching ICAO 9303's allowance for a partially or entirely unknown birth date.
- The sex marker additionally accepts the nonconformant `X` character observed in some real-world documents, mapped to `MrzSex.Unspecified` alongside the filler character.
- The composite check-digit position now requires a digit; a filler there is rejected with `MrzFormatException` as a structural error instead of being accepted and reported as an invalid check digit.
- README "Known limitations" section documenting the unimplemented ICAO extended document number mechanism for TD1 and TD2, and the other deliberate leniencies above.
