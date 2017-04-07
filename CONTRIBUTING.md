# Contributions

Contributions are welcomed into the BuildAMation source tree. Please check the existing Issues before starting on a new piece of work.

The BuildAMation github project contains the core (agnostic dependency generator and utilities) and basic set of packages that all users should find useful. If you find your intended usage is outside of this definition, and you want to extend BuildAMation into new areas, then most likely you will need to create a new package repository. Many of these exist already, e.g. bam-boost, bam-graphicssdks, bam-python, and contributions are welcomed to those as well.

BuildAMation currently has two branches on github:
* **master** - bleeding edge, which will target the next major.minor version.
* **v1.1** - for bug fix contributions to the family of maintenance patch releases of v1.1.
* **v1.0** - for bug fix contributions to the family of maintenance patch releases of v1.0.

A large new feature should be targetted into the master branch.

Any bug fixes should be into maintenance branches initially. These will be merged into master at a future time.

Feel free to fork BuildAMation to make changes, and submit pull requests.

## Requirements
* Please log an issue before making a bug fix. Discussions will be made in the Issue tracker.
    * If there are edge cases or known problems with a new feature, or bug fix, that remain unresolved after committing, please create additional issues so that these can be tracked.
* Please either add a new, or modify an existing, test to exercise your change.
    * If adding a new test, add it to the list of tests to run in tests/bamtests.py, for each valid build mode on each valid platform.
    * Run the tests with a wide-range of applicable toolchain versions.
* Additions should be demonstrated to be compatible with all build modes. There are some exceptions to this rule (e.g. publishing) but these should be discussed in the Issue tracker.
* All changes should be thread-safe where possible. Exercise changes (more than once) with the ```-j=0``` command line option to ensure there are no threading issues.
* For each commit, modify Changelog.txt and reference the issue number. Please provide change descriptions that will be beneficial to other readers. Include a date when the change was made. The format for each line in Changelog.txt is
```
dd-mmm-yyyy <Fixes|Issue> #<number>. <Description of change>
```
* Commit messages use the same text as for the Changelog.txt line, but replace the date with an abbreviation of which part of the source tree was modified as an indication of what changed. Copying and pasting from the Changelog entry is encouraged. For example, here are changes to the Core assembly, and individual packages, in separate commits:
```
[Core] Fixes #123. Added function Bam.Core.foobar that does ...
[packages,C] Fixes #456. Modified the output path for dynamic libraries ...
[packages,Gcc,5.0] Fixes #789. Added C++14 support to ....
```
* Please follow the coding style in existing source files.
* Preference is to use a minimal number of ```using``` statements, so code is explicit about the namespaces for functions. There are some exceptions to this, including binding extension methods. Use of ```var``` for automatic type deduction of variables to reduce statement length.
