# jira-export

Work item migration tool that assists with moving Jira items to Azure DevOps or TFS.

```txt
Usage: jira-export [options]
```

|Argument|Required|Description|
|---|---|---|
|-? \| -h \| --help|False|Show help information|
|-u \<username>|True|Username for authentication|
|-p \<password>|True|Password for authentication|
|--url \<jiraul>|True|Url of the Jira organization|
|--config \<configurationfilename>|True|Export the work items based on this configuration file|
|--force|False|Force execution from start (instead of continuing from previous run)|

## Example

```bash
jira-export -u myUser -p myPassword --url https://myorganization.atlassian.net --config config.json --force
```
