NUnit3 Summary
==============

**NUnit3 Summary** is a small and simple utility to parse **NUnit3-Console** XML result file to comparable text. 

The output text can be compared _(with previous parsed result)_ using any file comparing tool to see the change since last test run.

Surprisingly there seems not be a tool to simply compare test results.



Usage
-----

```
Usage: NUnit3summary.exe <result-file.xml> [summary-file.txt]
        -or-
       NUnit3summary.exe --console-in [summary-file.txt]
```

Application can be used also with console input/output. You can use `grep` to filter results _(at least for now)_.

Unfortunately the parameter `--console-in` is inevitable in *Mono*.

Current state
-------------

The application currently supports only **NUnit3 Console** result file _(in default state)_ and only three results: *Passed*, *Failed* and *Skipped*.

New functions and features will be added when they will be needed _(by me, or by somebody who needs them)_.

License
-------

The application is released under the [MIT license](License.txt). The license allow the use of **NUnit3 Summary** in free and commercial applications without restrictions. Nevertheless it would be nice to share custom changes with other users.

To do
-----

Add:

* other test result/states,
* filtering _(failed/skipped)_,
* displaying counts of passed/failed/skipped tests  _(overview)_,
* multiple files filtering _(mask, or list of files)_.

