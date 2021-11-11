# IIS Failed Request Logs Viewer

A convenient way to view your IIS failed request (FREB) logs.

- [Original source from iis.net](https://www.iis.net/downloads/community/2014/02/freb-viewer) ([archived](https://web.archive.org/web/20211110220449/https://www.iis.net/downloads/community/2014/02/freb-viewer))
- [Microsoft Docs blog post](https://docs.microsoft.com/en-us/archive/blogs/rakkimk/frebviewer-yet-another-freb-files-viewer) ([archived](https://web.archive.org/web/20211110232408/https://docs.microsoft.com/en-us/archive/blogs/rakkimk/frebviewer-yet-another-freb-files-viewer))
- [OneDrive download link](https://onedrive.live.com/?id=D51BD0FEA1143BBD%218588&cid=D51BD0FEA1143BBD) (see the [releases](https://github.com/MatthewL246/IIS-FrebViewer/releases/tag/original) for an archive)

## Description

This program is a useful way to view the failed request tracing logs generated by your IIS server. I mainly use it because it is only possible to view these logs in Internet Explorer due to the XML+XSL format. For security reasony, IE is disabled in newer Windows versions, but this program allows viewing the trace files even when IE is disabled. It is also useful for organizing the logs.

### Decompilation note

Yes, this source code was decompiled from the original program (using the awesome [ILSpy](https://github.com/icsharpcode/ILSpy)) because I could not find the original source code anywhere. This program is over a decade old, and the original developer does not appear to be active any more.

## Original description

> ### Overview
> Failed Request Tracing logs are very frequently looked by a few like me, who live on troubleshooting problems. There were a lot of times, I had to choose the right FREB file for a specific scenario, in 100s of files in the folder. This tool helps you in that situation.
> ### Features
> This Freb Viewer gives you an easy way to select the right file you are looking at, sort by the time-taken to spot the slowest request quickly, search for specific error code, or request text. 'Show the slowness' button is very helpful if you have a very large FREB file, and trying to find where the maximum slowness is.
> ### Benefits
> Provides an easy to use interface to sort the FREB files.
> ### Requirements
> .NET 2.0.
