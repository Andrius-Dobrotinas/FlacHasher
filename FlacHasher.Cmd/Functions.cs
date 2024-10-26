﻿using Andy.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Application.Cmd
{
    public static class Functions
    {
        public static IList<FileInfo> GetInputFiles(MainParameters settings, FileSearch fileSearch)
        {
            if (settings.InputFiles != null)
            {
                return settings.InputFiles
                    .Select(path => new FileInfo(path))
                    .ToArray();
            }
            if (settings.InputDirectory != null)
            {
                var fileExtension = settings.TargetFileExtension;
                if (fileExtension == null)
                    throw new ConfigurationException("Target file extension must be specified when scanning a directory");

                return fileSearch.FindFiles(new DirectoryInfo(settings.InputDirectory), fileExtension)
                    .ToList();
            }

            throw new InputFileMissingException("No input files or directory has been specified");
        }
    }
}