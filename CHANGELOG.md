# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project tries to implement the [Semantic Versioning](https://semver.org/spec/v2.0.0.html) Specification (SemVer).

## [1.0.6] - 2022-01-30
### Fixed
- Portable Heidi is now saved to plugin settings folder by default to keep the sessions.
Due to how the Flow launcher update process works, the portable Heidi-Sessions would be wiped on every plugin update.

## [1.0.5] - 2022-01-21
### Fixed
- Results are now displayed URL decoded (session names are url encoded - iso-8859-1)

### Changed
- Completely removed "default settings" entry from results

## [1.0.4] - 2022-01-21
### Added
- Initial commit to public github repository