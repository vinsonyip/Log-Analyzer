This log analyzer and parse the log written by any form as long as you defined a specialized log parser to the raw log

The log parser will parse the raw log to InfluxDB readable format, so we can visualize the raw log later

- The recommended logging framework would be the `Serilog`
- The log parser can process log and export the result to 
    1. Txt file (InfluxDB line protocol)
    2. InfluxDB

## Features ##
1. Fatal exception recovery (process log from the last breaking line)
2. Parse more than one log files (Multithreaded)
3. Console app UI (i.e. basic function interaction, progress bar..)

[![CodeQL](https://github.com/vinsonyip/Log-Analyzer/actions/workflows/csharpCodeQLCheck.yml/badge.svg)](https://github.com/vinsonyip/Log-Analyzer/actions/workflows/csharpCodeQLCheck.yml)
