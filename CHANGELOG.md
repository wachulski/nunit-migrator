# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2018-01-25
### Added
- Catch-all analyzer for the rest of issues that do not have fixers yet.
### Changed
- Code action titles (a prefix of [NUnit migration] added).

## [Early alpha development]

* [0.9.0] [2018-01-16] 
    - [Added] Result to ExpectedResult argument analyzer+fixer and Ignore reason analyzer+fixer.
    - [Fixed] Code formatting for existing code fixers.
* [0.8.0] [2018-01-11] [Added]   Deprecated attributes replacing.
* [0.7.0] [2018-01-10] [Added]   MTAThread and STAThread attributes analyzer.
* [0.6.0] [2018-01-09] [Added]   No longer supported attributes analyzer.
* [0.5.0] [2018-01-09] [Added]   TestCaseSource and ValueSource analyzer.
* [0.4.0] [2018-01-09] [Added]   Assertion migrations.
* [0.3.0] [2017-12-14] [Changed] Descriptors IDs shortened. 
* [0.2.0] [2017-12-14] [Added]   Is.InstanceOfType and Is.String* constraint replacements.
* [0.1.7] [2017-12-07] [Added]   Text.* to Does.* constraint replacements.
* [0.1.6] [2017-11-08] [Fixed]   UserMessage in TCs clustered together with ExpectedException.
* [0.1.5] [2017-11-08] [Fixed]   IExpectException and Handler in migrating attribute based exceptions.
* [0.1.4] [2017-11-08] [Fixed]   ExpectedException does not migrate if together with test cases in the original method.
* [0.1.3] [2017-10-31] [Fixed]   Test method naming in attribute expected exception clusters. 
* [0.1.2] [2017-10-26] [Fixed]   Message assertion checks migration to Does.Contain, Does.Match etc.
* [0.1.1] [2017-10-23] [Fixed]   Diagnostic squiggles scope (method identifiers).
* [0.1.0] [2017-10-16] [Added]   Analyzer and fixer for exception related attributes.