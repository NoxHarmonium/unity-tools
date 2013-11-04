NArrange - "An open source tool for arranging .Net source code"

Copyright (c) 2007-2008 James Nies and NArrange contributors.
Zip functionality courtesy of ic#code (Mike Krueger, John Reilly). 

License for this software can be viewed in license.rtf
  

***WARNING***
THIS PROGRAM MODIFIES SOURCE CODE.  BECAUSE IT IS POSSIBLE THAT BUGS EXIST IN THE PROGRAM, IT IS HIGHLY RECOMMENDED THAT YOU CREATE A BACKUP OF YOUR ORIGINAL SOURCE CODE FILES PRIOR TO RUNNING NARRANGE AGAINST THEM.

To ease command line usage, it is also recommended that you add the NArrange bin folder to your %PATH% environment variable.

To setup NArrange as an external tool in Microsoft Visual Studio or for more information on using NArrange, please refer to the documentation included with the binary or source distribution in the Doc directory.  Alternatively, refer the the product homepage for online documentation:
http://narrange.sourceforge.net


The following should help you get started:


ARRANGING FILES
--------------- 

To arrange a file just run the following from a command prompt:

>narrange-console <sourcefile> [optional output file]

NOTE: If an output file is not specified, the original source file will be overwritten. 

Alternatively, you can run NArrange against a C# or VB project file, solution or directory.  
NOTE: When arranging a project or solution, the original source files will be overwritten.


BACKUP
------

To automatically create a backup of source prior to arranging elements, pass the /b backup parameter.  Backup cannot be specified in conjunction with an output file.

To restore a prior backup, pass the /r restore parameter.  When restoring, use the same working directory and path that was used when the backup was created.


CONFIGURATION
-------------

If you don't like the default settings in DefaultConfig.xml you can copy it to a new config and specify the modified configuration file in the command line (see narrange-console help). To ease editing of configuration files, narrange-config.exe can be used.
 
NOTE:  Modifying DefaultConfig.xml will not override settings.  DefaultConfig.xml is provided as an example.  To run with a different configuration, you must specify the configuration file through the /c:configuration command argument.